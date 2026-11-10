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
    public static readonly string[] AudioExt = { ".mp3", ".flac", ".wav", ".ogg", ".oga", ".opus", ".m4a", ".aac" };
    public static readonly string[] ImgExt = { ".jpg", ".jpeg", ".png", ".webp", ".bmp" };
    public const string InfoFile = "song.json";
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
            App.WriteJson(Path.Combine(dir, InfoFile), new Dictionary<string, object> {
                { "artist", s.artist }, { "title", s.title }, { "drop", s.drop }, { "outro", s.outro }, { "cDur", s.cDur } });
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
        catch (Exception e) { App.Log("обложка в Music " + src + ": " + e.Message); return null; }
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
