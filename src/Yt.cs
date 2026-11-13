using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web.Script.Serialization;

// Песни по ссылке YouTube / YT Music. Качает yt-dlp.exe (github.com/yt-dlp/yt-dlp, Unlicense): рядом с программой, в app\ или в PATH.
// Клип / залив с чужого канала -> сначала песня YT Music (как переключатель «Пісня / Відео»; в клипах интро, вставки, сценки) — Ytm ниже.
// Звук — как есть (m4a, иначе webm/opus): ffmpeg не нужен, декодирует окно. Картинка — превью видео: у YT Music это обложка альбома
// по центру кадра 16:9, окно режет по центру в квадрат. Исполнитель и название — из данных YT Music (у обычного видео — имя файла).
// Одна песня: все качается во временную data\yt\, потом разом в Music\<следующий номер>\ (song.json, обложка, звук последним —
// импорт не увидит половину). song.json хранит id видео ("yt") — по нему плейлист не качает песню второй раз.
// Плейлист / альбом: List() отдает список (быстро, без скачивания), окно качает по одной через Get() — прогресс и «Стоп» в окне.
// Дальше — обычный импорт папки Music (окно: GO и финиш сами).
static class Yt
{
    static readonly object Busy = new object();
    const int TimeoutMs = 4 * 60 * 1000, MaxSec = 20 * 60, MaxList = 300;

    public static string Exe()
    {
        foreach (string p in new[] { Path.Combine(App.Root, "yt-dlp.exe"), Path.Combine(App.AppDir, "yt-dlp.exe") })
            if (File.Exists(p)) return p;
        foreach (string d in (Environment.GetEnvironmentVariable("PATH") ?? "").Split(';'))
        {
            try { string dd = d.Trim().Trim('"'); if (dd != "" && File.Exists(Path.Combine(dd, "yt-dlp.exe"))) return Path.Combine(dd, "yt-dlp.exe"); } catch { }
        }
        return null;
    }

    // только ссылки YouTube: music.youtube.com, www/m.youtube.com, youtu.be
    public static bool IsLink(string url)
    {
        Uri u;
        if (url == null || url.Length > 500 || url.IndexOfAny(new[] { '"', ' ', '\r', '\n', '\t' }) >= 0) return false;
        if (!Uri.TryCreate(url, UriKind.Absolute, out u) || (u.Scheme != "https" && u.Scheme != "http")) return false;
        string h = u.Host.ToLowerInvariant();
        return h == "youtu.be" || h == "youtube.com" || h.EndsWith(".youtube.com");
    }
    // плейлист или альбом (…/playlist?list=…, music.youtube.com/browse/…); watch?v=…&list=… — это одна песня
    static bool IsList(string url)
    {
        string p = new Uri(url).AbsolutePath.ToLowerInvariant();
        return p.StartsWith("/playlist") || p.StartsWith("/browse/");
    }

    // одна песня -> Music\<номер>\. Из плейлиста приходят еще исполнитель, название и длина: если ролик недоступен
    // (блок по стране / удален — YT Music в приложении сам подставляет другую версию), ищем ту же песню поиском YT Music:
    // название совпадает, длина ±4 с (чужую песню с тем же названием фильтр yt-dlp даже не скачает).
    // names = true: artist / title — из списка человека (.txt / таблица, окно «Пісні з YouTube»): пишем их в song.json вместо имен YouTube
    public static Dictionary<string, object> Get(string url, string artist = null, string title = null, double dur = 0, bool names = false)
    {
        url = (url ?? "").Trim();
        if (!IsLink(url)) throw new UserError("ytLink");
        if (IsList(url)) throw new UserError("ytLink");
        string sa = names ? Field(artist) : null, st = names ? Field(title) : null;
        string exe = Exe();
        if (exe == null) throw new UserError("ytNoExe");
        if (!Monitor.TryEnter(Busy)) throw new UserError("busy");
        try
        {
            // песня YT Music вместо клипа (Ytm ниже): нашли — качаем ее; нет / YouTube не ответил / не скачалась — ролик, как раньше
            string vid = VideoId(url);
            if (vid != null)
            {
                Ytm.Info song = null;
                try { song = Ytm.FindSong(vid, title, artist, dur); }
                catch (Exception e) { App.Log("YT Music " + vid + ": " + e.GetType().Name + " " + e.Message + " - downloading the video"); }
                if (song != null && song.Id != vid)
                {
                    try { var r = Download(exe, "https://music.youtube.com/watch?v=" + song.Id, 0, vid, sa, st); r["song"] = true; return r; }
                    catch (UserError e)
                    {
                        if (e.Key == "ytNet") throw;
                        App.Log("YT Music: song " + song.Id + " failed to download (" + e.Key + ") - downloading the video " + vid);
                    }
                }
            }
            try { return Download(exe, url, 0, null, sa, st); }
            catch (UserError e)
            {
                if (e.Key != "ytGone" || string.IsNullOrWhiteSpace(title)) throw;
                string from = VideoId(url);
                foreach (string alt in Alternatives(exe, artist, title))
                {
                    if (alt == from) continue;
                    try { var r = Download(exe, "https://music.youtube.com/watch?v=" + alt, dur, from, sa, st); r["alt"] = true; App.Log("YT replacement " + from + " -> " + alt); return r; }
                    catch (UserError e2) { if (e2.Key != "tooLong" && e2.Key != "ytGone") throw; }   // длина не та / тоже недоступна — следующий
                }
                throw;
            }
        }
        finally { Monitor.Exit(Busy); }
    }

