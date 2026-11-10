// BC7: декодер всех 8 режимов (обложки и фоны из игры -> картинки для окна) и простой кодировщик режима 6 (своя обложка -> игра).
// Таблицы разбиения и якорей — как в спецификации BPTC (сверены с таблицами Pillow).
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

static class Bc7
{
    static readonly ushort[] P2 = {
        0xcccc,0x8888,0xeeee,0xecc8,0xc880,0xfeec,0xfec8,0xec80,0xc800,0xffec,0xfe80,0xe800,0xffe8,0xff00,0xfff0,0xf000,
        0xf710,0x008e,0x7100,0x08ce,0x008c,0x7310,0x3100,0x8cce,0x088c,0x3110,0x6666,0x366c,0x17e8,0x0ff0,0x718e,0x399c,
        0xaaaa,0xf0f0,0x5a5a,0x33cc,0x3c3c,0x55aa,0x9696,0xa55a,0x73ce,0x13c8,0x324c,0x3bdc,0x6996,0xc33c,0x9966,0x0660,
        0x0272,0x04e4,0x4e40,0x2720,0xc936,0x936c,0x39c6,0x639c,0x9336,0x9cc6,0x817e,0xe718,0xccf0,0x0fcc,0x7744,0xee22 };
    static readonly uint[] P3 = {
        0xaa685050,0x6a5a5040,0x5a5a4200,0x5450a0a8,0xa5a50000,0xa0a05050,0x5555a0a0,0x5a5a5050,0xaa550000,0xaa555500,0xaaaa5500,0x90909090,0x94949494,0xa4a4a4a4,0xa9a59450,0x2a0a4250,
        0xa5945040,0x0a425054,0xa5a5a500,0x55a0a0a0,0xa8a85454,0x6a6a4040,0xa4a45000,0x1a1a0500,0x0050a4a4,0xaaa59090,0x14696914,0x69691400,0xa08585a0,0xaa821414,0x50a4a450,0x6a5a0200,
        0xa9a58000,0x5090a0a8,0xa8a09050,0x24242424,0x00aa5500,0x24924924,0x24499224,0x50a50a50,0x500aa550,0xaaaa4444,0x66660000,0xa5a0a5a0,0x50a050a0,0x69286928,0x44aaaa44,0x66666600,
        0xaa444444,0x54a854a8,0x95809580,0x96969600,0xa85454a8,0x80959580,0xaa141414,0x96960000,0xaaaa1414,0xa05050a0,0xa0a5a5a0,0x96000000,0x40804080,0xa9a8a9a8,0xaaaaaa44,0x2a4a5254 };
    static readonly byte[] A2 = {
        15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,2,8,2,2,8,8,15,2,8,2,2,8,8,2,2,
        15,15,6,8,2,8,15,15,2,8,2,2,2,15,15,6,6,2,6,8,15,15,2,2,15,15,15,15,15,2,2,15 };
    static readonly byte[] A3a = {
        3,3,15,15,8,3,15,15,8,8,6,6,6,5,3,3,3,3,8,15,3,3,6,10,5,8,8,6,8,5,15,15,
        8,15,3,5,6,10,8,15,15,3,15,5,15,15,15,15,3,15,5,5,5,8,5,10,5,10,8,13,15,12,3,3 };
    static readonly byte[] A3b = {
        15,8,8,3,15,15,3,8,15,15,15,15,15,15,15,8,15,8,15,3,15,8,15,8,3,15,6,10,15,15,10,8,
        15,3,15,10,10,8,9,10,6,15,8,15,3,6,6,8,15,3,15,15,15,15,15,15,15,15,15,15,3,15,15,8 };
    static readonly int[] W2 = { 0, 21, 43, 64 }, W3 = { 0, 9, 18, 27, 37, 46, 55, 64 }, W4 = { 0, 4, 9, 13, 17, 21, 26, 30, 34, 38, 43, 47, 51, 55, 60, 64 };
    // режим: NS PB RB ISB CB AB EPB SPB IB IB2
    static readonly int[,] M = {
        { 3, 4, 0, 0, 4, 0, 1, 0, 3, 0 }, { 2, 6, 0, 0, 6, 0, 0, 1, 3, 0 }, { 3, 6, 0, 0, 5, 0, 0, 0, 2, 0 }, { 2, 6, 0, 0, 7, 0, 1, 0, 2, 0 },
        { 1, 0, 2, 1, 5, 6, 0, 0, 2, 3 }, { 1, 0, 2, 0, 7, 8, 0, 0, 2, 2 }, { 1, 0, 0, 0, 7, 7, 1, 0, 4, 0 }, { 2, 6, 0, 0, 5, 5, 1, 0, 2, 0 } };

