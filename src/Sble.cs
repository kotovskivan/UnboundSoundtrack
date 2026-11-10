// Sble.cs — свой читатель/писатель банка NewWave (SBle) NFS Unbound. Разбор 10.11.2026 (reports/findings.md, «Формат SBle»).
// Модель: DSET -> поля (значения как сырые 64-бит), индексы Selection (сетка ячеек по ключам строк), справочные списки (рецепт из оригинала).
// Писатель раскладывает всё заново и строит цепочку перемещаемых указателей. Проверка: оригинал -> модель -> запись = байт в байт.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

class SbField
{
    public uint Hash; public byte Dt, St; public short Sp1; public long Sp2; public uint X20;
    public List<ulong> Vals = new List<ulong>();     // сырые значения (float — биты), для st3 — уже раскрытые из словаря
    public int Shift { get { return Sp1 & 0xFF; } }
    public int Size { get { return (Sp1 >> 8) & 0xFF; } }
}

class SbParam  // IndexParameterArray: значения ключа (номер трека, кусок…)
{
    public uint Hash; public int Type; public List<long> Vals = new List<long>();
}

class SbIndex
{
    public uint U1; public ushort U2; public List<int> ParamIdx = new List<int>();   // какие параметры (номера в списке параметров DSET)
    public bool HasArray;
}

class SbDset
{
    public uint Name, Unk1; public int Count; public ushort Num8;
    public List<SbField> Fields = new List<SbField>();
    public List<SbIndex> Indexes = new List<SbIndex>();
    public List<SbParam> Params = new List<SbParam>();
    public List<uint> Debug = new List<uint>();          // справочные списки: хэши полей по порядку (отсортированные уникальные сохраненные значения)
    public List<int[]> RowKeys;                           // для DSET с сеткой: ключ строки = индексы значений параметров первого индекса
}

class SbleBank
{
    public byte Align, Unk1; public uint Key, SbrType, Unk2; public byte[] HeadRest;  // HeadRest — байты заголовка 40..80
    public List<SbDset> Dsets = new List<SbDset>();
    public byte[] Data;   // область данных (GUID чанков и т.п.) с dataOffset до конца

    public SbDset this[uint name] { get { return Dsets.First(d => d.Name == name); } }
    public SbField F(uint ds, uint f) { return this[ds].Fields.First(x => x.Hash == f); }

    static uint U32(byte[] d, int o) { return BitConverter.ToUInt32(d, o); }
    static ushort U16(byte[] d, int o) { return BitConverter.ToUInt16(d, o); }