    // поиск YT Music (раздел «песни»): до 3 результатов с тем же названием (без регистра и знаков; «X (feat. Y)» = «X»)
    static List<string> Alternatives(string exe, string artist, string title)
    {
        string q = "https://music.youtube.com/search?q=" + Uri.EscapeDataString(((artist ?? "") + " " + title).Trim()) + "#songs";
        string so, se; int code = Run(exe, "--ignore-config --flat-playlist -J --encoding utf-8 --playlist-items 1-5 \"" + q + "\"", out so, out se);
        var res = new List<string>();
        Dictionary<string, object> pl = null;
        try { pl = App.Json.Deserialize<Dictionary<string, object>>(so); } catch { }
        object ev;
        if (pl == null || !pl.TryGetValue("entries", out ev) || !(ev is IEnumerable)) { App.Log("YT search " + q + " -> code " + code + " " + se.Trim()); return res; }
        string want = Norm(title);
        foreach (var e in ((IEnumerable)ev).OfType<Dictionary<string, object>>())
        {
            string id = Str(e, "id"), t = Norm(Str(e, "title"));
            if (id == null || t.Length < 2) continue;
            if (t == want || (want.Length >= 3 && t.Length >= 3 && (t.StartsWith(want) || want.StartsWith(t)))) res.Add(id);
            if (res.Count >= 3) break;
        }
        App.Log("YT search «" + artist + " - " + title + "»: " + string.Join(", ", res));
        return res;
    }
    static string Norm(string s) { return new string((s ?? "").ToLowerInvariant().Where(char.IsLetterOrDigit).ToArray()); }
    static string VideoId(string url)
    {
        var m = System.Text.RegularExpressions.Regex.Match(url, @"[?&]v=([A-Za-z0-9_-]{6,})");
        if (m.Success) return m.Groups[1].Value;
        m = System.Text.RegularExpressions.Regex.Match(url, @"youtu\.be/([A-Za-z0-9_-]{6,})");
        return m.Success ? m.Groups[1].Value : null;
    }

    // ссылка -> что это: одна песня ({playlist:false}) или список песен плейлиста (id, название, исполнитель, длина, have — уже есть в программе)
    public static Dictionary<string, object> List(string url)
    {
        url = (url ?? "").Trim();
        if (!IsLink(url)) throw new UserError("ytLink");
        if (!IsList(url)) return new Dictionary<string, object> { { "playlist", false } };
        string exe = Exe();
        if (exe == null) throw new UserError("ytNoExe");
        if (!Monitor.TryEnter(Busy)) throw new UserError("busy");
        try
        {
            string so, se; int code = Run(exe, "--ignore-config --flat-playlist -J --encoding utf-8 --playlist-items 1-" + MaxList + " \"" + url + "\"", out so, out se);
            App.Log("YT list " + url + " -> code " + code + (se.Trim() != "" ? "\r\n" + se.Trim() : ""));
            Dictionary<string, object> pl = null;
            try { pl = App.Json.Deserialize<Dictionary<string, object>>(so); } catch { }
            object ev;
            if (pl == null || !pl.TryGetValue("entries", out ev) || !(ev is IEnumerable)) throw new UserError("ytFail", LastError(se, code));
            var have = Have();
            string site = new Uri(url).Host.ToLowerInvariant().StartsWith("music.") ? "https://music.youtube.com/watch?v=" : "https://www.youtube.com/watch?v=";
            var items = new List<object>();
            foreach (var e in ((IEnumerable)ev).OfType<Dictionary<string, object>>())
            {
                string id = Str(e, "id"), title = Str(e, "title");
                if (id == null || title == null) continue;                 // удаленные / приватные видео в плейлисте
                string artist = Str(e, "channel") ?? Str(e, "uploader") ?? "";
                if (artist.EndsWith(" - Topic")) artist = artist.Substring(0, artist.Length - 8);
                object d; double dur = e.TryGetValue("duration", out d) && d != null ? Convert.ToDouble(d) : 0;
                items.Add(new Dictionary<string, object> { { "id", id }, { "url", site + id }, { "title", title }, { "artist", artist }, { "dur", dur },
                                                            { "have", have.Contains(id) || have.Contains(Key(artist, title)) } });
            }
            long free = -1;   // свободно на диске программы (Music + data) — окно сравнит с прикидкой веса
            try { free = new DriveInfo(Path.GetPathRoot(Path.GetFullPath(App.Root))).AvailableFreeSpace; } catch { }
            return new Dictionary<string, object> { { "playlist", true }, { "title", Str(pl, "title") ?? "" }, { "items", items }, { "free", free } };
        }
        finally { Monitor.Exit(Busy); }
    }