    struct Bits
    {
        ulong lo, hi; int pos;
        public Bits(byte[] b, int o) { lo = BitConverter.ToUInt64(b, o); hi = BitConverter.ToUInt64(b, o + 8); pos = 0; }
        public int Get(int n)
        {
            if (n == 0) return 0;
            ulong v = pos < 64 ? (lo >> pos) | (pos > 0 && pos + n > 64 ? hi << (64 - pos) : 0) : hi >> (pos - 64);
            pos += n;
            return (int)(v & ((1UL << n) - 1));
        }
    }

    static int Subset(int ns, int part, int i) { return ns == 1 ? 0 : ns == 2 ? (P2[part] >> i) & 1 : (int)(P3[part] >> (2 * i)) & 3; }
    static bool Anchor(int ns, int part, int i) { return i == 0 || (ns == 2 && i == A2[part]) || (ns == 3 && (i == A3a[part] || i == A3b[part])); }
    static int Expand(int v, int bits) { v <<= 8 - bits; return v | (v >> bits); }

    // один блок 16 байт -> 16 пикселей RGBA (out[dst..dst+64))
    static void Block(byte[] b, int o, byte[] px)
    {
        var r = new Bits(b, o);
        int mode = 0; while (mode < 8 && r.Get(1) == 0) mode++;
        if (mode == 8) { Array.Clear(px, 0, 64); return; }
        int ns = M[mode, 0], pb = M[mode, 1], rb = M[mode, 2], isb = M[mode, 3], cb = M[mode, 4], ab = M[mode, 5], epb = M[mode, 6], spb = M[mode, 7], ib = M[mode, 8], ib2 = M[mode, 9];
        int part = r.Get(pb), rot = r.Get(rb), sel = r.Get(isb);
        var e = new int[6, 4];
        for (int c = 0; c < 3; c++) for (int k = 0; k < ns * 2; k++) e[k, c] = r.Get(cb);
        for (int k = 0; k < ns * 2; k++) e[k, 3] = ab > 0 ? r.Get(ab) : 255;
        int cbits = cb, abits = ab;
        if (epb == 1) { for (int k = 0; k < ns * 2; k++) { int p = r.Get(1); for (int c = 0; c < (ab > 0 ? 4 : 3); c++) e[k, c] = e[k, c] << 1 | p; } cbits++; if (ab > 0) abits++; }
        if (spb == 1) { for (int s = 0; s < ns; s++) { int p = r.Get(1); for (int k = 2 * s; k < 2 * s + 2; k++) for (int c = 0; c < 3; c++) e[k, c] = e[k, c] << 1 | p; } cbits++; }
        for (int k = 0; k < ns * 2; k++)
        {
            for (int c = 0; c < 3; c++) e[k, c] = Expand(e[k, c], cbits);
            if (ab > 0) e[k, 3] = Expand(e[k, 3], abits);
        }
        var i1 = new int[16]; var i2 = new int[16];
        for (int i = 0; i < 16; i++) i1[i] = r.Get(Anchor(ns, part, i) ? ib - 1 : ib);
        if (ib2 > 0) for (int i = 0; i < 16; i++) i2[i] = r.Get(i == 0 ? ib2 - 1 : ib2);
        for (int i = 0; i < 16; i++)
        {
            int s = Subset(ns, part, i), e0 = 2 * s, e1 = 2 * s + 1;
            int[] wc, wa; int ic, ia;
            if (ib2 == 0) { wc = wa = ib == 2 ? W2 : ib == 3 ? W3 : W4; ic = ia = i1[i]; }
            else if (sel == 0) { wc = ib == 2 ? W2 : W3; ic = i1[i]; wa = ib2 == 2 ? W2 : W3; ia = i2[i]; }
            else { wc = ib2 == 2 ? W2 : W3; ic = i2[i]; wa = ib == 2 ? W2 : W3; ia = i1[i]; }
            int[] v = new int[4];
            for (int c = 0; c < 3; c++) v[c] = ((64 - wc[ic]) * e[e0, c] + wc[ic] * e[e1, c] + 32) >> 6;
            v[3] = ((64 - wa[ia]) * e[e0, 3] + wa[ia] * e[e1, 3] + 32) >> 6;
            if (rot > 0) { int t = v[3]; v[3] = v[rot - 1]; v[rot - 1] = t; }
            for (int c = 0; c < 4; c++) px[i * 4 + c] = (byte)v[c];
        }
    }