    // ---------------- чтение ----------------
    public static SbleBank Read(byte[] d)
    {
        if (U32(d, 0) != 0x656C4253) throw new Exception("не SBle");
        var b = new SbleBank();
        b.Align = d[8]; b.Unk1 = d[9];
        int n = U16(d, 10); b.Key = U32(d, 12); b.SbrType = U32(d, 16); b.Unk2 = U32(d, 20);
        int tab = (int)U32(d, 24), dataOff = (int)U32(d, 32);
        b.HeadRest = d.Skip(40).Take(40).ToArray();
        b.Data = d.Skip(dataOff).ToArray();
        for (int i = 0; i < n; i++)
        {
            int st = (int)U32(d, tab + i * 8);
            var ds = new SbDset { Name = U32(d, st + 8), Unk1 = U32(d, st + 12), Count = (int)U32(d, st + 0x38), Num8 = U16(d, st + 0x46) };
            int fc = U16(d, st + 0x3C), ic = U16(d, st + 0x3E), fo = U16(d, st + 0x40), io = U16(d, st + 0x42), po = U16(d, st + 0x44);
            for (int k = 0; k < fc; k++)
            {
                int p = st + fo + 24 * k;
                var f = new SbField { Hash = U32(d, p), Dt = d[p + 4], St = d[p + 5], Sp1 = BitConverter.ToInt16(d, p + 6), Sp2 = BitConverter.ToInt64(d, p + 8) };
                int toff = (int)U32(d, p + 16);
                for (int e = 0; e < ds.Count; e++) f.Vals.Add(ReadVal(d, f, toff, e));
                ds.Fields.Add(f);
            }
            int np = 0;
            var arrays = new List<int>();
            for (int k = 0; k < ic; k++)
            {
                int q = st + io + 32 * k;
                var ix = new SbIndex { U1 = U32(d, q + 24), U2 = U16(d, q + 28) };
                int cnt = U16(d, q + 30) >> 8;
                for (int j = 0; j < cnt; j++) ix.ParamIdx.Add(np + j);
                np += cnt;
                if (U32(d, q + 8) != 0 || U32(d, q + 16) != 0) throw new Exception("индекс: unkOffset/item3 не 0 — не умею");
                ix.HasArray = U32(d, q) != 0; arrays.Add((int)U32(d, q));
                ds.Indexes.Add(ix);
            }
            for (int k = 0; k < np; k++)
            {
                int q = st + po + 24 * k;
                uint cf = U32(d, q + 4); int cnt = (int)(cf & 0xFFFFFF);
                var pa = new SbParam { Hash = U32(d, q), Type = (int)(cf >> 24) };
                long mn = (int)U32(d, q + 8), mx = (int)U32(d, q + 12); int to = (int)U32(d, q + 16);
                if (pa.Type == 1) for (int e = 0; e < cnt; e++) pa.Vals.Add(mn + d[to + e]);
                else if (pa.Type == 0) for (int e = 0; e < cnt; e++) pa.Vals.Add(mn + e);
                else throw new Exception("параметр типа " + pa.Type + " — не умею");
                if (pa.Vals.Max() != mx) throw new Exception("параметр: макс не сошелся");
                ds.Params.Add(pa);
            }
            // ключи строк — из сетки первого индекса с массивом
            for (int k = 0; k < ic; k++)
            {
                if (!ds.Indexes[k].HasArray) continue;
                var ix = ds.Indexes[k]; int a = arrays[k];
                int cells = ix.ParamIdx.Aggregate(1, (s, pi) => s * ds.Params[pi].Vals.Count);
                if (ds.RowKeys == null)
                {
                    ds.RowKeys = new List<int[]>();
                    for (int c = 0; c < cells; c++)
                        for (int r = d[a + c]; r < d[a + c + 1]; r++) ds.RowKeys.Add(CellToKey(ds, ix, c));
                    if (ds.RowKeys.Count != ds.Count) throw new Exception("сетка покрывает " + ds.RowKeys.Count + " строк из " + ds.Count);
                }
            }
            b.Dsets.Add(ds);
        }
        // справочные списки: идут после таблиц каждого DSET, порядок берем из файла
        for (int i = 0; i < n; i++)
        {
            var ds = b.Dsets[i];
            int st = (int)U32(d, tab + i * 8), end = i + 1 < n ? (int)U32(d, tab + (i + 1) * 8) : dataOff;
            int p = st + (int)U32(d, st + 4);
            byte[] tables = b.TablesBlob(ds, p, new List<int>(), new List<int>(), new List<int>());
            p += tables.Length;
            while (true)
            {
                int q = Pad(p, 4); SbField hit = null;
                foreach (var f in ds.Fields.Where(f => f.St != 0 && !ds.Debug.Contains(f.Hash)))
                {
                    byte[] blob = DebugBlob(f);
                    if (blob.Length > 0 && q + blob.Length <= end && d.Skip(q).Take(blob.Length).SequenceEqual(blob)) { hit = f; p = q + blob.Length; break; }
                }
                if (hit == null) break;
                ds.Debug.Add(hit.Hash);
            }
        }
        return b;
    }