    // окно «Пісні з YouTube»: какие ссылки списка уже есть в программе — id ролика (yt / ytFrom в song.json) или исполнитель|название из списка
    public static Dictionary<string, object> HaveList(IEnumerable items)
    {
        var have = Have(); var res = new List<bool>();
        foreach (object o in items ?? new object[0])
        {
            var d = o as Dictionary<string, object>;
            if (d == null) { res.Add(false); continue; }
            string id = VideoId(Str(d, "url") ?? ""), t = Str(d, "title");
            res.Add((id != null && have.Contains(id)) || (t != null && have.Contains(Key(Str(d, "artist"), t))));
        }
        long free = -1;   // свободно на диске программы — окно сравнит с прикидкой веса (как у плейлиста)
        try { free = new DriveInfo(Path.GetPathRoot(Path.GetFullPath(App.Root))).AvailableFreeSpace; } catch { }
        return new Dictionary<string, object> { { "have", res }, { "free", free } };
    }
    // окно «Список з .txt»: файл-пример рядом с программой (Ваня: «файл пример должен быть»; «максимально простой — если хочешь написать, пиши в приложение»):
    // только строки песен в формате Вани, без комментариев, латиница (кириллицу в файлах импорта не используем). Пояснения — в окне программы.
    public const string SampleName = "YouTube-list.txt";
    public static readonly string Sample = string.Join("\r\n", new[] {
        "Bass Inside,AC Slater,https://music.youtube.com/watch?v=m_j8vkVHRl8;",
        "Technique,Minnesota,https://www.youtube.com/watch?v=D0-eCHab-Lo.",   // точка закрывает список (Ваня); читается и без нее
        "" });

    // окно «Пісні з YouTube»: что по ссылке — название, исполнитель, длина (YT Music next, без yt-dlp и без очереди) — показать в списке до скачивания
    // (Ваня: «не понятно, что программа видит по ссылке, просто тупая ссылка»). Нет ответа — found:false, скачивание все равно попробует.
    public static Dictionary<string, object> Peek(string url)
    {
        url = (url ?? "").Trim();
        if (!IsLink(url) || IsList(url)) throw new UserError("ytLink");
        string vid = VideoId(url);
        if (vid == null) throw new UserError("ytLink");
        Ytm.Info v = null;
        try { v = Ytm.Video(vid); } catch (Exception e) { App.Log("YT peek " + vid + ": " + e.GetType().Name + " " + e.Message); }
        if (v == null) return new Dictionary<string, object> { { "id", vid }, { "found", false } };
        return new Dictionary<string, object> { { "id", vid }, { "found", true }, { "title", v.Title ?? "" }, { "by", v.By ?? "" }, { "len", v.Len }, { "type", v.Type ?? "" } };
    }
    // поле из списка человека: пустое — null, длинное — обрезать
    static string Field(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return null;
        s = s.Trim();
        return s.Length > 200 ? s.Substring(0, 200) : s;
    }

