// Окно: сервер на 127.0.0.1 + окно браузера (по умолчанию Edge в режиме приложения, выбор — меню «Браузер», OpenWindow). Окно закрыли — программа выходит.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

class Job
{
    public string id, task, step = "", error, errorArg; public int pct = -1; public bool done; public object result;
    public List<string> lines = new List<string>();
    public Dictionary<string, object> View()
    {
        lock (this) return new Dictionary<string, object> { { "id", id }, { "task", task }, { "step", step }, { "pct", pct }, { "done", done }, { "error", error }, { "errorArg", errorArg }, { "result", result } };
    }
}

static class Ui
{
    static HttpServer srv;
    static DateTime lastPing = DateTime.Now.AddSeconds(60);
    static DateTime? byeAt;
    static volatile bool quit;
    static Job job;
    static Dictionary<string, string[]> meta;
    static Dictionary<string, object> previews;                       // слот -> {d: id Deezer} | {i: ссылка iTunes}
    static readonly Dictionary<string, Tuple<string, DateTime>> previewCache = new Dictionary<string, Tuple<string, DateTime>>();

    // 30-секундное превью: Deezer отдает ссылку на звук, которая живет недолго, — берем свежую (кэш 10 мин)
    static string PreviewUrl(string slot)
    {
        object o; if (slot == null || !previews.TryGetValue(slot, out o)) return null;
        var d = o as Dictionary<string, object>; if (d == null) return null;
        if (d.ContainsKey("i")) return d["i"] as string;
        lock (previewCache)
        {
            Tuple<string, DateTime> c;
            if (previewCache.TryGetValue(slot, out c) && (DateTime.Now - c.Item2).TotalMinutes < 10) return c.Item1;
        }
        System.Net.ServicePointManager.SecurityProtocol |= System.Net.SecurityProtocolType.Tls12;
        using (var wc = new System.Net.WebClient { Encoding = Encoding.UTF8 })
        {
            wc.Headers["User-Agent"] = "UnboundSoundtrack/" + App.Version;
            var j = App.Json.Deserialize<Dictionary<string, object>>(wc.DownloadString("https://api.deezer.com/track/" + Convert.ToInt64(d["d"])));
            string url = j != null && j.ContainsKey("preview") ? j["preview"] as string : null;
            if (string.IsNullOrEmpty(url)) return null;
            lock (previewCache) previewCache[slot] = Tuple.Create(url, DateTime.Now);
            return url;
        }
    }