    static int[] CellToKey(SbDset ds, SbIndex ix, int c)
    {
        var key = new int[ix.ParamIdx.Count];
        for (int j = ix.ParamIdx.Count - 1; j >= 0; j--) { int m = ds.Params[ix.ParamIdx[j]].Vals.Count; key[j] = c % m; c /= m; }
        return key;
    }

    static ulong ReadVal(byte[] d, SbField f, int toff, int e)
    {
        switch (f.St)
        {
            case 0: return (ulong)f.Sp2;
            case 1: return (ulong)(f.Sp2 + (long)f.Sp1 * e);
            case 2:
                {
                    int sz = f.Size; ulong raw = 0;
                    for (int k = 0; k < sz; k++) raw |= (ulong)d[toff + sz * e + k] << (8 * k);
                    return (raw << f.Shift) + (ulong)f.Sp2;
                }
            case 3:
                {
                    int bits = f.Sp1 & 0xFF, sz = (f.Sp1 >> 8) & 0xFF;
                    int bp = toff + (int)f.Sp2 * sz + e * bits / 8;
                    int idx = (d[bp] >> (e * bits % 8)) & ((1 << bits) - 1);
                    ulong raw = 0;
                    for (int k = 0; k < sz; k++) raw |= (ulong)d[toff + idx * sz + k] << (8 * k);
                    return raw;
                }
        }
        throw new Exception("тип хранения " + f.St);
    }

    static int Pad(int p, int a) { return (p + a - 1) / a * a; }

    // сохраненные значения поля (то, что лежит в таблице / словаре)
    static List<ulong> Stored(SbField f)
    {
        if (f.St == 2) return f.Vals.Select(v => (v - (ulong)f.Sp2) >> f.Shift).ToList();
        if (f.St == 3) return f.Vals.Distinct().OrderBy(v => v).ToList();
        if (f.St == 1) return f.Vals.Select(v => v - (ulong)f.Sp2).ToList();
        return new List<ulong>();
    }

    static int SizeOf(ulong max) { return max <= 0xFF ? 1 : max <= 0xFFFF ? 2 : max <= 0xFFFFFFFF ? 4 : 8; }

    static void Put(List<byte> o, ulong v, int sz) { for (int k = 0; k < sz; k++) o.Add((byte)(v >> (8 * k))); }

    static byte[] DebugBlob(SbField f)
    {
        var s = Stored(f).Distinct().OrderBy(v => v).ToList();
        if (s.Count == 0) return new byte[0];
        int sz = f.St == 2 ? f.Size : f.St == 3 ? ((f.Sp1 >> 8) & 0xFF) : SizeOf(s.Max());
        var o = new List<byte>(); foreach (ulong v in s) Put(o, v, sz); return o.ToArray();
    }

    // пересчитать параметры хранения (база, размер) под текущие значения — тип хранения оставляем, если он еще подходит
    public static void Restore(SbField f)
    {
        var v = f.Vals;
        if (f.St == 0 && v.All(x => x == v[0])) { f.Sp2 = (long)v[0]; return; }
        if (f.St == 1 && v.Count > 1)
        {
            long step = (long)(v[1] - v[0]);
            if (step >= short.MinValue && step <= short.MaxValue && Enumerable.Range(0, v.Count).All(i => v[i] == (ulong)((long)v[0] + step * i))) { f.Sp1 = (short)step; f.Sp2 = (long)v[0]; return; }
        }
        if (f.St == 3)
        {
            int k = v.Distinct().Count(); int bits = k <= 2 ? 1 : k <= 4 ? 2 : k <= 16 ? 4 : 8;
            f.Sp1 = (short)((((f.Sp1 >> 8) & 0xFF) << 8) | bits); f.Sp2 = k; return;
        }
        // массив: база = минимум, сдвиг 0 (так в банках игры), размер по максимуму
        ulong mn = v.Min(); f.St = 2; f.Sp2 = (long)mn;
        int sz = Math.Max(SizeOf(v.Max(x => x - mn)), f.Size == 0 ? 1 : 0);
        f.Sp1 = (short)(sz << 8);
    }