    // что уже есть: id видео из song.json папок Music + «исполнитель|название» песен программы
    static HashSet<string> Have()
    {
        var h = new HashSet<string>();
        foreach (var s in Settings.Get().songs.Where(x => !x.draft)) h.Add(Key(s.artist, s.title));
        try
        {
            if (Directory.Exists(App.MusicDir))
                foreach (string d in Directory.GetDirectories(App.MusicDir))
                {
                    var j = App.ReadJson<Dictionary<string, object>>(Path.Combine(d, Music.InfoFile));
                    if (j == null) continue;
                    foreach (string k in new[] { "yt", "ytFrom" }) { string id = Str(j, k); if (id != null) h.Add(id); }   // ytFrom — ролик из плейлиста, вместо которого скачана замена
                }
        }
        catch (Exception e) { App.Log("YT have: " + e.Message); }
        return h;
    }
    // «исполнитель|название» без регистра, пробелов и знаков: «AIN'T SHIT CHANGE» = «Ain't Shit Change»
    static string Key(string artist, string title)
    {
        Func<string, string> n = s => new string((s ?? "").ToLowerInvariant().Where(char.IsLetterOrDigit).ToArray());
        return "\u0001" + n(artist) + "|" + n(title);
    }

    // want > 0 — нужна песня этой длины (±4 с, замена недоступного ролика); from — id ролика, вместо которого качаем замену;
    // setArtist / setTitle — имена из списка человека (важнее YouTube), null — с YouTube
    static Dictionary<string, object> Download(string exe, string url, double want, string from, string setArtist = null, string setTitle = null)
    {
        string tmp = Path.Combine(App.DataDir, "yt");
        try { if (Directory.Exists(tmp)) Directory.Delete(tmp, true); } catch { }
        Directory.CreateDirectory(tmp);
        // одна песня (из ссылки с &list= — только она), без чужих настроек yt-dlp, не стрим и не длиннее 20 минут; файлы song.*
        string filter = "!is_live & duration <=? " + MaxSec;
        if (want > 0) filter += " & duration > " + (int)Math.Floor(want - 4) + " & duration < " + (int)Math.Ceiling(want + 4);
        string args = "--ignore-config --no-playlist --no-progress --encoding utf-8 --match-filters \"" + filter + "\""
                    + " -f \"bestaudio[ext=m4a]/bestaudio\" --write-thumbnail --write-info-json --no-mtime"
                    + " -P \"" + tmp + "\" -o \"song.%(ext)s\" \"" + url + "\"";
        string so, se, outp = ""; int code = 0, attempt = 0;
        for (; attempt < 2; attempt++)
        {
            code = Run(exe, args, out so, out se);
            outp = (so + se).Trim();
            App.Log("YT " + url + " -> code " + code + "\r\n" + outp);
            // 403 от YouTube бывает разовым (проверено: тот же ролик со второго раза качается) — один повтор
            if (code == 0 || !outp.Contains("HTTP Error 403")) break;
            Thread.Sleep(1500);
        }

        string[] got = Directory.GetFiles(tmp, "song.*");
        string audio = got.FirstOrDefault(f => Music.AudioExt.Contains(Path.GetExtension(f).ToLowerInvariant()));
        if (audio == null)
        {
            if (outp.Contains("does not pass filter")) throw new UserError("tooLong");
            if (outp.Contains("Video unavailable")) throw new UserError("ytGone");   // блок по стране / удален
            if (outp.Contains("HTTP Error 403")) throw new UserError("yt403");       // и со второй попытки
            if (new[] { "getaddrinfo failed", "urlopen error", "Unable to download webpage", "timed out", "Connection reset", "No route to host" }.Any(outp.Contains))
                throw new UserError("ytNet");                                       // нет интернета / YouTube не отвечает
            throw new UserError("ytFail", LastError(outp, code));
        }

        // имена: у песни YT Music есть track + artists; у обычного видео — нет, тогда имя файла = название видео («Исполнитель - Песня»)
        var info = App.ReadJson<Dictionary<string, object>>(Path.Combine(tmp, "song.info.json")) ?? new Dictionary<string, object>();
        string track = Str(info, "track"), artist = Names(info, "artists") ?? Str(info, "artist"), id = Str(info, "id");
        if (setArtist != null || setTitle != null)   // имена из списка: заполненное поле — его, пустое — с YouTube
        {
            track = setTitle ?? track ?? Str(info, "title") ?? "song";
            artist = setArtist ?? artist ?? "";
        }
        bool named = track != null && artist != null;
        string full = named ? (artist != "" ? artist + " - " + track : track) : null;
        string name = Safe(full ?? Str(info, "title") ?? "song");

        string dir = Music.NextDir();
        Directory.CreateDirectory(dir);
        var sj = new Dictionary<string, object>();
        if (named) { sj["artist"] = artist; sj["title"] = track; }
        if (id != null) sj["yt"] = id;
        if (from != null && from != id) sj["ytFrom"] = from;
        if (sj.Count > 0) App.WriteJson(Path.Combine(dir, Music.InfoFile), sj);
        string img = got.FirstOrDefault(f => Music.ImgExt.Contains(Path.GetExtension(f).ToLowerInvariant()));
        if (img != null) File.Move(img, Path.Combine(dir, "cover" + Path.GetExtension(img).ToLowerInvariant()));
        string dst = Path.Combine(dir, name + Path.GetExtension(audio).ToLowerInvariant());
        File.Move(audio, dst);
        try { Directory.Delete(tmp, true); } catch { }
        App.Log("YT -> Music\\" + Music.Rel(dst));
        return new Dictionary<string, object> { { "ok", true }, { "name", full ?? name }, { "retried", attempt > 0 } };
    }