    public static int Run(string[] args)
    {
        try
        {
            Directory.CreateDirectory(App.DataDir);
            File.WriteAllText(Path.Combine(App.DataDir, ".write-test"), "ok"); File.Delete(Path.Combine(App.DataDir, ".write-test"));
        }
        catch (Exception e)
        {
            System.Windows.Forms.MessageBox.Show("Unbound Soundtrack can't save files in this folder:\n" + App.Root +
                "\n\nMove the whole \"Unbound Soundtrack\" folder somewhere like Desktop or Documents and start it again.\n\n(" + e.Message + ")", "Unbound Soundtrack");
            return 1;
        }
        // запуск прямо из архива: Проводник достает один exe (нет app\), WinRAR/7-Zip — всё во временную папку (настройки и песни потом пропадут)
        // Проводник Windows 10: Temp1_<имя>.zip; новый Проводник: <GUID>_<имя>.zip.<hex> (R1-08); WinRAR: Rar$…; 7-Zip: 7zXXXX
        bool fromArchive = System.Text.RegularExpressions.Regex.IsMatch(App.Root + "\\", @"\\(Temp\d+_[^\\]*\.zip|[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}_[^\\]*\.zip\.[0-9a-f]+|Rar\$[^\\]*|7z[A-Z][^\\]*)\\", System.Text.RegularExpressions.RegexOptions.IgnoreCase)
                           && App.Root.StartsWith(Path.GetTempPath(), StringComparison.OrdinalIgnoreCase);
        if (fromArchive || !Directory.Exists(App.WebDir) || !Directory.Exists(App.FrostyDir) || !Directory.Exists(App.EncDir))
        {
            App.Log("not started from an extracted folder: " + App.Root);
            System.Windows.Forms.MessageBox.Show(UnpackText(), "Unbound Soundtrack", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
            return 1;
        }
        bool created;
        var mutex = new Mutex(true, "UnboundSoundtrack-" + (App.Root.ToLowerInvariant().GetHashCode() & 0x7fffffff), out created);
        if (!created)
        {
            try { int port = int.Parse(File.ReadAllText(Path.Combine(App.DataDir, "port.txt")).Trim()); OpenWindow("http://127.0.0.1:" + port + "/"); } catch { }
            return 0;
        }
        App.Log("=== start " + App.Version + ", folder " + App.Root + ", Windows " + App.WinVersion());
        CleanDrafts();
        try { Directory.CreateDirectory(App.MusicDir); } catch (Exception e) { App.Log("Music: " + e.Message); }
        meta =new Dictionary<string, string[]>();
        var rawMeta = App.ReadJson<Dictionary<string, object>>(Path.Combine(App.WebDir, "tracks_meta.json"));
        if (rawMeta != null)
            foreach (var kv in rawMeta)
            {
                var arr = kv.Value as System.Collections.ArrayList;   // "_source" и прочие служебные — строки, пропускаем
                if (arr != null && arr.Count >= 2) meta[kv.Key] = new[] { Convert.ToString(arr[0]), Convert.ToString(arr[1]) };
            }
        previews = App.ReadJson<Dictionary<string, object>>(Path.Combine(App.WebDir, "previews.json")) ?? new Dictionary<string, object>();
        new Thread(() => { try { Find.Game(); Find.Fmm(); } catch (Exception e) { App.Log("search: " + e); } }) { IsBackground = true }.Start();

        srv = new HttpServer(Handle);
        File.WriteAllText(Path.Combine(App.DataDir, "port.txt"), srv.Port.ToString());
        new Thread(srv.Serve) { IsBackground = true }.Start();
        string url = "http://127.0.0.1:" + srv.Port + "/";
        App.Log("server " + url);
        if (Array.IndexOf(args, "--no-window") < 0) OpenWindow(url);   // --no-window — для проверки без окна
        // Процесс браузера, который мы запустили, может сразу передать окно другому своему процессу и выйти —
        // поэтому закрытие окна узнаем от страницы (pagehide -> /api/bye), а запасной путь — тишина + нет браузера с нашим профилем.
        DateTime lastEdgeCheck = DateTime.MinValue;
        while (!quit)
        {
            Thread.Sleep(1000);
            if (byeAt.HasValue && (DateTime.Now - byeAt.Value).TotalSeconds > 6 && lastPing < byeAt.Value) { App.Log("window closed"); break; }
            if (job != null && !job.done) continue;                              // идет работа — ждем
            if ((DateTime.Now - lastPing).TotalMinutes > 3 && (DateTime.Now - lastEdgeCheck).TotalSeconds > 30)
            {
                lastEdgeCheck = DateTime.Now;
                if (!WindowAlive()) { App.Log("no window (no contact for 3 min, no browser with our profile)"); break; }
            }
            if ((DateTime.Now - lastPing).TotalHours > 6) { App.Log("no contact with the window for 6 h"); break; }
        }
        // Ваня 13.11: вышел из программы — все останавливается, сборка в фоне не доделывается (рабочий процесс и сам следит за нами — Worker.WatchParent)
        if (job != null && !job.done && jobProc != null) { jobStop = true; App.Log("task " + job.task + ": stopped - program closed"); KillTree(jobProc.Id); }
        srv.Stop();
        App.Log("exit");
        GC.KeepAlive(mutex);
        return 0;
    }

    // языки окна (те же коды, что в web/i18n.js LANGS)
    static readonly string[] Langs = { "uk", "en", "de", "es", "fr", "it", "pl", "pt", "tr", "ru", "zh", "ja", "ko", "fi" };   // fi — 13.11, 1.0.2 (Ваня)

    // «распакуй zip» на языке Windows (окна еще нет — язык программы неизвестен)
    static string UnpackText()
    {
        switch (System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName)
        {
            case "ru": return "Программа запущена прямо из архива.\n\nСначала распакуй ВЕСЬ zip в обычную папку (Рабочий стол, Документы, D:\\Mods…), а потом запусти \"Unbound Soundtrack.exe\" оттуда.\nИначе настройки и песни пропадут.";
            case "es": return "El programa se abrió directamente desde el archivo zip.\n\nPrimero descomprime TODO el zip en una carpeta normal (Escritorio, Documentos, D:\\Mods…) y luego abre \"Unbound Soundtrack.exe\" desde allí.\nSi no, se perderán los ajustes y las canciones.";
            case "fr": return "Le programme a été lancé directement depuis l'archive.\n\nDécompresse d'abord TOUT le zip dans un dossier normal (Bureau, Documents, D:\\Mods…), puis lance \"Unbound Soundtrack.exe\" depuis ce dossier.\nSinon, les réglages et les chansons seront perdus.";
            case "it": return "Il programma è stato avviato direttamente dall'archivio.\n\nPrima estrai TUTTO lo zip in una cartella normale (Desktop, Documenti, D:\\Mods…), poi avvia \"Unbound Soundtrack.exe\" da lì.\nAltrimenti impostazioni e canzoni andranno perse.";
            case "tr": return "Program doğrudan arşivin içinden başlatıldı.\n\nÖnce zip'in TAMAMINI normal bir klasöre çıkar (Masaüstü, Belgeler, D:\\Mods…), sonra \"Unbound Soundtrack.exe\" dosyasını oradan başlat.\nAksi halde ayarlar ve şarkılar kaybolur.";
            case "zh": return "程序是直接从压缩包里启动的。\n\n请先把整个 zip 解压到普通文件夹（桌面、文档、D:\\Mods…），再从那里启动 \"Unbound Soundtrack.exe\"。\n否则设置和歌曲会丢失。";
            case "ja": return "プログラムが zip の中から直接起動されました。\n\nまず zip をすべて普通のフォルダー（デスクトップ、ドキュメント、D:\\Mods…）に展開してから、そこで \"Unbound Soundtrack.exe\" を起動してください。\nそうしないと設定と曲が失われます。";
            case "ko": return "프로그램이 압축 파일 안에서 바로 실행되었습니다.\n\n먼저 zip 전체를 일반 폴더(바탕 화면, 문서, D:\\Mods…)에 압축 해제한 다음, 거기서 \"Unbound Soundtrack.exe\"를 실행하세요.\n그렇지 않으면 설정과 노래가 사라집니다.";
            case "uk": return "Програму запущено прямо з архіву.\n\nСпочатку розпакуй УВЕСЬ zip у звичайну папку (Робочий стіл, Документи, D:\\Mods…), а потім запусти \"Unbound Soundtrack.exe\" звідти.\nІнакше налаштування й пісні пропадуть.";
            case "pl": return "Program uruchomiono prosto z archiwum.\n\nNajpierw rozpakuj CAŁY zip do zwykłego folderu (Pulpit, Dokumenty, D:\\Mods…), a potem uruchom stamtąd \"Unbound Soundtrack.exe\".\nInaczej ustawienia i piosenki przepadną.";
            case "de": return "Das Programm wurde direkt aus dem Archiv gestartet.\n\nEntpacke zuerst das GANZE Zip in einen normalen Ordner (Desktop, Dokumente, D:\\Mods…) und starte \"Unbound Soundtrack.exe\" von dort.\nSonst gehen Einstellungen und Songs verloren.";
            case "pt": return "O programa foi aberto direto do arquivo zip.\n\nPrimeiro descompacte o zip INTEIRO numa pasta comum (Área de Trabalho, Documentos, D:\\Mods…) e abra \"Unbound Soundtrack.exe\" de lá.\nSenão as configurações e músicas se perdem.";
            case "fi": return "Ohjelma käynnistettiin suoraan arkistosta.\n\nPura ensin KOKO zip tavalliseen kansioon (Työpöytä, Tiedostot, D:\\Mods…) ja käynnistä \"Unbound Soundtrack.exe\" sieltä.\nMuuten asetukset ja biisit katoavat.";
            default: return "The program was started right from the archive.\n\nFirst unpack the WHOLE zip to a normal folder (Desktop, Documents, D:\\Mods…), then start \"Unbound Soundtrack.exe\" from there.\nOtherwise your settings and songs will be lost.";
        }
    }

    // Edge предлагает «Перевести страницу?» (язык окна != язык Windows); флаг --disable-features=Translate он не слушает —
    // выключаем перевод в настройках нашего профиля (translate.enabled = false) до запуска окна
    static void NoTranslate(string prof)
    {
        try
        {
            string f = Path.Combine(prof, "Default", "Preferences");
            var d = File.Exists(f) ? App.ReadJson<Dictionary<string, object>>(f) : new Dictionary<string, object>();
            if (d == null) return;                                   // не прочитался — чужой файл не перезаписываем
            object o; var tr = d.TryGetValue("translate", out o) ? o as Dictionary<string, object> : null;
            if (tr == null) d["translate"] = tr = new Dictionary<string, object>();
            if (tr.TryGetValue("enabled", out o) && Equals(o, false)) return;
            tr["enabled"] = false;
            App.WriteJson(f, d);
        }
        catch (Exception e) { App.Log("edge, translate setting: " + e.Message); }
    }

    // ---------- окно: в каком браузере (Ваня 13.11: выбор в меню «Браузер», по умолчанию Edge) ----------
    // Chromium-браузеры (Edge, Chrome, Brave, Vivaldi, Яндекс…) — окном без адресной строки (--app) со своим профилем в data\;
    // остальные (Firefox, Opera…) и «браузер по умолчанию» — обычной вкладкой: закрытие узнаем только от страницы (pagehide / ping)
    static readonly string[] AppModeExe = { "msedge.exe", "chrome.exe", "brave.exe", "vivaldi.exe", "browser.exe", "chromium.exe", "thorium.exe" };
    static string winExe, winProfile;   // чем открыто окно; winProfile == null — вкладка (проверить процесс нельзя)

    static string EdgePath()
    {
        foreach (string p in new[] { @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe", @"C:\Program Files\Microsoft\Edge\Application\msedge.exe",
                                     Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Microsoft\Edge\Application\msedge.exe") })
            if (File.Exists(p)) return p;
        return null;
    }
    static bool IsEdge(string exe) { return Path.GetFileName(exe).Equals("msedge.exe", StringComparison.OrdinalIgnoreCase); }
    static bool AppMode(string exe) { return AppModeExe.Contains(Path.GetFileName(exe).ToLowerInvariant()); }
    static string ExeOf(string cmd)
    {
        if (string.IsNullOrWhiteSpace(cmd)) return null;
        cmd = Environment.ExpandEnvironmentVariables(cmd.Trim());
        if (cmd.StartsWith("\"")) { int q = cmd.IndexOf('"', 1); return q > 1 ? cmd.Substring(1, q - 1) : null; }
        int i = cmd.IndexOf(".exe", StringComparison.OrdinalIgnoreCase);
        return i > 0 ? cmd.Substring(0, i + 4) : null;
    }

    // установленные браузеры — Windows хранит их в Clients\StartMenuInternet (HKCU и HKLM): [путь exe, имя]; Edge первым
    static List<string[]> Browsers()
    {
        var res = new List<string[]>();
        foreach (var k in new[] { Tuple.Create(Microsoft.Win32.Registry.CurrentUser, @"SOFTWARE\Clients\StartMenuInternet"),
                                  Tuple.Create(Microsoft.Win32.Registry.LocalMachine, @"SOFTWARE\Clients\StartMenuInternet"),
                                  Tuple.Create(Microsoft.Win32.Registry.LocalMachine, @"SOFTWARE\WOW6432Node\Clients\StartMenuInternet") })
            try
            {
                using (var rk = k.Item1.OpenSubKey(k.Item2))
                {
                    if (rk == null) continue;
                    foreach (string sub in rk.GetSubKeyNames())
                        using (var sk = rk.OpenSubKey(sub))
                        using (var ck = sk == null ? null : sk.OpenSubKey(@"shell\open\command"))
                        {
                            string exe = ExeOf(ck == null ? null : ck.GetValue("") as string);
                            if (exe == null || !File.Exists(exe) || res.Any(b => string.Equals(b[0], exe, StringComparison.OrdinalIgnoreCase))) continue;
                            if (Path.GetFileName(exe).Equals("iexplore.exe", StringComparison.OrdinalIgnoreCase)) continue;   // IE окно программы не потянет
                            string name = sk.GetValue("") as string;
                            res.Add(new[] { exe, string.IsNullOrWhiteSpace(name) ? sub : name.Trim() });
                        }
                }
            }
            catch (Exception e) { App.Log("browsers: " + e.Message); }
        string edge = EdgePath();
        if (edge != null && !res.Any(b => IsEdge(b[0]))) res.Add(new[] { edge, "Microsoft Edge" });
        return res.OrderBy(b => IsEdge(b[0]) ? 0 : 1).ThenBy(b => b[1], StringComparer.OrdinalIgnoreCase).ToList();
    }

    static void OpenWindow(string url)
    {
        string want = Settings.Get().browser, exe = want == "default" ? null : want;
        if (exe != null && !File.Exists(exe)) { App.Log("browser not found: " + exe + " - using Edge"); exe = null; want = null; }
        if (want == null) exe = EdgePath() ?? Browsers().Select(b => b[0]).FirstOrDefault(AppMode);   // нет Edge — любой Chromium
        winExe = exe; winProfile = null;
        if (exe != null)
            try
            {
                if (AppMode(exe))
                {
                    string prof = Path.Combine(App.DataDir, IsEdge(exe) ? "edge" : "browser-" + Path.GetFileNameWithoutExtension(exe).ToLowerInvariant());
                    NoTranslate(prof);
                    // окно не больше рабочей области экрана (R1-06: 1440x900 уходило под панель задач на 1366x768 / 125%), по центру
                    var wa = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea;
                    int w = Math.Max(800, Math.Min(1440, wa.Width - 40)), h = Math.Max(560, Math.Min(900, wa.Height - 30));
                    int x = wa.Left + Math.Max(0, (wa.Width - w) / 2), y = wa.Top + Math.Max(0, (wa.Height - h) / 2);
                    Process.Start(new ProcessStartInfo(exe, "--app=" + url + " --user-data-dir=\"" + prof + "\" --no-first-run --no-default-browser-check --disable-features=Translate" +
                        " --window-size=" + w + "," + h + " --window-position=" + x + "," + y) { UseShellExecute = false });
                    winProfile = prof;
                }
                else Process.Start(new ProcessStartInfo(exe, (Path.GetFileName(exe).Equals("firefox.exe", StringComparison.OrdinalIgnoreCase) ? "-new-window " : "") + url) { UseShellExecute = false });
                App.Log("window: " + exe + (winProfile != null ? " (app window)" : " (tab)"));
                return;
            }
            catch (Exception e) { App.Log("browser " + exe + ": " + e.Message); }
        winExe = null;
        App.Log(want == "default" ? "window: default browser" : "no Edge / Chromium browser - opening the default browser");
        try { Process.Start(url); } catch (Exception e) { App.Log("browser: " + e.Message); }
    }

    // окно еще открыто? Только для окна --app (свой профиль в командной строке); вкладку так не найти — там решает ping
    static bool WindowAlive()
    {
        if (winProfile == null || winExe == null) return false;
        try
        {
            string prof = winProfile.ToLowerInvariant();
            using (var q = new System.Management.ManagementObjectSearcher("SELECT CommandLine FROM Win32_Process WHERE Name = '" + Path.GetFileName(winExe).Replace("'", "") + "'"))
                foreach (System.Management.ManagementObject o in q.Get())
                {
                    string cl = o["CommandLine"] as string;
                    if (cl != null && cl.ToLowerInvariant().Contains(prof)) return true;
                }
            return false;
        }
        catch (Exception e) { App.Log("window check: " + e.Message); return true; }   // не смогли проверить — не выходим
    }

    static void CleanDrafts()
    {
        Settings.Edit(s =>
        {
            foreach (var d in s.songs.Where(x => x.draft).ToList()) { DropSong(d.id); s.songs.Remove(d); }
            if (Directory.Exists(App.SongsDir))
                foreach (string dir in Directory.GetDirectories(App.SongsDir))
                    if (!s.songs.Any(x => x.id == Path.GetFileName(dir))) DropSong(Path.GetFileName(dir));
        });
    }
    static void DropSong(string id)
    {
        if (string.IsNullOrEmpty(id) || id.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 || id.Contains("..")) return;
        try { string d = Path.Combine(App.SongsDir, id); if (Directory.Exists(d)) Directory.Delete(d, true); } catch (Exception e) { App.Log("delete song " + id + ": " + e.Message); }
    }

    // ---------- состояние для окна ----------
    static Dictionary<string, object> State()
    {
        var st = Settings.Get();
        string game = Find.IsGame(st.gamePath) ? st.gamePath : null, fmm = Find.IsFmm(st.fmmPath) ? st.fmmPath : null;
        var tj = App.ReadJson<Dictionary<string, object>>(Path.Combine(App.UiDir, "tracks.json"));
        bool ready = tj != null && tj.ContainsKey("tracks");
        bool stale = !ready || (game != null && (tj["fingerprint"] as string) != Find.Fingerprint(game)) || (tj.ContainsKey("app") && (tj["app"] as string) != App.Version);
        var tracks = new List<object>();
        if (ready)
            foreach (Dictionary<string, object> t in (System.Collections.ArrayList)tj["tracks"])
            {
                string slot = (string)t["slot"];
                string[] m; meta.TryGetValue(slot, out m);
                bool[] on; if (!st.choice.TryGetValue(slot, out on) || on == null || on.Length < 2) on = new[] { true, true };
                on = new[] { on[0], on[1], on.Length > 2 && on[2], on.Length > 3 ? on[3] : on[1] };   // окну всегда [гонки, город, low, гараж]
                tracks.Add(new Dictionary<string, object> {
                    { "slot", slot }, { "artist", m != null ? m[0] : "" }, { "title", m != null ? m[1] : slot },
                    { "cover", File.Exists(Path.Combine(App.UiDir, "covers", slot + ".jpg")) ? "/ui/covers/" + slot + ".jpg" : null },
                    { "preview", previews.ContainsKey(slot) },
                    { "on", on } });
            }
        var bg = new Dictionary<string, object>();
        foreach (string f in new[] { "photo.jpg", "leak.jpg", "logo.png", "halftone.png", "grunge.png", "dots.png", "noise.png" })
            if (File.Exists(Path.Combine(App.UiDir, "bg", f))) bg[Path.GetFileNameWithoutExtension(f)] = "/ui/bg/" + f;
        return new Dictionary<string, object> {
            { "version", App.Version }, { "lang", st.lang }, { "agreed", st.agreed }, { "pathsOk", st.pathsOk }, { "musicTold", st.musicTold },
            { "game", game }, { "fmm", fmm }, { "key", fmm != null && File.Exists(Find.KeyPath(fmm)) },
            { "ready", ready }, { "stale", stale }, { "tracks", tracks }, { "bg", bg },
            { "songs", Music.Ordered(st.songs.Where(s => !s.draft)).Select(SongView).ToList() },
            { "job", job == null ? null : job.View() }, { "lastBuild", st.lastBuild }, { "view", st.view },
            { "presets", st.presets.Select(p => new Dictionary<string, object> { { "id", p.id }, { "name", p.name } }).ToList() }, { "active", st.activePreset } };
    }

    static Dictionary<string, object> SongView(Song s)
    {
        return new Dictionary<string, object> {
            { "id", s.id }, { "title", s.title }, { "artist", s.artist }, { "file", s.file }, { "dur", s.dur }, { "drop", s.drop }, { "outro", s.outro }, { "cDur", s.cDur },
            { "race", s.race }, { "city", s.city }, { "garage", s.Gar() }, { "low", s.low }, { "added", s.added }, { "audio", "/songs/" + s.id + "/song.wav" }, { "src", s.src },
            { "cover", s.cover ? "/songs/" + s.id + "/cover.png?v=" + s.added : null } };
    }

    // ---------- папка Music: пропавшие песни удаляем, новые и измененные отдаем окну на импорт ----------
    static Dictionary<string, object> MusicScan()
    {
        var found = Music.Scan();
        var have = new HashSet<string>(found.Select(e => e.src), StringComparer.OrdinalIgnoreCase);
        var gone = new List<string>();
        var st = Settings.Get();
        if (st.songs.Any(s => s.src != null && !s.draft && !have.Contains(s.src)) || st.skipped.Any(k => !have.Contains(k)))
            Settings.Edit(x =>
            {
                // папку переименовали (порядок): тот же звук (размер и время файла) под новым путем — та же песня, GO и флаги сохраняются
                var used = new HashSet<string>(x.songs.Where(s => s.src != null).Select(s => s.src), StringComparer.OrdinalIgnoreCase);
                foreach (var s in x.songs.Where(s => s.src != null && !s.draft && !have.Contains(s.src)).ToList())
                {
                    var e = found.FirstOrDefault(f => !used.Contains(f.src) && f.asig == s.asig && s.asig != "");
                    if (e != null) { App.Log("Music: " + s.src + " -> " + e.src); s.src = e.src; s.isig = e.isig; used.Add(e.src); continue; }
                    x.songs.Remove(s); gone.Add(s.id);
                }
                x.skipped.RemoveAll(k => !have.Contains(k));
            });
        foreach (string id in gone) DropSong(id);
        if (gone.Count > 0) App.Log("Music: songs removed (missing from the folder): " + gone.Count);
        var todo = new List<object>();
        foreach (var e in found)
        {
            if (st.skipped.Contains(e.src, StringComparer.OrdinalIgnoreCase)) continue;
            var s = st.songs.FirstOrDefault(y => !y.draft && y.src != null && string.Equals(y.src, e.src, StringComparison.OrdinalIgnoreCase));
            string kind = s == null || s.asig != e.asig ? "new" : s.isig != e.isig ? "cover" : null;   // звук поменяли — заново, картинку — только обложку
            if (kind == null) continue;
            todo.Add(new Dictionary<string, object> { { "kind", kind }, { "id", s == null ? null : s.id }, { "src", e.src }, { "name", e.name }, { "image", e.image },
                { "asig", e.asig }, { "isig", e.isig }, { "title", s == null ? null : s.title }, { "artist", s == null ? null : s.artist }, { "info", e.info } });
        }
        return new Dictionary<string, object> { { "todo", todo }, { "gone", gone.Count } };
    }

    // ---------- задания (рабочий процесс) ----------
    static Process jobProc;            // рабочий процесс текущего задания
    static volatile bool jobStop;      // остановили сами («Стоп» / вышли из программы) — это не ошибка
    // рабочий процесс + его кодировщик (Tool.exe) — деревом по PID
    static void KillTree(int pid)
    {
        try { using (var k = Process.Start(new ProcessStartInfo("taskkill", "/T /F /PID " + pid) { UseShellExecute = false, CreateNoWindow = true })) k.WaitForExit(5000); }
        catch (Exception e) { App.Log("kill worker: " + e.Message); }
    }

    static Job StartJob(string task, Dictionary<string, object> data)
    {
        lock (typeof(Ui))
        {
            if (job != null && !job.done) throw new UserError("busy");
            // P2-05: рабочий процесс от прошлого запуска (окно закрыли посреди сборки) еще работает — второй поверх него упадет на A.sek.tmp
            Mutex old;
            if (Mutex.TryOpenExisting(Worker.MutexName, out old)) { old.Dispose(); App.Log("previous worker still running"); throw new UserError("busyOld"); }
            var j = new Job { id = DateTime.Now.Ticks.ToString(), task = task };
            data["task"] = task;
            data["parent"] = Process.GetCurrentProcess().Id;   // рабочий процесс выходит вместе с программой (Worker.WatchParent)
            string jp = Path.Combine(App.DataDir, "job.json");
            App.WriteJson(jp, data);
            var psi = new ProcessStartInfo(System.Reflection.Assembly.GetExecutingAssembly().Location, "--worker \"" + jp + "\"")
            { UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, StandardOutputEncoding = Encoding.UTF8, WorkingDirectory = App.Root };
            var p = Process.Start(psi);
            job = j; jobProc = p; jobStop = false;
            new Thread(() =>
            {
                string line;
                while ((line = p.StandardOutput.ReadLine()) != null)
                {
                    lock (j)
                    {
                        j.lines.Add(line);
                        if (line.StartsWith("STEP ")) { string ns = line.Substring(5); if (ns.Split(' ')[0] != (j.step ?? "").Split(' ')[0]) j.pct = -1; j.step = ns; }   // тот же шаг (следующая песня) — полосу не сбрасываем
                        else if (line.StartsWith("PCT ")) { int v; if (int.TryParse(line.Substring(4), out v)) j.pct = v; }
                        else if (line.StartsWith("ERR ")) { string r = line.Substring(4); int sp = r.IndexOf(' '); j.error = sp > 0 ? r.Substring(0, sp) : r; j.errorArg = sp > 0 ? r.Substring(sp + 1) : null; }
                        else if (line.StartsWith("OK ")) { try { j.result = App.Json.DeserializeObject(line.Substring(3)); } catch { j.result = line.Substring(3); } }
                    }
                }
                p.WaitForExit();
                lock (j)
                {
                    if (j.result == null && j.error == null) { if (jobStop) j.error = "stopped"; else { j.error = "crash"; j.errorArg = "exit " + p.ExitCode; } }
                    j.done = true;
                }
                App.Log("task " + task + " done: " + (j.error ?? "ok"));
                if (task == "build" && j.error == null) Settings.Edit(s => s.lastBuild = (string)((Dictionary<string, object>)j.result)["path"]);
            }) { IsBackground = true }.Start();
            return j;
        }
    }

    // ---------- запросы ----------
    static void Handle(Req r, Resp w)
    {
        try
        {
            if (r.Path.StartsWith("/api/")) { Api(r, w); return; }
            string file = null;
            if (r.Path == "/" || r.Path == "/index.html") file = Path.Combine(App.WebDir, "index.html");
            else if (r.Path.StartsWith("/ui/")) file = Safe(App.UiDir, r.Path.Substring(4));
            else if (r.Path.StartsWith("/songs/"))
            {
                string rest = r.Path.Substring(7); string name = Path.GetFileName(rest);
                if (name == "song.wav" || name == "cover.png") file = Safe(App.SongsDir, rest);
            }
            else file = Safe(App.WebDir, r.Path.TrimStart('/'));
            if (file != null) w.File(file, r);
        }
        catch (UserError e) { w.Json(new Dictionary<string, object> { { "error", e.Key }, { "errorArg", e.Arg } }, 400); }
        catch (Exception e) { App.Log("request " + r.Path + ": " + e); if (!w.Sent) w.Json(new Dictionary<string, object> { { "error", "crash" }, { "errorArg", e.Message } }, 500); }
    }

    static string Safe(string root, string rel)
    {
        string full = Path.GetFullPath(Path.Combine(root, rel.Replace('/', '\\')));
        return full.StartsWith(Path.GetFullPath(root).TrimEnd('\\') + "\\", StringComparison.OrdinalIgnoreCase) ? full : null;
    }

    static string PresetName(Dictionary<string, object> b)
    {
        string n = (b.ContainsKey("name") ? (string)b["name"] : "") ?? "";
        n = n.Replace((char)13, (char)32).Replace((char)10, (char)32).Trim();
        if (n.Length > 40) n = n.Substring(0, 40);
        return n == "" ? "Profile " + (Settings.Get().presets.Count + 1) : n;
    }

    static readonly Dictionary<string, object> OK = new Dictionary<string, object> { { "ok", true } };

    static void Api(Req r, Resp w)
    {
        switch (r.Path)
        {
            case "/api/ping": lastPing = DateTime.Now; w.Json(OK); return;
            case "/api/bye": byeAt = DateTime.Now; w.Json(OK); return;
            case "/api/state": w.Json(State()); return;
            case "/api/job": w.Json(job == null ? null : job.View()); return;
            case "/api/job/stop":
            {
                // «Стоп» в окне сборки (Ваня 13.11): гасим рабочий процесс; мод в Mods не тронут — новый подменяет старый одним шагом в самом конце
                var jp = jobProc; var jj = job;
                if (jj != null && !jj.done && jp != null) { jobStop = true; App.Log("task " + jj.task + ": stopped by user at " + jj.step); KillTree(jp.Id); }
                w.Json(OK); return;
            }
            case "/api/browsers":
            {
                var st = Settings.Get();
                w.Json(new Dictionary<string, object> { { "current", st.browser ?? EdgePath() ?? "" },
                    { "items", Browsers().Select(b => new Dictionary<string, object> { { "id", b[0] }, { "name", b[1] }, { "app", AppMode(b[0]) }, { "edge", IsEdge(b[0]) } }).ToList() } });
                return;
            }
            case "/api/preview":
            {
                string u;
                try { u = PreviewUrl(r.Q["slot"]); } catch (Exception e) { App.Log("preview " + r.Q["slot"] + ": " + e.Message); throw new UserError("noPreview"); }
                if (u == null) throw new UserError("noPreview");
                w.Json(new Dictionary<string, object> { { "url", u } }); return;
            }
            case "/api/music/file":
            {
                string f = Music.FileFor(r.Q["p"]);
                if (f == null) { w.Error("not found", 404); return; }
                w.File(f, r); return;
            }
        }
        if (r.Method != "POST") { w.Error("POST only", 405); return; }
        switch (r.Path)
        {
            case "/api/lang":
            {
                string c = Convert.ToString(r.JsonBody()["lang"]);
                if (Array.IndexOf(Langs, c) < 0) throw new UserError("crash", "lang " + c);
                if (Settings.Get().lang != c) { Settings.Edit(s => s.lang = c); App.Log("language: " + c); }
                w.Json(OK); return;
            }
            case "/api/view":
            {
                // вид главного окна (P2-07) — в settings.json: localStorage окна каждый запуск новый (порт другой)
                var b = r.JsonBody(); var v = new Dictionary<string, string>();
                foreach (string k in new[] { "sort", "art", "where", "view" }) { object o; if (b.TryGetValue(k, out o) && o != null) v[k] = Music.Cut(Convert.ToString(o), 200); }
                Settings.Edit(s => s.view = v); w.Json(OK); return;
            }
            case "/api/browser":
            {
                // выбор браузера из меню: запоминаем и сразу открываем окно в нем (старое окно закроет само себя)
                string id = Convert.ToString(r.JsonBody()["id"]) ?? "";
                if (id != "default" && !Browsers().Any(x => string.Equals(x[0], id, StringComparison.OrdinalIgnoreCase))) throw new UserError("crash", "browser " + id);
                Settings.Edit(s => s.browser = id != "default" && IsEdge(id) ? null : id);   // Edge = как по умолчанию (null)
                App.Log("browser chosen: " + id);
                lastPing = DateTime.Now.AddSeconds(60);   // старое окно уйдет (bye) раньше, чем новое пришлет ping — не выходить
                OpenWindow("http://127.0.0.1:" + srv.Port + "/");
                w.Json(OK); return;
            }
            case "/api/agree": Settings.Edit(s => s.agreed = true); App.Log("agreement accepted"); w.Json(OK); return;
            case "/api/pathsok": Settings.Edit(s => s.pathsOk = true); App.Log("folders confirmed: " + Settings.Get().gamePath + " | " + Settings.Get().fmmPath); w.Json(OK); return;
            case "/api/quit": w.Json(OK); quit = true; return;
            case "/api/search":
                Find.ResetSearch(); Find.Game(); Find.Fmm(); w.Json(State()); return;
            case "/api/dirs":
            {
                // выбор папки прямо в окне: диски/быстрые места или подпапки пути
                // путь из Проводника «Копировать как путь» приходит в кавычках (R1-01)
                string path = ((string)r.JsonBody()["path"] ?? "").Trim().Trim('"').Trim();
                var res = new Dictionary<string, object>();
                bool missing = path != "" && !Directory.Exists(path);
                if (path == "" || missing)
                {
                    var places = new List<object>();
                    string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                    // kind — окно подпишет на своем языке (R1-07: имена папок в Windows всегда английские)
                    foreach (var pl in new[] { Tuple.Create("desktop", Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)), Tuple.Create("downloads", Path.Combine(home, "Downloads")),
                                               Tuple.Create("documents", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)) })
                        if (Directory.Exists(pl.Item2)) places.Add(new Dictionary<string, object> { { "name", Path.GetFileName(pl.Item2) }, { "kind", pl.Item1 }, { "path", pl.Item2 } });
                    foreach (var dr in DriveInfo.GetDrives().Where(d => d.IsReady && (d.DriveType == DriveType.Fixed || d.DriveType == DriveType.Removable)))
                        places.Add(new Dictionary<string, object> { { "name", dr.Name.TrimEnd('\\') + (string.IsNullOrEmpty(dr.VolumeLabel) ? "" : "  " + dr.VolumeLabel) }, { "path", dr.Name } });
                    res["path"] = ""; res["parent"] = null; res["dirs"] = places;
                    if (missing) res["missing"] = path;   // R1-02: такой папки нет — окно оставит введенный путь и скажет об этом
                }
                else
                {
                    string full = Path.GetFullPath(path);
                    var dirs = new List<object>();
                    try
                    {
                        foreach (string d in Directory.GetDirectories(full).OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
                        {
                            var di = new DirectoryInfo(d);
                            if ((di.Attributes & (FileAttributes.Hidden | FileAttributes.System)) != 0) continue;
                            dirs.Add(new Dictionary<string, object> { { "name", di.Name }, { "path", d }, { "game", Find.IsGame(d) }, { "fmm", Find.IsFmm(d) } });
                        }
                    }
                    catch (Exception e) { res["denied"] = e.Message; }
                    var par = Directory.GetParent(full);
                    res["path"] = full; res["parent"] = par == null ? "" : par.FullName; res["dirs"] = dirs;
                    res["game"] = Find.IsGame(full); res["fmm"] = Find.IsFmm(full);
                }
                w.Json(res); return;
            }
            case "/api/pick":
            {
                var pb = r.JsonBody();
                string what = (string)pb["what"];
                string p = pb.ContainsKey("path") ? (string)pb["path"] : null;
                if (string.IsNullOrWhiteSpace(p)) { w.Json(new Dictionary<string, object> { { "cancel", true } }); return; }
                p = Find.Norm(p);
                if (what == "game")
                {
                    if (!Find.IsGame(p)) { w.Json(new Dictionary<string, object> { { "error", "notGame" }, { "errorArg", p } }); return; }
                    Settings.Edit(s => s.gamePath = p);
                }
                else
                {
                    if (!Find.IsFmm(p)) { w.Json(new Dictionary<string, object> { { "error", "notFmm" }, { "errorArg", p } }); return; }
                    Settings.Edit(s => s.fmmPath = p);
                    Find.ResetSearch();
                }
                App.Log("picked " + what + ": " + p);
                w.Json(State()); return;
            }
            case "/api/prepare":
            {
                var st = Settings.Get();
                if (!Find.IsGame(st.gamePath)) throw new UserError("noGame");
                if (!Find.IsFmm(st.fmmPath)) throw new UserError("noFmm");
                w.Json(StartJob("extract", new Dictionary<string, object> { { "game", st.gamePath }, { "fmm", st.fmmPath }, { "fingerprint", Find.Fingerprint(st.gamePath) } }).View());
                return;
            }
            case "/api/preset/use":
            {
                string id = (string)r.JsonBody()["id"];
                Settings.Edit(x => { var p = x.presets.Find(q => q.id == id); if (p == null) throw new UserError("crash", "no preset"); x.Snapshot(x.Active); x.Load(p); });
                App.Log("profile: " + Settings.Get().Active.name); w.Json(State()); return;
            }
            case "/api/preset/new":
            {
                string name = PresetName(r.JsonBody());
                Settings.Edit(x => { x.Snapshot(x.Active); var p = new Preset { id = "p" + DateTime.Now.Ticks, name = name }; x.presets.Add(p); x.activePreset = p.id; });
                App.Log("new profile: " + name); w.Json(State()); return;
            }
            case "/api/preset/rename":
            {
                var b = r.JsonBody(); string id = (string)b["id"], name = PresetName(b);
                Settings.Edit(x => { var p = x.presets.Find(q => q.id == id); if (p != null) p.name = name; });
                w.Json(State()); return;
            }
            case "/api/preset/delete":
            {
                string id = (string)r.JsonBody()["id"];
                Settings.Edit(x =>
                {
                    if (x.presets.Count <= 1) return;
                    var p = x.presets.Find(q => q.id == id); if (p == null) return;
                    bool wasActive = x.activePreset == id;
                    x.presets.Remove(p);
                    if (wasActive) x.Load(x.presets[0]);
                });
                w.Json(State()); return;
            }
            case "/api/choice":
            {
                var b = r.JsonBody();
                var c = ((Dictionary<string, object>)b["choice"]).ToDictionary(k => k.Key, k => ((System.Collections.ArrayList)k.Value).Cast<object>().Select(Convert.ToBoolean).ToArray());
                Settings.Edit(s => s.choice = c); w.Json(OK); return;
            }
            case "/api/song/upload":
            {
                // тело — PCM 48 кГц стерео 16 бит (декодирует окно), до ~20 минут
                if (r.Length <= 0 || r.Length > 48000L * 4 * 60 * 20) throw new UserError("tooLong");
                string id = "s" + DateTime.Now.ToString("yyyyMMddHHmmss") + new Random().Next(100, 999);
                string dir = Path.Combine(App.SongsDir, id); Directory.CreateDirectory(dir);
                Audio.WriteWav(Path.Combine(dir, "song.wav"), r.Body, r.Length);
                var s = new Song { id = id, file = r.Q["name"] ?? "", dur = r.Length / 4.0 / Audio.SR, added = DateTime.Now.Ticks, draft = true };
                Settings.Edit(x => { foreach (var d in x.songs.Where(y => y.draft).ToList()) { DropSong(d.id); x.songs.Remove(d); } x.songs.Add(s); });
                App.Log("song uploaded " + id + " (" + s.file + "), " + s.dur.ToString("0.0") + " s");
                w.Json(new Dictionary<string, object> { { "id", id }, { "audio", "/songs/" + id + "/song.wav" }, { "dur", s.dur } });
                return;
            }
            case "/api/song/cover":
            {
                string id = r.Q["id"];
                var song = Settings.Get().songs.FirstOrDefault(x => x.id == id);
                if (song == null || r.Length <= 8 || r.Length > 8 * 1024 * 1024) throw new UserError("badImage");
                var b = new byte[r.Length]; int o = 0; while (o < b.Length) { int n = r.Body.Read(b, o, b.Length - o); if (n <= 0) break; o += n; }
                if (b[0] != 0x89 || b[1] != 'P' || b[2] != 'N' || b[3] != 'G') throw new UserError("badImage");
                File.WriteAllBytes(Path.Combine(App.SongsDir, id, "cover.png"), b);
                Settings.Edit(x => { var s = x.songs.First(y => y.id == id); s.cover = true; s.added = DateTime.Now.Ticks; });
                if (r.Q["tofolder"] == "1" && song.src != null)   // поменяли в окне (не импорт из папки) — и в папку песни
                {
                    string mf = Music.SetCover(song.src, b);
                    if (mf != null) Settings.Edit(x => { var s = x.songs.First(y => y.id == id); s.isig = Music.Sig(mf); });
                }
                w.Json(OK); return;
            }
            case "/api/song/save":
            {
                var b = r.JsonBody(); string id = (string)b["id"];
                Settings.Edit(x =>
                {
                    var s = x.songs.FirstOrDefault(y => y.id == id); if (s == null) throw new UserError("songMissing");
                    s.title = Music.Cut((string)b["title"], Music.MaxTitle); s.artist = Music.Cut((string)b["artist"], Music.MaxArtist);
                    s.drop = Convert.ToDouble(b["drop"]); s.outro = Convert.ToDouble(b["outro"]); s.cDur = Convert.ToDouble(b["cDur"]);
                    if (b.ContainsKey("race")) s.race = Convert.ToBoolean(b["race"]);
                    if (b.ContainsKey("city")) s.city = Convert.ToBoolean(b["city"]);
                    if (b.ContainsKey("garage")) s.garage = Convert.ToBoolean(b["garage"]);
                    if (s.title == "") s.title = Music.Cut(Path.GetFileNameWithoutExtension(s.file), Music.MaxTitle);
                    s.draft = false;
                });
                Music.WriteInfo(Settings.Get().songs.FirstOrDefault(y => y.id == id));   // песня из Music — правки и в ее song.json
                w.Json(OK); return;
            }
            case "/api/song/flags":
            {
                var b = r.JsonBody(); string id = (string)b["id"];
                Settings.Edit(x => { var s = x.songs.FirstOrDefault(y => y.id == id); if (s != null) { s.race = Convert.ToBoolean(b["race"]); s.city = Convert.ToBoolean(b["city"]);
                    if (b.ContainsKey("garage")) s.garage = Convert.ToBoolean(b["garage"]);
                    if (b.ContainsKey("low")) s.low = Convert.ToBoolean(b["low"]); } });
                w.Json(OK); return;
            }
            case "/api/music/order":
            {
                // свои песни перетащили мышкой: новый порядок = номера папок Music (Music.Reorder), окно получает новое состояние
                var ids = ((System.Collections.ArrayList)r.JsonBody()["ids"]).Cast<object>().Select(o => o as string).ToList();
                Music.Reorder(ids);
                w.Json(State()); return;
            }
            case "/api/song/delete":
            {
                string id = (string)r.JsonBody()["id"];
                Settings.Edit(x =>
                {
                    var s = x.songs.FirstOrDefault(y => y.id == id); if (s == null) return;
                    x.songs.Remove(s);
                    if (s.src != null && !x.skipped.Contains(s.src, StringComparer.OrdinalIgnoreCase)) x.skipped.Add(s.src);   // папка в Music осталась — не тянуть снова
                });
                DropSong(id); w.Json(OK); return;
            }
            // ---------- папка Music ----------
            case "/api/music/scan": w.Json(MusicScan()); return;
            case "/api/music/done":
            {
                // песня из Music импортирована окном (upload → cover → save): запоминаем, откуда она; replace — старая версия (звук в папке поменяли)
                var b = r.JsonBody(); string id = (string)b["id"], old = b.ContainsKey("replace") ? b["replace"] as string : null;
                Settings.Edit(x =>
                {
                    var s = x.songs.FirstOrDefault(y => y.id == id); if (s == null) throw new UserError("songMissing");
                    s.src = (string)b["src"]; s.asig = (string)b["asig"]; s.isig = (string)b["isig"];
                    var o = old == null || old == id ? null : x.songs.FirstOrDefault(y => y.id == old);
                    if (o == null) return;
                    s.race = o.race; s.city = o.city; s.garage = o.garage; s.low = o.low; s.added = o.added;
                    foreach (var p in x.presets) { bool[] f; if (p.songs.TryGetValue(o.id, out f)) { p.songs.Remove(o.id); p.songs[s.id] = f; } }
                    x.songs.Remove(o);
                });
                if (old != null && old != id) DropSong(old);
                Music.WriteInfo(Settings.Get().songs.FirstOrDefault(y => y.id == id));
                w.Json(OK); return;
            }
            case "/api/music/told": Settings.Edit(s => s.musicTold = true); w.Json(OK); return;
            case "/api/music/put":
            {
                // песня добавлена кнопкой: оригинальный файл (тело) + обложка программы + song.json -> Music\<следующий номер>\, песня помечается как «из Music»
                string id = r.Q["id"], name = Path.GetFileName(r.Q["name"] ?? "");
                var song = Settings.Get().songs.FirstOrDefault(x => x.id == id && !x.draft);
                if (song == null) throw new UserError("songMissing");
                if (song.src != null) { w.Json(OK); return; }
                if (name == "" || !Music.AudioExt.Contains(Path.GetExtension(name).ToLowerInvariant())) name = "song" + (Music.AudioExt.Contains(Path.GetExtension(name).ToLowerInvariant()) ? Path.GetExtension(name) : ".mp3");
                if (r.Length <= 0 || r.Length > 1024L * 1024 * 1024) throw new UserError("tooLong");
                string dir = Music.NextDir();
                // длинное имя файла (P2-04: путь > 260 знаков — Windows не дает записать, песня оставалась без файла) — не длиннее Music.MaxFile
                string ext = Path.GetExtension(name), stem = Music.Cut(Path.GetFileNameWithoutExtension(name), Math.Min(Music.MaxFile, 240 - dir.Length - 1 - ext.Length)).TrimEnd('.', ' ');
                name = (stem == "" ? "song" : stem) + ext;
                Directory.CreateDirectory(dir);
                string audio = Path.Combine(dir, name), cover = Path.Combine(dir, "cover.png");
                using (var f = new FileStream(audio, FileMode.CreateNew, FileAccess.Write))
                {
                    var buf = new byte[65536]; long left = r.Length;
                    while (left > 0) { int n = r.Body.Read(buf, 0, (int)Math.Min(buf.Length, left)); if (n <= 0) break; f.Write(buf, 0, n); left -= n; }
                    if (left > 0) { f.Close(); try { Directory.Delete(dir, true); } catch { } throw new UserError("crash", "upload cut"); }
                }
                string cv = Path.Combine(App.SongsDir, id, "cover.png");
                if (File.Exists(cv)) File.Copy(cv, cover);
                Settings.Edit(x => { var s = x.songs.First(y => y.id == id); s.src = Music.Rel(audio); s.asig = Music.Sig(audio); s.isig = File.Exists(cover) ? Music.Sig(cover) : ""; });
                Music.WriteInfo(Settings.Get().songs.FirstOrDefault(y => y.id == id));
                App.Log("song " + id + " -> Music\\" + Music.Rel(audio));
                w.Json(OK); return;
            }
            case "/api/yt/list":
            {
                // ссылка: одна песня или плейлист / альбом (список песен, уже имеющиеся помечены) — Yt.List
                var b = r.JsonBody(); object u;
                w.Json(Yt.List(b.TryGetValue("url", out u) ? u as string : null)); return;
            }
            case "/api/yt":
            {
                // песня по ссылке YouTube / YT Music -> Music\<номер>\ (Yt.cs); импорт — окно, как любую папку Music
                var b = r.JsonBody(); object u;
                object ar, ti, du, nm;   // из плейлиста: исполнитель, название, длина — для замены недоступного ролика; names — имена из списка человека
                w.Json(Yt.Get(b.TryGetValue("url", out u) ? u as string : null, b.TryGetValue("artist", out ar) ? ar as string : null,
                              b.TryGetValue("title", out ti) ? ti as string : null, b.TryGetValue("dur", out du) && du != null ? Convert.ToDouble(du) : 0,
                              b.TryGetValue("names", out nm) && nm is bool && (bool)nm)); return;
            }
            case "/api/yt/sample":
            {
                // окно «Список з .txt»: пример рядом с программой (свой список человека не перезаписываем) — Проводник с выделенным файлом, как «Лог»
                string p = Path.Combine(App.Root, Yt.SampleName);
                if (!File.Exists(p)) File.WriteAllText(p, Yt.Sample, new System.Text.UTF8Encoding(false));
                Process.Start("explorer.exe", "/select,\"" + p + "\"");
                App.Log("YT sample -> " + p);
                w.Json(new Dictionary<string, object> { { "name", Yt.SampleName } }); return;
            }
            case "/api/yt/info":
            {
                // окно «Пісні з YouTube»: что по ссылке (название, исполнитель, длина) — строка списка до скачивания
                var b = r.JsonBody(); object u;
                w.Json(Yt.Peek(b.TryGetValue("url", out u) ? u as string : null)); return;
            }
            case "/api/yt/have":
            {
                // окно «Пісні з YouTube»: какие ссылки списка уже есть в программе ({items: [{url, artist, title}]} -> {have: [bool]})
                var b = r.JsonBody(); object it;
                w.Json(Yt.HaveList(b.TryGetValue("items", out it) ? it as System.Collections.IEnumerable : null)); return;
            }
            case "/api/build":
            {
                var st = Settings.Get();
                if (!Find.IsGame(st.gamePath)) throw new UserError("noGame");
                if (!Find.IsFmm(st.fmmPath)) throw new UserError("noFmm");
                var songs = Music.Ordered(st.songs.Where(s => !s.draft && (s.race || s.city || s.Gar())));   // в мод — только включенные; выключенные в игре и так не играют
                w.Json(StartJob("build", new Dictionary<string, object> {
                    { "game", st.gamePath }, { "fmm", st.fmmPath }, { "fingerprint", Find.Fingerprint(st.gamePath) },
                    { "songs", songs }, { "choice", st.choice }, { "preset", st.Active.name }, { "out", Path.Combine(Find.ModsDir(st.fmmPath), "UnboundSoundtrack.fbmod") } }).View());
                return;
            }
            case "/api/open":
            {
                string what = (string)r.JsonBody()["what"]; var st = Settings.Get();
                if (what == "mod" && st.lastBuild != null && File.Exists(st.lastBuild)) Process.Start("explorer.exe", "/select,\"" + st.lastBuild + "\"");
                else if (what == "fmm" && Find.IsFmm(st.fmmPath))
                {
                    // R1-03: не запустился (антивирус, битый exe) — окно должно сказать, а не молчать
                    try { Process.Start(new ProcessStartInfo(Path.Combine(st.fmmPath, Find.FmmExe)) { WorkingDirectory = st.fmmPath, UseShellExecute = true }); }
                    catch (Exception e)
                    {
                        var we = e as System.ComponentModel.Win32Exception;
                        if (we == null || we.NativeErrorCode != 1223) { App.Log("start Mod Manager: " + e.Message); throw new UserError("fmmStart", e.Message); }   // 1223 — сам отменил UAC
                    }
                }
                else if (what == "vcredist")
                {
                    // P2-01, запасной путь: Visual C++ Redistributable x64 (установщик Microsoft лежит в app\vcredist)
                    string vc = Path.Combine(App.AppDir, "vcredist", "VC_redist.x64.exe");
                    if (!File.Exists(vc)) throw new UserError("crash", "no " + vc);
                    try { Process.Start(new ProcessStartInfo(vc) { UseShellExecute = true }); App.Log("VC++ installer started"); }
                    catch (System.ComponentModel.Win32Exception e) { if (e.NativeErrorCode != 1223) throw; App.Log("VC++ installer: UAC cancelled"); }
                }
                else if (what == "log") Process.Start("explorer.exe", "/select,\"" + App.LogPath + "\"");
                else if (what == "folder") Process.Start("explorer.exe", "\"" + App.Root + "\"");
                else if (what == "music") { Directory.CreateDirectory(App.MusicDir); Process.Start("explorer.exe", "\"" + App.MusicDir + "\""); }
                w.Json(OK); return;
            }
            case "/api/url":
            {
                string u = (string)r.JsonBody()["url"];
                if (u != null && (u.StartsWith("https://") || u.StartsWith("http://")) && u.IndexOfAny(new[] { '"', ' ', '\n' }) < 0) Process.Start(u);
                w.Json(OK); return;
            }
        }
        w.Error("unknown", 404);
    }
}
