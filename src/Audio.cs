// Своя песня -> куски A/B/C (48 кГц, стерео, 16 бит) -> кодировщик EA (Tool.exe) -> .sps + .sek.
// A — полная; B — «старт гонки»: 17 с катсцены + 3,45 с старта 3-2-1 до GO и дальше до конца (GO раньше 20 с — с начала);
// C — «финиш»: свой отрезок с затуханием 3 с.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

static class Audio
{
    public const int SR = 48000;
    // старт гонки: 17 с катсцены, потом 3 с отсчета 3-2-1, на GO — дроп. До 12.11 было 17 (только катсцена) — дроп бил на «3»
    // (видео Вани 12.11 + отзыв в Steam: «drop on go не на go, а на начало отсчета»)
    // старт гонки: катсцена + старт (отсчет 3-2-1), на GO — дроп. 12.11: 17+3 (отзыв MtheAX: при 17 дроп бил на «3»);
    // 13.11 Ваня в игре: 21+3 — «шляпа», 17+4 — мимо; 20,65 = замер по самой игре: дроп в куске B у треков игры на 20,3-21,1 с,
    // медиана сильных 20,65 (паспорт игры, «Старт гонки»; tools/frosty-reader/intro_drops.py); 20,65 в игре тоже мимо → Ваня: 20,45
    // (окно: те же числа в web/index.html B_CUT / B_COUNT; ключ кэша куска B содержит B_INTRO — старые куски перекодируются сами)
    public const double B_CUT = 17.0, B_COUNT = 3.45, B_INTRO = B_CUT + B_COUNT;

    // song.wav пишем сами (из браузера приходит готовый PCM), поэтому заголовок простой: 44 байта
    public static void WriteWav(string path, Stream pcm, long bytes)
    {
        using (var f = File.Create(path))
        {
            WriteHeader(f, bytes);
            var buf = new byte[1 << 20]; long left = bytes;
            while (left > 0) { int n = pcm.Read(buf, 0, (int)Math.Min(buf.Length, left)); if (n <= 0) break; f.Write(buf, 0, n); left -= n; }
            if (left > 0) throw new IOException("upload cut short");
        }
    }
    static void WriteHeader(Stream f, long bytes)
    {
        var w = new BinaryWriter(f, Encoding.ASCII, true);
        w.Write(Encoding.ASCII.GetBytes("RIFF")); w.Write((uint)(36 + bytes)); w.Write(Encoding.ASCII.GetBytes("WAVEfmt "));
        w.Write(16); w.Write((short)1); w.Write((short)2); w.Write(SR); w.Write(SR * 4); w.Write((short)4); w.Write((short)16);
        w.Write(Encoding.ASCII.GetBytes("data")); w.Write((uint)bytes); w.Flush();
    }

    static byte[] ReadPcm(string wav)
    {
        byte[] all = File.ReadAllBytes(wav);
        if (all.Length < 44 || Encoding.ASCII.GetString(all, 0, 4) != "RIFF") throw new Exception("bad wav " + wav);
        int p = 12;
        while (p + 8 <= all.Length)
        {
            string id = Encoding.ASCII.GetString(all, p, 4); int sz = BitConverter.ToInt32(all, p + 4);
            if (id == "data") { int n = Math.Min(sz, all.Length - p - 8); var d = new byte[n - n % 4]; Buffer.BlockCopy(all, p + 8, d, 0, d.Length); return d; }
            p += 8 + sz + (sz & 1);
        }
        throw new Exception("no data in " + wav);
    }

    // pad — тишина перед куском (с): старт гонки, если GO в песне раньше 20 с (Ваня 12.11: «да делать на GO») — дроп все равно ровно на GO
    static void Piece(byte[] pcm, double from, double len, double fade, string path, double pad = 0)
    {
        long total = pcm.Length / 4;
        long a = Math.Max(0, Math.Min(total, (long)Math.Round(from * SR)));
        long n = len < 0 ? total - a : Math.Min(total - a, (long)Math.Round(len * SR));
        long z = Math.Max(0, (long)Math.Round(pad * SR));
        var d = new byte[(z + n) * 4]; Buffer.BlockCopy(pcm, (int)(a * 4), d, (int)(z * 4), (int)(n * 4));   // массив — нули: тишина уже есть
        if (fade > 0)
        {
            long fn = Math.Min(n, (long)(fade * SR));
            for (long i = 0; i < fn; i++)
            {
                double k = 1.0 - (double)i / Math.Max(1, fn - 1);   // от 1 до 0, как np.linspace(1, 0, n)
                long s = (z + n - fn + i) * 4;
                for (int c = 0; c < 2; c++) { short v = BitConverter.ToInt16(d, (int)s + 2 * c); short nv = (short)(v * k); d[s + 2 * c] = (byte)nv; d[s + 2 * c + 1] = (byte)(nv >> 8); }
            }
        }
        using (var f = File.Create(path)) { WriteHeader(f, d.Length); f.Write(d, 0, d.Length); }
    }