    // запуск yt-dlp без окна: stdout и stderr отдельно; не уложился — гасим дерево
    static int Run(string exe, string args, out string stdout, out string stderr)
    {
        var psi = new ProcessStartInfo(exe, args) { UseShellExecute = false, CreateNoWindow = true, WorkingDirectory = App.DataDir,
                                                    RedirectStandardOutput = true, RedirectStandardError = true,
                                                    StandardOutputEncoding = Encoding.UTF8, StandardErrorEncoding = Encoding.UTF8 };
        var o = new StringBuilder(); var e = new StringBuilder();
        using (var p = new Process { StartInfo = psi })
        {
            p.OutputDataReceived += (s, a) => { if (a.Data != null) lock (o) o.AppendLine(a.Data); };
            p.ErrorDataReceived += (s, a) => { if (a.Data != null) lock (e) e.AppendLine(a.Data); };
            p.Start(); p.BeginOutputReadLine(); p.BeginErrorReadLine();
            if (!p.WaitForExit(TimeoutMs)) { Kill(p.Id); App.Log("YT: timed out " + args); throw new UserError("ytFail", "timeout"); }
            p.WaitForExit();
            lock (o) stdout = o.ToString(); lock (e) stderr = e.ToString();
            return p.ExitCode;
        }
    }

    static string Str(Dictionary<string, object> d, string k)
    {
        object v; string s = d.TryGetValue(k, out v) ? v as string : null;
        return string.IsNullOrWhiteSpace(s) ? null : s.Trim();
    }
    static string Names(Dictionary<string, object> d, string k)
    {
        object v; if (!d.TryGetValue(k, out v) || v is string || !(v is IEnumerable)) return null;
        var list = ((IEnumerable)v).OfType<string>().Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Trim()).ToList();
        return list.Count > 0 ? string.Join(", ", list) : null;
    }
    // имя файла для Windows: без запрещенных знаков, не длиннее Music.MaxFile (Ваня 13.11: длинные имена в игре все равно не видно)
    static string Safe(string s)
    {
        var bad = Path.GetInvalidFileNameChars();
        var sb = new StringBuilder();
        foreach (char c in s) sb.Append(bad.Contains(c) || c < ' ' ? '_' : c);
        string r = sb.ToString().Trim();
        if (r.Length > Music.MaxFile) r = r.Substring(0, Music.MaxFile);
        r = r.TrimEnd('.', ' ');
        return r == "" ? "song" : r;
    }
    // строка ERROR от yt-dlp — человеку в окно (без «ERROR: [youtube] id:»)
    static string LastError(string outp, int code)
    {
        string e = outp.Split('\n').Select(l => l.Trim()).LastOrDefault(l => l.StartsWith("ERROR:"));
        if (e == null) return "yt-dlp " + code;
        e = e.Substring(6).Trim();
        int k = e.IndexOf("]");
        if (e.StartsWith("[") && k > 0) { e = e.Substring(k + 1).Trim(); int c = e.IndexOf(": "); if (c > 0 && c < 16 && !e.Substring(0, c).Contains(" ")) e = e.Substring(c + 2); }
        return e.Length > 240 ? e.Substring(0, 240) + "…" : e;
    }
    // yt-dlp.exe — распаковщик + дочерний процесс: гасим дерево по PID
    static void Kill(int pid)
    {
        try { using (var k = Process.Start(new ProcessStartInfo("taskkill", "/T /F /PID " + pid) { UseShellExecute = false, CreateNoWindow = true })) k.WaitForExit(5000); }
        catch (Exception e) { App.Log("YT kill: " + e.Message); }
    }
}