    // таблицы полей + таблицы параметров одного DSET (позиции — в списки), base — где начинаются
    byte[] TablesBlob(SbDset ds, int bas, List<int> fieldPos, List<int> paramPos, List<int> dummy)
    {
        var o = new List<byte>();
        foreach (var f in ds.Fields)
        {
            if (f.St == 2)
            {
                while ((bas + o.Count) % f.Size != 0) o.Add(0);
                fieldPos.Add(bas + o.Count);
                foreach (ulong s in Stored(f)) Put(o, s, f.Size);
            }
            else if (f.St == 3)
            {
                int bits = f.Sp1 & 0xFF, sz = (f.Sp1 >> 8) & 0xFF;
                while ((bas + o.Count) % 4 != 0) o.Add(0);
                fieldPos.Add(bas + o.Count);
                var dict = Stored(f);
                foreach (ulong s in dict) Put(o, s, sz);
                var bitsArr = new byte[(f.Vals.Count * bits + 7) / 8];
                for (int e = 0; e < f.Vals.Count; e++) bitsArr[e * bits / 8] |= (byte)(dict.IndexOf(f.Vals[e]) << (e * bits % 8));
                o.AddRange(bitsArr);
            }
            else fieldPos.Add(0);
        }
        var done = new Dictionary<uint, int>();
        foreach (var pa in ds.Params)
        {
            if (pa.Type == 0) { paramPos.Add(0); continue; }
            if (done.ContainsKey(pa.Hash)) { paramPos.Add(done[pa.Hash]); continue; }
            int at = bas + o.Count; done[pa.Hash] = at; paramPos.Add(at);
            long mn = pa.Vals.Min();
            foreach (long x in pa.Vals) { if (x - mn > 255) throw new Exception("значение параметра не влезает в байт"); o.Add((byte)(x - mn)); }
        }
        return o.ToArray();
    }

    // массив индекса: для каждой ячейки — первая строка с ячейкой >= c, плюс замыкающий
    static byte[] IndexArray(SbDset ds, SbIndex ix)
    {
        var first = ds.Indexes.First(x => x.HasArray);
        int cells = ix.ParamIdx.Aggregate(1, (s, pi) => s * ds.Params[pi].Vals.Count);
        var cellOf = ds.RowKeys.Select(k =>
        {
            int c = 0;
            foreach (int pi in ix.ParamIdx)
            {
                int j = first.ParamIdx.FindIndex(x => ds.Params[x].Hash == ds.Params[pi].Hash);
                c = c * ds.Params[pi].Vals.Count + k[j];
            }
            return c;
        }).ToList();
        for (int r = 1; r < cellOf.Count; r++) if (cellOf[r] < cellOf[r - 1]) throw new Exception("строки не по порядку ячеек");
        if (ds.Count > 255) throw new Exception("строк " + ds.Count + " > 255 — сетка на байтах не влезет");
        var o = new byte[cells + 1];
        int row = 0;
        for (int c = 0; c <= cells; c++) { while (row < cellOf.Count && cellOf[row] < c) row++; o[c] = (byte)row; }
        return o;
    }