    // готовит папку enc\ с A/B/C .sps/.sek. Кусок перекодируется, только если поменялось то, от чего он зависит:
    // A — звук; B — звук + GO + длина старта; C — звук + финиш. Двинул GO — кодируется один B (раньше все три, ~4 с на песню).
    // Ключи — enc\key_A.txt / key_B.txt / key_C.txt (пишутся после удачного кодирования куска).
    public static string Prepare(Song s, Action<string> say)
    {
        string dir = Path.Combine(App.SongsDir, s.id), enc = Path.Combine(dir, "enc");
        string wav = Path.Combine(dir, "song.wav");
        if (!File.Exists(wav)) throw new UserError("songMissing", s.title);
        var wi = new FileInfo(wav);
        string w = wi.Length + "|" + wi.LastWriteTimeUtc.Ticks;
        var keys = new Dictionary<string, string> {
            { "A", w + "|v1" },
            { "B", w + "|" + s.drop.ToString("0.00") + "|" + B_INTRO.ToString("0.00") + (s.drop < B_INTRO ? "|pad" : "") + "|v1" },   // pad — тишина перед ранним GO (12.11)
            { "C", w + "|" + s.outro.ToString("0.00") + "|" + s.cDur.ToString("0.00") + "|v1" } };
        // кэш до 12.11 (один key.txt на три куска, B — с 17 с): A и C из него годятся, B — перекодировать
        string oldFile = Path.Combine(enc, "key.txt");
        string oldKey = string.Join("|", wi.Length, wi.LastWriteTimeUtc.Ticks, s.drop.ToString("0.00"), s.outro.ToString("0.00"), s.cDur.ToString("0.00"), "v1");
        bool oldOk = File.Exists(oldFile) && File.ReadAllText(oldFile) == oldKey;
        Directory.CreateDirectory(enc);
        var todo = new List<string>();
        foreach (var k in keys)
        {
            string kf = Path.Combine(enc, "key_" + k.Key + ".txt");
            bool have = File.Exists(Path.Combine(enc, k.Key + ".sps")) && File.Exists(Path.Combine(enc, k.Key + ".sek"));
            if (have && File.Exists(kf) && File.ReadAllText(kf) == k.Value) continue;
            if (have && oldOk && k.Key != "B") { File.WriteAllText(kf, k.Value); continue; }
            todo.Add(k.Key);
        }
        if (File.Exists(oldFile)) File.Delete(oldFile);
        if (todo.Count == 0) return enc;

        byte[] pcm = ReadPcm(wav);
        double dur = pcm.Length / 4.0 / SR;
        double b0 = Math.Max(0, s.drop - B_INTRO);
        double cLen = Math.Min(s.cDur, dur), c0 = Math.Max(0, Math.Min(s.outro, dur - cLen));
        foreach (string k in todo)
        {
            File.Delete(Path.Combine(enc, "key_" + k + ".txt"));
            foreach (string f in Directory.GetFiles(enc, k + ".*")) File.Delete(f);
            if (k == "A") Piece(pcm, 0, -1, 0, Path.Combine(enc, "A.wav"));
            if (k == "B") Piece(pcm, b0, -1, 0, Path.Combine(enc, "B.wav"), Math.Max(0, B_INTRO - s.drop));   // GO раньше B_INTRO — тишина перед песней
            if (k == "C") Piece(pcm, c0, cLen, 3.0, Path.Combine(enc, "C.wav"));
        }
        pcm = null;
        foreach (string k in todo)
        {
            say(k);
            Encode(Path.Combine(enc, k + ".wav"), Path.Combine(enc, k));
            File.Delete(Path.Combine(enc, k + ".wav"));
            File.WriteAllText(Path.Combine(enc, "key_" + k + ".txt"), keys[k]);
        }
        return enc;
    }

    public static Process Current;   // кодировщик, который работает сейчас
    static void Encode(string wav, string outBase)
    {
        string tool = Path.Combine(App.EncDir, "Tool.exe");
        if (!File.Exists(tool)) throw new UserError("encoderMissing", tool);
        // кодировщик EA не понимает кириллицу в путях (P2-03: «Мої моди», C:\Users\Іван) — запускаем из папки wav, в аргументах только имена
        // (wav и outBase всегда в одной папке; сам Tool.exe из папки с кириллицей работает — проверено 12.11)
        var psi = new ProcessStartInfo(tool, "-sndplayer -fileformatversion1 -ealayer3pcm_int -seekable \"" + Path.GetFileName(wav) + "\" \"-=" + Path.GetFileName(outBase) + "\"")
        { WorkingDirectory = Path.GetDirectoryName(wav), UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true };
        using (var p = Process.Start(psi))
        {
            Current = p;   // программу закрыли посреди кодирования — Worker.WatchParent гасит и его
            string o = p.StandardOutput.ReadToEnd(), e = p.StandardError.ReadToEnd();
            p.WaitForExit();
            if (p.ExitCode != 0 || !File.Exists(outBase + ".sps") || !File.Exists(outBase + ".sek"))
            {
                App.Log("encoder exit " + p.ExitCode + "\n" + o + "\n" + e);
                if (p.ExitCode == -1073741515) throw new UserError("vcMissing", "Tool.exe");   // 0xC0000135: нет DLL (VCRUNTIME140 в app\encoder пропала)
                throw new UserError("encodeFailed", Path.GetFileName(wav));
            }
        }
    }
}

// ошибка, которую окно покажет на языке человека (key — ключ перевода, arg — подробность)
class UserError : Exception
{
    public string Key, Arg;
    public UserError(string key, string arg = null) : base(key + (arg != null ? ": " + arg : "")) { Key = key; Arg = arg; }
}