// Песня YT Music вместо клипа — как переключатель «Пісня / Відео» в YT Music. Пару «клип -> песня» YouTube отдает только Premium-аккаунту,
// у людей аккаунта нет, поэтому без входа: тип и имя ролика — youtubei/v1/next, песню ищем поиском YT Music (раздел «Песни», youtubei/v1/search).
// Берем песню, только если: то же название (без «Official Video», «Lyric Video», feat. …), хотя бы один исполнитель общий и каждый исполнитель
// песни упомянут в ролике (иначе «Set Me Free» Martin Garrix & Arcando вместо Dillon Francis). Из версий — ближе по длине к ролику;
// «(Radio Edit)», «(Original Mix)» — только со штрафом 15 с. Длина НЕ фильтр: песня бывает длиннее клипа (Bass Inside: клип 3:08, песня 4:24).
// Не уверены / YouTube поменял ответ / нет сети — null, качается ролик, как раньше (никогда не хуже 1.0.0a).
// Сверено 12.11.2026 с парами Premium-аккаунта Вани (плейлист 195, 172 сверено): 158–159 те же, 3–4 — та же песня с другого релиза
// или версия ближе по длине, 6 не нашлось (качается ролик), чужих песен 0. Прототип и сверка — scratchpad сессии (match_eval5.py).
static class Ytm
{
    public class Info { public string Id, Type, Title, By; public double Len; public List<string> Artists = new List<string>(); }

    const string Client = "1.20261110.04.00";              // версия веб-клиента YT Music (старые версии YouTube принимает долго)
    const string SongsFilter = "EgWKAQIIAWoMEA4QChADEAQQCRAF";   // фильтр поиска «Песни»
    // ответы YT Music глубже 64 уровней (App.Json) — свой разборщик
    static readonly JavaScriptSerializer J = new JavaScriptSerializer { MaxJsonLength = int.MaxValue, RecursionLimit = 1000 };

    static Dictionary<string, object> Post(string ep, Dictionary<string, object> body)
    {
        body["context"] = new Dictionary<string, object> { { "client", new Dictionary<string, object> {
            { "clientName", "WEB_REMIX" }, { "clientVersion", Client }, { "hl", "en" } } } };   // hl=en: имена латиницей, не «Кота Тхе Френд»
        var rq = (HttpWebRequest)WebRequest.Create("https://music.youtube.com/youtubei/v1/" + ep + "?prettyPrint=false");
        rq.Method = "POST"; rq.ContentType = "application/json";
        rq.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/140.0.0.0 Safari/537.36";
        rq.Headers["Origin"] = "https://music.youtube.com";
        rq.Timeout = 15000; rq.ReadWriteTimeout = 15000;
        rq.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
        byte[] b = Encoding.UTF8.GetBytes(J.Serialize(body));
        using (var s = rq.GetRequestStream()) s.Write(b, 0, b.Length);
        using (var rs = (HttpWebResponse)rq.GetResponse())
        using (var sr = new StreamReader(rs.GetResponseStream(), Encoding.UTF8))
            return J.Deserialize<Dictionary<string, object>>(sr.ReadToEnd());
    }

    // все объекты под ключом key (в глубину; внутрь найденного не заходим)
    static IEnumerable<Dictionary<string, object>> Find(object o, string key)
    {
        var d = o as Dictionary<string, object>;
        if (d != null)
        {
            object v;
            if (d.TryGetValue(key, out v)) { var dv = v as Dictionary<string, object>; if (dv != null) yield return dv; yield break; }
            foreach (var x in d.Values) foreach (var y in Find(x, key)) yield return y;
            yield break;
        }
        if (o is string) yield break;
        var e = o as IEnumerable;
        if (e != null) foreach (var x in e) foreach (var y in Find(x, key)) yield return y;
    }
    static object At(object o, params string[] path)
    {
        foreach (string k in path) { var d = o as Dictionary<string, object>; object v; if (d == null || !d.TryGetValue(k, out v)) return null; o = v; }
        return o;
    }
    static List<Dictionary<string, object>> Runs(object o)
    {
        var a = At(o, "runs") as IEnumerable;
        return a == null ? new List<Dictionary<string, object>>() : a.OfType<Dictionary<string, object>>().ToList();
    }
    static string Text(object o) { return string.Concat(Runs(o).Select(r => At(r, "text") as string ?? "")); }
    static double Sec(string s)
    {
        if (string.IsNullOrEmpty(s) || !Regex.IsMatch(s.Trim(), @"^\d+(:\d\d){1,2}$")) return 0;
        double v = 0; foreach (string p in s.Trim().Split(':')) v = v * 60 + int.Parse(p);
        return v;
    }

