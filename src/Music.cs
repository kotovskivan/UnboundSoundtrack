using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

// Папка Music рядом с exe — библиотека песен. Одна песня — одна папка с номером: Music\1\, Music\2\… (порядок в окне = номер).
// В папке: звук + картинка (cover/folder/front, иначе первая) + song.json (исполнитель, название, GO, финиш — пишет программа;
// имя папки ни на что не влияет, Windows ничего не режет). Кнопка «Додати пісню» сама создает следующую папку.
// Папка главная: появилась — окно импортирует (звук декодирует браузер); пропала — песня удаляется. Тут — файлы; импорт — Ui (/api/music/*).
static class Music
{
    public static readonly string[] AudioExt = { ".mp3", ".flac", ".wav", ".ogg", ".oga", ".opus", ".m4a", ".aac", ".webm" };
    public static readonly string[] ImgExt = { ".jpg", ".jpeg", ".png", ".webp", ".bmp" };
    public const string InfoFile = "song.json";
    // длина имен (Ваня 13.11: «запрещай длинные — в игре их все равно не видно»). У треков самой игры максимум: название 27, исполнитель 70.
    public const int MaxTitle = 40, MaxArtist = 60, MaxFile = 60;
    public static string Cut(string s, int max) { s = (s ?? "").Trim(); max = Math.Max(0, max); return s.Length > max ? s.Substring(0, max).TrimEnd() : s; }
    const int MaxFiles = 500, MaxDepth = 4;

    // src — путь звука относительно Music (ключ песни), name — имя файла без расширения (если нет ни song.json, ни тегов),
    // image — картинка относительно Music или null, asig/isig — размер:время звука и картинки (поменялись — переимпорт), info — song.json
    public class Entry { public string src, name, image, asig, isig; public Dictionary<string, object> info; }

    static string Root { get { return Path.GetFullPath(App.MusicDir).TrimEnd('\\'); } }

    public static List<Entry> Scan()
    {
        var res = new List<Entry>();
        string root = Root;
        if (!Directory.Exists(root)) return res;
        var dirs = new List<string>();
        Walk(root, 0, dirs);
        foreach (string dir in dirs)
        {
            string[] files = Files(dir);
            var audio = files.Where(f => Has(AudioExt, f)).OrderBy(f => f, StringComparer.OrdinalIgnoreCase).ToList();
            if (audio.Count == 0) continue;
            var imgs = files.Where(f => Has(ImgExt, f)).OrderBy(f => f, StringComparer.OrdinalIgnoreCase).ToList();
            bool inRoot = string.Equals(dir, root, StringComparison.OrdinalIgnoreCase);
            string common = inRoot ? null : imgs.FirstOrDefault(i => new[] { "cover", "folder", "front" }.Contains(Path.GetFileNameWithoutExtension(i).ToLowerInvariant())) ?? imgs.FirstOrDefault();
            var info = !inRoot && audio.Count == 1 ? App.ReadJson<Dictionary<string, object>>(Path.Combine(dir, InfoFile)) : null;
            foreach (string f in audio)
            {
                if (res.Count >= MaxFiles) return res;
                string bn = Path.GetFileNameWithoutExtension(f);
                string img = imgs.FirstOrDefault(i => string.Equals(Path.GetFileNameWithoutExtension(i), bn, StringComparison.OrdinalIgnoreCase)) ?? common;
                res.Add(new Entry { src = Rel(root, f), name = bn, image = img == null ? null : Rel(root, img),
                                    asig = Sig(f), isig = img == null ? "" : Sig(img), info = info });
            }
        }
        return res;
    }