    // ---------------- запись ----------------
    public byte[] Write()
    {
        var o = new List<byte>();
        var links = new List<int>();     // адреса указателей (для цепочки)
        Action<int, uint> W32 = (p, v) => { var b = BitConverter.GetBytes(v); for (int k = 0; k < 4; k++) o[p + k] = b[k]; };
        Action<int, ushort> W16 = (p, v) => { var b = BitConverter.GetBytes(v); o[p] = b[0]; o[p + 1] = b[1]; };
        Action<int> Zero = (n) => { for (int k = 0; k < n; k++) o.Add(0); };
        Action<int> PadTo = (a) => { while (o.Count % a != 0) o.Add(0); };

        Zero(0x50);
        W32(0, 0x656C4253); o[8] = Align; o[9] = Unk1; W16(10, (ushort)Dsets.Count); W32(12, Key); W32(16, SbrType); W32(20, Unk2);
        for (int k = 0; k < 40; k++) o[40 + k] = HeadRest[k];
        links.Add(24); links.Add(32);
        int tab = o.Count; W32(24, (uint)tab);
        Zero(8 * Dsets.Count); for (int i = 0; i < Dsets.Count; i++) links.Add(tab + 8 * i);
        PadTo(16);
        var dataFix = new List<int>();   // места, куда вписать dataOffset
        dataFix.Add(32);
        for (int i = 0; i < Dsets.Count; i++)
        {
            var ds = Dsets[i];
            foreach (var f in ds.Fields) if (f.Vals.Count != ds.Count) throw new Exception("поле " + f.Hash.ToString("X8") + ": значений " + f.Vals.Count + " != " + ds.Count);
            int st = o.Count; W32(tab + 8 * i, (uint)st);
            int fo = 0x48, io = fo + 24 * ds.Fields.Count, po = io + 32 * ds.Indexes.Count, hsz = po + 24 * ds.Params.Count;
            Zero(hsz);
            W32(st, 0x44534554); /* байты "TESD" */ W32(st + 4, (uint)hsz); W32(st + 8, ds.Name); W32(st + 12, ds.Unk1);
            links.Add(st + 16); links.Add(st + 24); dataFix.Add(st + 24);
            W32(st + 0x38, (uint)ds.Count); W16(st + 0x3C, (ushort)ds.Fields.Count); W16(st + 0x3E, (ushort)ds.Indexes.Count);
            W16(st + 0x40, (ushort)fo); W16(st + 0x42, (ushort)(ds.Indexes.Count > 0 ? io : 0)); W16(st + 0x44, (ushort)(ds.Params.Count > 0 ? po : 0)); W16(st + 0x46, ds.Num8);
            var fpos = new List<int>(); var ppos = new List<int>();
            byte[] tables = TablesBlob(ds, o.Count, fpos, ppos, null);
            o.AddRange(tables);
            for (int k = 0; k < ds.Fields.Count; k++)
            {
                var f = ds.Fields[k]; int p = st + fo + 24 * k;
                W32(p, f.Hash); o[p + 4] = f.Dt; o[p + 5] = f.St; W16(p + 6, (ushort)f.Sp1);
                var b8 = BitConverter.GetBytes(f.Sp2); for (int j = 0; j < 8; j++) o[p + 8 + j] = b8[j];
                if (f.St == 2 || f.St == 3) { W32(p + 16, (uint)fpos[k]); links.Add(p + 16); }
            }
            for (int k = 0; k < ds.Params.Count; k++)
            {
                var pa = ds.Params[k]; int q = st + po + 24 * k;
                W32(q, pa.Hash); W32(q + 4, (uint)((pa.Type << 24) | pa.Vals.Count)); W32(q + 8, (uint)pa.Vals.Min()); W32(q + 12, (uint)pa.Vals.Max());
                if (pa.Type != 0) { W32(q + 16, (uint)ppos[k]); links.Add(q + 16); }
            }
            foreach (uint h in ds.Debug)
            {
                PadTo(4);
                o.AddRange(DebugBlob(ds.Fields.First(f => f.Hash == h)));
            }
            for (int k = 0; k < ds.Indexes.Count; k++)
            {
                var ix = ds.Indexes[k]; int q = st + io + 32 * k;
                W32(q + 24, ix.U1); W16(q + 28, ix.U2); W16(q + 30, (ushort)(ix.ParamIdx.Count << 8));
                if (ix.HasArray) { W32(q, (uint)o.Count); links.Add(q); o.AddRange(IndexArray(ds, ix)); }
            }
            PadTo(i + 1 < Dsets.Count ? 16 : 4);   // между DSET — 16; область данных в оригинале идет сразу (по 4)
        }
        int dataOff = o.Count;
        o.AddRange(Data);
        foreach (int p in dataFix) W32(p, (uint)dataOff);
        W32(4, (uint)o.Count);
        links.Sort();
        for (int k = 0; k < links.Count; k++) W32(links[k] + 4, k + 1 < links.Count ? (uint)links[k + 1] : 0xFFFFFFFF);
        return o.ToArray();
    }