    // ролик: тип (ATV — песня, OMV — официальный клип, UGC — залив), имя и исполнитель по данным YT Music, длина
    public static Info Video(string vid)
    {
        var d = Post("next", new Dictionary<string, object> { { "videoId", vid }, { "isAudioOnly", true }, { "enablePersistentPlaylistPanel", true } });
        foreach (var p in Find(d, "playlistPanelVideoRenderer"))
        {
            if (At(p, "videoId") as string != vid) continue;
            string type = At(p, "navigationEndpoint", "watchEndpoint", "watchEndpointMusicSupportedConfigs", "watchEndpointMusicConfig", "musicVideoType") as string ?? "";
            string by = Text(At(p, "longBylineText"));
            int k = by.IndexOf(" • "); if (k >= 0) by = by.Substring(0, k);
            return new Info { Id = vid, Type = type.Replace("MUSIC_VIDEO_TYPE_", ""), Title = Text(At(p, "title")), By = by, Len = Sec(Text(At(p, "lengthText"))) };
        }
        return null;
    }

    // поиск YT Music, раздел «Песни»
    public static List<Info> Search(string q, int n)
    {
        var d = Post("search", new Dictionary<string, object> { { "query", q }, { "params", SongsFilter } });
        var res = new List<Info>();
        foreach (var r in Find(d, "musicResponsiveListItemRenderer"))
        {
            var cols = (At(r, "flexColumns") as IEnumerable ?? new object[0]).OfType<Dictionary<string, object>>()
                       .Select(c => At(c, "musicResponsiveListItemFlexColumnRenderer", "text")).ToList();
            if (cols.Count < 2) continue;
            var t = Runs(cols[0]); var meta = Runs(cols[1]);
            string id = At(r, "playlistItemData", "videoId") as string;
            if (id == null) foreach (var x in t) id = At(x, "navigationEndpoint", "watchEndpoint", "videoId") as string ?? id;
            if (id == null) continue;
            var info = new Info { Id = id, Type = "ATV", Title = string.Concat(t.Select(x => At(x, "text") as string)) };
            foreach (var x in meta)
            {
                string pt = At(x, "navigationEndpoint", "browseEndpoint", "browseEndpointContextSupportedConfigs", "browseEndpointContextMusicConfig", "pageType") as string;
                if (pt == "MUSIC_PAGE_TYPE_ARTIST" && At(x, "text") is string) info.Artists.Add((string)At(x, "text"));
            }
            var last = meta.Select(x => At(x, "text") as string).LastOrDefault(s => Sec(s) > 0);
            info.Len = Sec(last);
            res.Add(info);
            if (res.Count >= n) break;
        }
        return res;
    }

    // ---- подбор песни для ролика ----
    static readonly Regex Junk = new Regex(@"\s*[\[(](?:[^\])]*?\b(?:official|lyrics?|audio|video|visuali[sz]er|monstercat|out now|premiere|hd|hq|4k|clip|mv)\b[^\])]*)[\])]", RegexOptions.IgnoreCase);
    static readonly Regex Feat = new Regex(@"\s*[\[(]\s*(?:feat|ft|featuring|with)\b\.?[^\])]*[\])]|\s+\b(?:feat|ft|featuring)\b\.?[^\[\]()]*", RegexOptions.IgnoreCase);
    static readonly Regex EditTag = new Regex(@"\s*[\[(](?:radio edit|original mix|album version|single version|edit)[\])]", RegexOptions.IgnoreCase);
    static readonly Regex ArtSplit = new Regex(@",|&| x | and | vs\.? |\bfeat\.?|\bft\.?|\bwith\b", RegexOptions.IgnoreCase);
    static readonly string[] Seps = { " - ", " – ", " — ", " ~ " };