    // весь mip0 -> RGBA (w*h*4)
    public static byte[] Decode(byte[] data, int w, int h)
    {
        var rgba = new byte[w * h * 4];
        int bw = (w + 3) / 4, bh = (h + 3) / 4;
        var px = new byte[64];
        for (int by = 0; by < bh; by++)
            for (int bx = 0; bx < bw; bx++)
            {
                int o = (by * bw + bx) * 16; if (o + 16 > data.Length) return rgba;
                Block(data, o, px);
                for (int y = 0; y < 4; y++)
                {
                    int yy = by * 4 + y; if (yy >= h) break;
                    for (int x = 0; x < 4; x++)
                    {
                        int xx = bx * 4 + x; if (xx >= w) break;
                        Buffer.BlockCopy(px, (y * 4 + x) * 4, rgba, (yy * w + xx) * 4, 4);
                    }
                }
            }
        return rgba;
    }

    // ---- кодировщик: режим 6 (RGBA 7+p бит, индексы 4 бита), главная ось блока + перебор p-бит ----
    public static byte[] Encode(byte[] rgba, int w, int h)
    {
        var out_ = new byte[(w / 4) * (h / 4) * 16];
        var blk = new int[16, 4];
        int n = 0;
        for (int by = 0; by < h / 4; by++)
            for (int bx = 0; bx < w / 4; bx++, n++)
            {
                for (int i = 0; i < 16; i++) for (int c = 0; c < 4; c++) blk[i, c] = rgba[((by * 4 + i / 4) * w + bx * 4 + i % 4) * 4 + c];
                EncodeBlock(blk, out_, n * 16);
            }
        return out_;
    }

    static void EncodeBlock(int[,] px, byte[] dst, int o)
    {
        var mean = new double[4];
        for (int i = 0; i < 16; i++) for (int c = 0; c < 4; c++) mean[c] += px[i, c] / 16.0;
        var cov = new double[4, 4];
        for (int i = 0; i < 16; i++) for (int a = 0; a < 4; a++) for (int b = 0; b < 4; b++) cov[a, b] += (px[i, a] - mean[a]) * (px[i, b] - mean[b]);
        // главная ось — степенной метод
        var ax = new double[] { 1, 1, 1, 1 };
        for (int it = 0; it < 24; it++)
        {
            var nx = new double[4];
            for (int a = 0; a < 4; a++) for (int b = 0; b < 4; b++) nx[a] += cov[a, b] * ax[b];
            double l = Math.Sqrt(nx[0] * nx[0] + nx[1] * nx[1] + nx[2] * nx[2] + nx[3] * nx[3]);
            if (l < 1e-9) { ax = new double[] { 0.5, 0.5, 0.5, 0.5 }; break; }
            for (int a = 0; a < 4; a++) ax[a] = nx[a] / l;
        }
        double tmin = double.MaxValue, tmax = double.MinValue;
        for (int i = 0; i < 16; i++) { double t = 0; for (int c = 0; c < 4; c++) t += (px[i, c] - mean[c]) * ax[c]; tmin = Math.Min(tmin, t); tmax = Math.Max(tmax, t); }
        var lo = new double[4]; var hi = new double[4];
        for (int c = 0; c < 4; c++) { lo[c] = Clamp(mean[c] + ax[c] * tmin, 0, 255); hi[c] = Clamp(mean[c] + ax[c] * tmax, 0, 255); }

        long bestErr = long.MaxValue; int[] bc0 = null, bc1 = null, bidx = null; int bp0 = 0, bp1 = 0;
        for (int p0 = 0; p0 < 2; p0++)
            for (int p1 = 0; p1 < 2; p1++)
            {
                int[] c0 = new int[4], c1 = new int[4], e0 = new int[4], e1 = new int[4];
                for (int c = 0; c < 4; c++)
                {
                    c0[c] = (int)Clamp(Math.Round((lo[c] - p0) / 2, MidpointRounding.ToEven), 0, 127);
                    c1[c] = (int)Clamp(Math.Round((hi[c] - p1) / 2, MidpointRounding.ToEven), 0, 127);
                    e0[c] = c0[c] * 2 + p0; e1[c] = c1[c] * 2 + p1;
                }
                var pal = new int[16, 4];
                for (int k = 0; k < 16; k++) for (int c = 0; c < 4; c++) pal[k, c] = ((64 - W4[k]) * e0[c] + W4[k] * e1[c] + 32) >> 6;
                var idx = new int[16]; long err = 0;
                for (int i = 0; i < 16; i++)
                {
                    long bd = long.MaxValue; int bk = 0;
                    for (int k = 0; k < 16; k++)
                    {
                        long d = 0; for (int c = 0; c < 4; c++) { long q = px[i, c] - pal[k, c]; d += q * q; }
                        if (d < bd) { bd = d; bk = k; }
                    }
                    idx[i] = bk; err += bd;
                }
                if (err < bestErr) { bestErr = err; bc0 = c0; bc1 = c1; bidx = idx; bp0 = p0; bp1 = p1; }
            }
        // якорный индекс (пиксель 0) — старший бит 0, иначе меняем концы
        if (bidx[0] >= 8)
        {
            var t = bc0; bc0 = bc1; bc1 = t; int tp = bp0; bp0 = bp1; bp1 = tp;
            for (int i = 0; i < 16; i++) bidx[i] = 15 - bidx[i];
        }
        ulong lo64 = 0, hi64 = 0; int pos = 0;
        Action<int, int> put = (val, bits) =>
        {
            ulong v = (ulong)(val & ((1 << bits) - 1));
            if (pos < 64) { lo64 |= v << pos; if (pos + bits > 64) hi64 |= v >> (64 - pos); }
            else hi64 |= v << (pos - 64);
            pos += bits;
        };
        put(1 << 6, 7);
        for (int c = 0; c < 4; c++) { put(bc0[c], 7); put(bc1[c], 7); }
        put(bp0, 1); put(bp1, 1);
        put(bidx[0], 3);
        for (int i = 1; i < 16; i++) put(bidx[i], 4);
        Buffer.BlockCopy(BitConverter.GetBytes(lo64), 0, dst, o, 8);
        Buffer.BlockCopy(BitConverter.GetBytes(hi64), 0, dst, o + 8, 8);
    }
    static double Clamp(double v, double a, double b) { return v < a ? a : v > b ? b : v; }
}