    // обход папок; недоступная папка не валит весь поиск
    static void Walk(string dir, int depth, List<string> acc)
    {
        acc.Add(dir);
        if (depth >= MaxDepth) return;
        string[] subs;
        try { subs = Directory.GetDirectories(dir); } catch (Exception e) { App.Log("Music: " + dir + ": " + e.Message); return; }
        foreach (string d in subs.OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
        {
            try { if ((File.GetAttributes(d) & (FileAttributes.Hidden | FileAttributes.System)) != 0) continue; } catch { continue; }
            Walk(d, depth + 1, acc);
        }
    }
    static string[] Files(string dir) { try { return Directory.GetFiles(dir); } catch (Exception e) { App.Log("Music: " + dir + ": " + e.Message); return new string[0]; } }
    static bool Has(string[] ext, string f) { return ext.Contains(Path.GetExtension(f).ToLowerInvariant()); }
    static string Rel(string root, string f) { return f.Substring(root.Length).TrimStart('\\'); }
    public static string Rel(string f) { return Rel(Root, Path.GetFullPath(f)); }
    public static string Sig(string f) { try { var i = new FileInfo(f); return i.Length + ":" + i.LastWriteTimeUtc.Ticks; } catch { return ""; } }

    // порядок в окне и в моде: папки с номером по номеру, потом остальные по имени, потом песни не из Music
    static long Num(string src)
    {
        if (src == null) return long.MaxValue;
        int k = src.IndexOf('\\'); long n;
        return k > 0 && long.TryParse(src.Substring(0, k), out n) && n >= 0 ? n : long.MaxValue - 1;
    }
    public static List<Song> Ordered(IEnumerable<Song> songs)
    {
        return songs.OrderBy(s => Num(s.src)).ThenBy(s => s.src ?? "", StringComparer.OrdinalIgnoreCase).ThenBy(s => s.added).ToList();
    }

    // свои песни перетащили мышкой (Ваня: «папки 1, 2, 3 = порядок»): номера папок ЭТИХ песен переставляются между собой
    // по новому порядку ids. Только папки «номер = одна песня»; переименование в 2 шага через временные имена, ошибка — откат.
    // src песен правим сразу (не ждем скана: тот же звук под новым путем он бы тоже узнал по asig).
    public static void Reorder(List<string> ids)
    {
        var st = Settings.Get();
        var list = ids.Select(id => st.songs.FirstOrDefault(s => s.id == id && !s.draft && s.src != null)).ToList();
        if (list.Count < 2 || list.Any(s => s == null)) throw new UserError("orderFail", "songs");
        var cur = list.Select(s => Num(s.src)).ToList();
        if (cur.Any(n => n >= long.MaxValue - 1)) throw new UserError("orderFail", "folder without number");
        var perFolder = st.songs.Where(s => s.src != null && !s.draft).GroupBy(s => Num(s.src)).ToDictionary(g => g.Key, g => g.Count());
        if (cur.Distinct().Count() != cur.Count || cur.Any(n => perFolder[n] != 1)) throw new UserError("orderFail", "several songs in one folder");
        var nums = cur.OrderBy(n => n).ToList();                       // те же номера — в новом порядке
        var moves = new List<Tuple<string, long, long>>();             // id, было, стало
        for (int i = 0; i < list.Count; i++) if (cur[i] != nums[i]) moves.Add(Tuple.Create(list[i].id, cur[i], nums[i]));
        if (moves.Count == 0) return;
        string tag = "_order_" + DateTime.Now.Ticks + "_";
        Func<string, string> P = n => Path.Combine(App.MusicDir, n);
        var toTmp = new List<long>(); var toNew = new List<Tuple<long, long>>();
        try
        {
            foreach (var m in moves) { Directory.Move(P(m.Item2.ToString()), P(tag + m.Item2)); toTmp.Add(m.Item2); }
            foreach (var m in moves) { Directory.Move(P(tag + m.Item2), P(m.Item3.ToString())); toNew.Add(Tuple.Create(m.Item2, m.Item3)); }
        }
        catch (Exception e)
        {
            App.Log("song order: " + e.Message + " - rolling back");
            foreach (var d in toNew) try { Directory.Move(P(d.Item2.ToString()), P(tag + d.Item1)); } catch (Exception e2) { App.Log("rollback: " + e2.Message); }
            foreach (long n in toTmp) try { Directory.Move(P(tag + n), P(n.ToString())); } catch (Exception e2) { App.Log("rollback: " + e2.Message); }
            throw new UserError("orderFail", e.Message);
        }
        Settings.Edit(x =>
        {
            foreach (var m in moves)
            {
                var s = x.songs.First(y => y.id == m.Item1); int k = s.src.IndexOf('\\');
                s.src = m.Item3 + s.src.Substring(k);
            }
        });
        App.Log("song order: folders moved " + moves.Count + " (" + string.Join(", ", moves.Select(m => m.Item2 + "->" + m.Item3)) + ")");
    }

    // следующая папка для песни, добавленной кнопкой: наибольший номер + 1
    public static string NextDir()
    {
        Directory.CreateDirectory(App.MusicDir);
        long max = 0, n;
        foreach (string d in Directory.GetDirectories(App.MusicDir)) if (long.TryParse(Path.GetFileName(d), out n) && n > max) max = n;
        string p = Path.Combine(App.MusicDir, (max + 1).ToString());
        while (Directory.Exists(p) || File.Exists(p)) p = Path.Combine(App.MusicDir, (++max + 1).ToString());
        return p;
    }

    // song.json в папке песни: исполнитель, название, GO, финиш — переживает переустановку программы
    public static void WriteInfo(Song s)
    {
        try
        {
            if (s == null || s.src == null || s.src.IndexOf('\\') < 0) return;           // файл в корне Music — своей папки нет
            string dir = Path.GetDirectoryName(Path.Combine(App.MusicDir, s.src));
            if (!Directory.Exists(dir) || Files(dir).Count(f => Has(AudioExt, f)) != 1) return;
            string pj = Path.Combine(dir, InfoFile);
            var j = App.ReadJson<Dictionary<string, object>>(pj) ?? new Dictionary<string, object>();   // чужие поля (yt — id видео) не теряем
            j["artist"] = s.artist; j["title"] = s.title; j["drop"] = s.drop; j["outro"] = s.outro; j["cDur"] = s.cDur;
            App.WriteJson(pj, j);
        }
        catch (Exception e) { App.Log("song.json " + s.src + ": " + e.Message); }
    }

    // обложку поменяли в окне — она же и в папке песни (старые картинки папки убираем, чтобы при переустановке не вернулась старая)
    public static string SetCover(string src, byte[] png)
    {
        try
        {
            if (src == null || src.IndexOf('\\') < 0) return null;
            string dir = Path.GetDirectoryName(Path.Combine(App.MusicDir, src));
            if (!Directory.Exists(dir)) return null;
            var files = Files(dir);
            if (files.Count(f => Has(AudioExt, f)) != 1) return null;
            foreach (string f in files.Where(f => Has(ImgExt, f))) File.Delete(f);
            string p = Path.Combine(dir, "cover.png");
            File.WriteAllBytes(p, png);
            return p;
        }
        catch (Exception e) { App.Log("cover in Music " + src + ": " + e.Message); return null; }
    }

    // файл для окна: только звук и картинки внутри Music
    public static string FileFor(string rel)
    {
        if (string.IsNullOrEmpty(rel)) return null;
        string root = Root, full;
        try { full = Path.GetFullPath(Path.Combine(root, rel.Replace('/', '\\'))); } catch { return null; }
        if (!full.StartsWith(root + "\\", StringComparison.OrdinalIgnoreCase)) return null;
        return Has(AudioExt, full) || Has(ImgExt, full) ? full : null;
    }
}