    // «Ain't Shit» = «AINT SHIT», «A$AP» = «ASAP», «&» = «and»
    static string Norm(string s)
    {
        s = (s ?? "").ToLowerInvariant().Replace("&", "and").Replace("$", "s");
        return new string(s.Where(char.IsLetterOrDigit).ToArray());
    }
    // название без мусора видео и без feat: «Red Lips feat. Sam Bruno (Skrillex Remix) [Official Video]» -> «Red Lips (Skrillex Remix)»
    static string Base(string t)
    {
        t = Junk.Replace(t ?? "", ""); t = Feat.Replace(t, "");
        t = Regex.Replace(t, @"\s*\|.*$", "");                       // «… | OUT NOW»
        return t.Trim(' ', '-', '–', '—', '_');
    }
    static HashSet<string> ArtSet(params string[] xs)
    {
        var h = new HashSet<string>();
        foreach (string s in xs) foreach (string x in ArtSplit.Split(s ?? "")) { string n = Norm(x); if (n.Length >= 2) h.Add(n); }
        return h;
    }
    // «Исполнитель - Название» -> (исполнитель, название); без разделителя — (by, название)
    static void Split(string title, string by, out string art, out string t)
    {
        foreach (string sep in Seps)
        {
            int i = title.IndexOf(sep);
            if (i > 0) { art = title.Substring(0, i).Trim(); t = title.Substring(i + sep.Length).Trim(); return; }
        }
        art = by ?? ""; t = title;
    }

    static Info Pick(string q, string want, HashSet<string> arts, double len, string alltext)
    {
        Info best = null; double bs = double.MaxValue;
        foreach (var r in Search(q, 10))
        {
            var ra = ArtSet(r.Artists.ToArray());
            if (ra.Count == 0 || !ra.Overlaps(arts)) continue;                 // исполнитель обязан совпасть
            if (ra.Any(x => !alltext.Contains(x))) continue;                   // и ни одного чужого исполнителя
            string b = Base(r.Title);
            double sc;
            if (Norm(b) == want) sc = 0;
            else if (Norm(EditTag.Replace(b, "")) == want) sc = 15;           // «(Radio Edit)» — только если точная далеко по длине
            else continue;
            sc += Math.Abs(r.Len - len);
            if (sc < bs) { bs = sc; best = r; }
        }
        return best;
    }

    // песня YT Music для ролика vid или null (ролик сам песня / не нашлось). flatTitle и channel — из плейлиста (yt-dlp), если есть
    public static Info FindSong(string vid, string flatTitle, string channel, double len)
    {
        Info v = Video(vid);                                              // null — ролик недоступен / YouTube ответил иначе
        if (v != null && v.Type == "ATV") { App.Log("YT Music " + vid + ": already a song"); return null; }
        if (len <= 0 && v != null) len = v.Len;
        var tries = new List<string[]>();                                 // { исполнитель, название, by }
        string a, t;
        // 1) исполнитель из названия ролика «A - T»: у клипа «Blood» YT Music пишет авторов-сочинителей вместо Getter & Adair
        if (!string.IsNullOrWhiteSpace(flatTitle) && Seps.Any(flatTitle.Contains)) { Split(flatTitle, "", out a, out t); tries.Add(new[] { a, t, "" }); }
        // 2) данные YT Music (у официального клипа — чистое имя: «Bass Inside», AC Slater)
        if (v != null && !string.IsNullOrWhiteSpace(v.Title)) { Split(v.Title, v.By, out a, out t); tries.Add(new[] { a, t, v.By ?? "" }); }
        // 3) название ролика + канал
        if (!string.IsNullOrWhiteSpace(flatTitle)) { Split(flatTitle, channel, out a, out t); tries.Add(new[] { a, t, "" }); }
        string alltext = Norm(string.Join(" ", flatTitle, channel, v == null ? "" : v.Title, v == null ? "" : v.By));
        var seen = new HashSet<string>();
        foreach (var x in tries)
        {
            string want = Norm(Base(x[1])); var arts = ArtSet(x[0], x[2]);
            string q = (x[0] + " " + Base(x[1])).Trim();
            if (want == "" || arts.Count == 0 || !seen.Add(q)) continue;
            Info r = Pick(q, want, arts, len, alltext);
            if (r != null)
            {
                App.Log("YT Music " + vid + " (" + (v == null ? "?" : v.Type) + " «" + (v == null ? flatTitle : v.Title) + "») -> song " + r.Id + " «" + r.Title + "» "
                        + string.Join(", ", r.Artists) + " " + r.Len + " s (video " + len + " s)");
                return r;
            }
        }
        App.Log("YT Music " + vid + " (" + (v == null ? "?" : v.Type) + " «" + (v == null ? flatTitle : v.Title) + "»): no song - downloading the video");
        return null;
    }
}
