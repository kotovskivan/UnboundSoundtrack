// Громкость песни (17.11, Ваня; отзыв MGS3PLAYER на Nexus: его песни ~−15 дБ, в игре «почти не слышно»).
// gain ≤ 1 — просто тише. gain > 1 — громче с мягким лимитером: потолок −1 dBFS, упреждение 10 мс (громкость опускается заранее и плавно),
// отпускание 300 мс. Тест в игре 17.11: быстрый лимитер (отпускание 80 мс) «пердел басом» — дергал громкость внутри волны баса;
// мягкий (AUTO S, те же 10 / 300 мс у ffmpeg alimiter) — Ваня: «самый нормальный».
// Срезанных сэмплов нет: громкость в кадре i = среднее h[i−A..i], где h[k] = минимум нужной громкости на [k, k+A] — каждое такое окно содержит i,
// значит среднее не больше нужной в i.
using System;

static class Limiter
{
    public const double Ceiling = 0.891;   // −1 dBFS
    public const double Attack = 0.010, Release = 0.300;

    // pcm — 16 бит стерео (как Audio.ReadPcm), меняется на месте
    // lim = false — без защиты (галочку сняли): просто громче, что выше края — срезается (треск; окно предупреждает)
    public static void Apply(byte[] pcm, double gain, int sr, bool lim = true)
    {
        long n = pcm.Length / 4;
        if (n == 0 || gain == 1) return;
        if (gain < 1 || !lim)
        {
            for (long i = 0; i < n * 2; i++) Put(pcm, i, Get(pcm, i) * gain);
            return;
        }
        int A = Math.Max(1, (int)Math.Round(Attack * sr));
        double rel = 1 - Math.Exp(-1.0 / (Release * sr));
        // e[i] — нужная громкость с отпусканием: сразу вниз, вверх — плавно
        var e = new float[n];
        double env = 1;
        for (long i = 0; i < n; i++)
        {
            double x = Math.Max(Math.Abs(Get(pcm, 2 * i)), Math.Abs(Get(pcm, 2 * i + 1))) / 32768.0 * gain;
            double need = x > Ceiling ? Ceiling / x : 1;
            env = Math.Min(need, env + (1 - env) * rel);
            e[i] = (float)env;
        }
        // h(k) = min e[max(0,k) .. min(n−1, k+A)] — монотонная очередь индексов; g(i) = среднее h(i−A..i) — кольцо из A+1 значений
        var dq = new long[A + 2]; int qh = 0, qn = 0;   // кольцевая очередь, по возрастанию e
        var ring = new double[A + 1]; double sum = 0; int rp = 0;
        long next = 0;   // следующий индекс e, который еще не в очереди
        for (long k = -A; k < n; k++)
        {
            long hi = Math.Min(n - 1, k + A);
            while (next <= hi)
            {
                while (qn > 0 && e[dq[(qh + qn - 1) % dq.Length]] >= e[next]) qn--;
                dq[(qh + qn) % dq.Length] = next; qn++; next++;
            }
            long lo = Math.Max(0, k);
            while (qn > 0 && dq[qh] < lo) { qh = (qh + 1) % dq.Length; qn--; }
            double h = e[dq[qh]];
            sum += h - ring[rp]; ring[rp] = h; rp = (rp + 1) % ring.Length;
            if (k < 0) continue;
            double g = sum / (A + 1);   // начали с k = −A — к k = 0 в кольце уже A+1 значений
            Put(pcm, 2 * k, Get(pcm, 2 * k) * gain * g);
            Put(pcm, 2 * k + 1, Get(pcm, 2 * k + 1) * gain * g);
        }
    }

    static int Get(byte[] d, long s) { return (short)(d[2 * s] | (d[2 * s + 1] << 8)); }
    static void Put(byte[] d, long s, double v)
    {
        int x = (int)Math.Round(v); if (x > 32767) x = 32767; else if (x < -32768) x = -32768;
        d[2 * s] = (byte)x; d[2 * s + 1] = (byte)(x >> 8);
    }
}