    // ---------------- своя песня ----------------
    public const uint DS_SEL = 0x11D5B185, DS_VAR = 0xA29AF127, DS_SEG = 0x3FE2AFD5, DS_CHK = 0xA28D4A0D;
    const uint P_TRACK = 0x68A3404D, P_PIECE = 0xA4D852E6;
    const uint SEL_VARIDX = 0x6AC4E4EA, SEL_INTRO = 0xE37A5357, ID = 0xF5F914D9;
    const uint VAR_MEM = 0x4E5B3721, VAR_STREAM = 0x678C1CBC, VAR_FIRSTSEG = 0xE4660A62;
    const uint SEG_LEN = 0x6CFCCE5B, SEG_OFF = 0xE8E591DD, CHK_SIZE = 0xDC19107B, CHK_GUID = 0xF4369173;

    public class Piece { public int Value; public long Offset; public float Length; public bool Intro; public Guid MemGuid; public uint MemSize; }

    public long MaxTrack() { return this[DS_SEL].Params.First(p => p.Hash == P_TRACK).Vals.Max(); }
    public int SelRows() { return this[DS_SEL].Count; }
    public List<long> Tracks() { return this[DS_SEL].Params.First(p => p.Hash == P_TRACK).Vals.ToList(); }

    // убрать трек игры из сетки Selection (строки всех его кусков) — место своим песням.
    // Варианты, сегменты и чанки остаются: на них просто больше не ссылается ни одна строка. Возвращает, сколько строк освободилось.
    public int RemoveTrack(long track)
    {
        var sel = this[DS_SEL];
        var trackParams = sel.Params.Where(p => p.Hash == P_TRACK).ToList();
        int ti = trackParams[0].Vals.IndexOf(track);
        if (ti < 0) return 0;
        if (trackParams[0].Vals.Count <= 1) throw new Exception("последний трек банка не убираем");
        var keep = Enumerable.Range(0, sel.Count).Where(r => sel.RowKeys[r][0] != ti).ToList();
        int removed = sel.Count - keep.Count;
        foreach (var f in sel.Fields) f.Vals = keep.Select(r => f.Vals[r]).ToList();
        sel.RowKeys = keep.Select(r => { var k = (int[])sel.RowKeys[r].Clone(); if (k[0] > ti) k[0]--; return k; }).ToList();
        sel.Count = keep.Count;
        foreach (var p in trackParams) p.Vals.RemoveAt(ti);
        foreach (var f in sel.Fields) Restore(f);
        return removed;
    }

