// Своя песня -> куски A/B/C (48 кГц, стерео, 16 бит) -> кодировщик EA (Tool.exe) -> .sps + .sek.
// A — полная; B — «старт гонки»: 17 с до GO и дальше до конца (GO раньше 17 с — с начала); C — «финиш»: свой отрезок с затуханием 3 с.
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

static class Audio
{
    public const int SR = 48000;
    public const double B_INTRO = 17.0;

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

    static void Piece(byte[] pcm, double from, double len, double fade, string path)
    {
        long total = pcm.Length / 4;
        long a = Math.Max(0, Math.Min(total, (long)Math.Round(from * SR)));
        long n = len < 0 ? total - a : Math.Min(total - a, (long)Math.Round(len * SR));
        var d = new byte[n * 4]; Buffer.BlockCopy(pcm, (int)(a * 4), d, 0, d.Length);
        if (fade > 0)
        {
            long fn = Math.Min(n, (long)(fade * SR));
            for (long i = 0; i < fn; i++)
            {
                double k = 1.0 - (double)i / Math.Max(1, fn - 1);   // от 1 до 0, как np.linspace(1, 0, n)
                long s = (n - fn + i) * 4;
                for (int c = 0; c < 2; c++) { short v = BitConverter.ToInt16(d, (int)s + 2 * c); short nv = (short)(v * k); d[s + 2 * c] = (byte)nv; d[s + 2 * c + 1] = (byte)(nv >> 8); }
            }
        }
        using (var f = File.Create(path)) { WriteHeader(f, d.Length); f.Write(d, 0, d.Length); }
    }

    // готовит папку enc\ с A/B/C .sps/.sek; если метки не менялись — ничего не делает
    public static string Prepare(Song s, Action<string> say)
    {
        string dir = Path.Combine(App.SongsDir, s.id), enc = Path.Combine(dir, "enc");
        string wav = Path.Combine(dir, "song.wav");
        if (!File.Exists(wav)) throw new UserError("songMissing", s.title);
        var wi = new FileInfo(wav);
        string key = string.Join("|", wi.Length, wi.LastWriteTimeUtc.Ticks, s.drop.ToString("0.00"), s.outro.ToString("0.00"), s.cDur.ToString("0.00"), "v1");
        string keyFile = Path.Combine(enc, "key.txt");
        bool ok = File.Exists(keyFile) && File.ReadAllText(keyFile) == key;
        foreach (string k in new[] { "A", "B", "C" }) ok &= File.Exists(Path.Combine(enc, k + ".sps")) && File.Exists(Path.Combine(enc, k + ".sek"));
        if (ok) return enc;
        if (Directory.Exists(enc)) Directory.Delete(enc, true);
        Directory.CreateDirectory(enc);
        byte[] pcm = ReadPcm(wav);
        double dur = pcm.Length / 4.0 / SR;
        double b0 = Math.Max(0, s.drop - B_INTRO);
        double cLen = Math.Min(s.cDur, dur), c0 = Math.Max(0, Math.Min(s.outro, dur - cLen));
        Piece(pcm, 0, -1, 0, Path.Combine(enc, "A.wav"));
        Piece(pcm, b0, -1, 0, Path.Combine(enc, "B.wav"));
        Piece(pcm, c0, cLen, 3.0, Path.Combine(enc, "C.wav"));
        pcm = null;
        foreach (string k in new[] { "A", "B", "C" })
        {
            say(k);
            Encode(Path.Combine(enc, k + ".wav"), Path.Combine(enc, k));
            File.Delete(Path.Combine(enc, k + ".wav"));
        }
        File.WriteAllText(keyFile, key);
        return enc;
    }

    static void Encode(string wav, string outBase)
    {
        string tool = Path.Combine(App.EncDir, "Tool.exe");
        if (!File.Exists(tool)) throw new UserError("encoderMissing", tool);
        var psi = new ProcessStartInfo(tool, "-sndplayer -fileformatversion1 -ealayer3pcm_int -seekable \"" + wav + "\" \"-=" + outBase + "\"")
        { WorkingDirectory = App.EncDir, UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true };
        using (var p = Process.Start(psi))
        {
            string o = p.StandardOutput.ReadToEnd(), e = p.StandardError.ReadToEnd();
            p.WaitForExit();
            if (p.ExitCode != 0 || !File.Exists(outBase + ".sps") || !File.Exists(outBase + ".sek"))
            {
                App.Log("encoder exit " + p.ExitCode + "\n" + o + "\n" + e);
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