// картинки через System.Drawing (GDI+): RGBA <-> файл, масштаб
static class Img
{
    public static Bitmap FromRgba(byte[] rgba, int w, int h, bool alpha)
    {
        var bmp = new Bitmap(w, h, alpha ? PixelFormat.Format32bppArgb : PixelFormat.Format24bppRgb);
        var bd = bmp.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, bmp.PixelFormat);
        int bpp = alpha ? 4 : 3; var row = new byte[bd.Stride];
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                int s = (y * w + x) * 4, d = x * bpp;
                row[d] = rgba[s + 2]; row[d + 1] = rgba[s + 1]; row[d + 2] = rgba[s];
                if (alpha) row[d + 3] = rgba[s + 3];
            }
            Marshal.Copy(row, 0, bd.Scan0 + y * bd.Stride, bd.Stride);
        }
        bmp.UnlockBits(bd);
        return bmp;
    }

    public static byte[] ToRgba(Bitmap src, int w, int h)
    {
        using (var bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb))
        {
            using (var g = Graphics.FromImage(bmp))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                using (var ia = new ImageAttributes()) { ia.SetWrapMode(System.Drawing.Drawing2D.WrapMode.TileFlipXY); g.DrawImage(src, new Rectangle(0, 0, w, h), 0, 0, src.Width, src.Height, GraphicsUnit.Pixel, ia); }
            }
            var bd = bmp.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            var raw = new byte[w * h * 4];
            for (int y = 0; y < h; y++) Marshal.Copy(bd.Scan0 + y * bd.Stride, raw, y * w * 4, w * 4);
            bmp.UnlockBits(bd);
            for (int i = 0; i < raw.Length; i += 4) { byte t = raw[i]; raw[i] = raw[i + 2]; raw[i + 2] = t; }   // BGRA -> RGBA
            return raw;
        }
    }

    public static void Save(byte[] rgba, int w, int h, string path, int maxW = 0, bool alpha = false)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        using (var bmp = FromRgba(rgba, w, h, alpha))
        {
            Bitmap outB = bmp;
            if (maxW > 0 && w > maxW)
            {
                int nh = (int)Math.Round(h * (double)maxW / w);
                outB = new Bitmap(maxW, nh, alpha ? PixelFormat.Format32bppArgb : PixelFormat.Format24bppRgb);
                using (var g = Graphics.FromImage(outB))
                {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                    g.DrawImage(bmp, 0, 0, maxW, nh);
                }
            }
            try
            {
                if (path.EndsWith(".jpg"))
                {
                    var enc = Array.Find(ImageCodecInfo.GetImageEncoders(), c => c.FormatID == ImageFormat.Jpeg.Guid);
                    var ps = new EncoderParameters(1); ps.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 88L);
                    outB.Save(path, enc, ps);
                }
                else outB.Save(path, ImageFormat.Png);
            }
            finally { if (outB != bmp) outB.Dispose(); }
        }
    }
}