    // новый номер трека: строки Selection (по кускам), варианты, сегменты, memory-чанки; весь звук — в одном НОВОМ stream-чанке
    public void AddSong(int track, List<Piece> pieces, Guid streamGuid, uint streamSize)
    {
        var sel = this[DS_SEL]; var vr = this[DS_VAR]; var sg = this[DS_SEG]; var ch = this[DS_CHK];
        var trackParams = sel.Params.Where(p => p.Hash == P_TRACK).ToList();
        if (trackParams[0].Vals.Contains(track)) throw new Exception("номер " + track + " уже есть в банке");
        if (track <= trackParams[0].Vals.Max()) throw new Exception("номер должен быть больше " + trackParams[0].Vals.Max() + " (дописываем в конец сетки)");
        foreach (var p in trackParams) p.Vals.Add(track);
        var pieceVals = sel.Params.First(p => p.Hash == P_PIECE).Vals;
        int trackIdx = trackParams[0].Vals.Count - 1;

        // чанки: сначала stream, потом memory
        Action<Guid, uint> AddChunk = (g, size) =>
        {
            F(DS_CHK, CHK_SIZE).Vals.Add(size);
            var gv = F(DS_CHK, CHK_GUID).Vals; gv.Add(gv[gv.Count - 1] + 16);
            Data = Data.Concat(g.ToByteArray()).ToArray();
            ch.Count++;
        };
        int streamIdx = ch.Count; AddChunk(streamGuid, streamSize);

        var rnd = new Random(track * 7919 + 17);
        var used = new HashSet<ulong>(F(DS_VAR, ID).Vals.Concat(F(DS_SEL, ID).Vals));
        int tv = 0;                      // образец варианта
        int tsg = (int)F(DS_VAR, VAR_FIRSTSEG).Vals[tv];
        foreach (var pc in pieces.OrderBy(p => p.Value))
        {
            int memIdx = ch.Count; AddChunk(pc.MemGuid, pc.MemSize);
            // сегмент
            int segIdx = sg.Count;
            foreach (var f in sg.Fields) f.Vals.Add(f.Vals[tsg]);
            ulong flags = F(DS_SEG, SEG_OFF).Vals[tsg] & 3;
            F(DS_SEG, SEG_OFF).Vals[segIdx] = (ulong)pc.Offset | flags;
            F(DS_SEG, SEG_LEN).Vals[segIdx] = BitConverter.ToUInt32(BitConverter.GetBytes(pc.Length), 0);
            sg.Count++;
            // вариант
            ulong id; do { id = (ulong)(uint)rnd.Next(int.MinValue, int.MaxValue); } while (used.Contains(id) || id == 0); used.Add(id);
            int varIdx = vr.Count;
            foreach (var f in vr.Fields) f.Vals.Add(f.Vals[tv]);
            F(DS_VAR, VAR_MEM).Vals[varIdx] = ((ulong)memIdx << 1) | 1;
            F(DS_VAR, VAR_STREAM).Vals[varIdx] = ((ulong)streamIdx << 1) | 1;
            F(DS_VAR, VAR_FIRSTSEG).Vals[varIdx] = (ulong)segIdx;
            F(DS_VAR, ID).Vals[varIdx] = id;
            vr.Count++;
            // строка Selection
            int pieceIdx = pieceVals.IndexOf(pc.Value);
            if (pieceIdx < 0) throw new Exception("кусок " + pc.Value + " не из {" + string.Join(",", pieceVals) + "}");
            F(DS_SEL, SEL_VARIDX).Vals.Add((ulong)varIdx);
            F(DS_SEL, SEL_INTRO).Vals.Add(pc.Intro ? (ulong)BitConverter.ToUInt32(BitConverter.GetBytes(17.0f), 0) : 0UL);
            F(DS_SEL, ID).Vals.Add(id);
            sel.RowKeys.Add(new[] { trackIdx, pieceIdx });
            sel.Count++;
        }
        foreach (var ds in new[] { sel, vr, sg, ch }) foreach (var f in ds.Fields) Restore(f);
    }

    public static void Cmd(string[] args)
    {
        if (args[1] == "round")
        {
            byte[] a = File.ReadAllBytes(args[2]);
            var bank = Read(a);
            Console.WriteLine("прочитан: DSET " + bank.Dsets.Count + ", " + string.Join(", ", bank.Dsets.Select(x => x.Name.ToString("X8") + " x" + x.Count + " справочные [" + string.Join(" ", x.Debug.Select(h => h.ToString("X8"))) + "]")));
            byte[] b = bank.Write();
            if (args.Length > 3) File.WriteAllBytes(args[3], b);
            Console.WriteLine(a.SequenceEqual(b) ? "байт в байт" : "РАЗНИЦА");
        }
        else throw new Exception("sble: round <bank> [out]");
    }
}
