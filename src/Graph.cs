// Граф музыки без писателя Frosty (15.11.2026).
// EbxWriterRiff (FrostySdk, форк HarGabt 1.0.7) портит массивы структур с выравниванием 1: перед данными не остается места под счетчик,
// его затирают соседи. В графе audio/music/logic/exc_music_control так пропадает SamplerNodeData.Plugins у 12 из 13 проигрывателей,
// даже без единой правки, — и в игре молчит музыка погони (Ваня, 15.11; после подмены графа — «Заиграла музыка погони»).
// Поэтому: граф = байты игры + свои банки в ReferencedData корня (владелец — узел WaveSwitcherNodeData, как у ev02) и в WaveSwitcherNodeData.Waves.
// Frosty по-прежнему пишет граф в мод (ресурс, бандлы, зависимости), а после записи данные этого ресурса в .fbmod подменяются нашими байтами.
// Формат RIFF EBX (разобран 15.11): EBXD — данные (с 0x20), указатель = int32 от адреса поля + 4 нулевых байта, все места указателей — в EFIX ptr;
// импорт = (номер << 1) | 1 (+ 4 нуля), места — в EFIX impoff; массив — счетчик u32 за 4 байта до данных, список массивов — в EBXX.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FrostySdk;
using FrostySdk.IO;

static class GraphPatch
{
    class Arr { public int Off, Count; public byte[] Rest; }

    static int I32(byte[] b, int o) { return BitConverter.ToInt32(b, o); }
    static byte[] Sub(byte[] b, int o, int n) { var r = new byte[n]; Buffer.BlockCopy(b, o, r, 0, n); return r; }
    static void Tag(byte[] b, int o, string t) { if (Encoding.ASCII.GetString(b, o, 4) != t) throw new Exception("graph: no " + t + " at " + o); }
    static List<int> RList(byte[] b, ref int p) { int n = I32(b, p); p += 4; var v = new List<int>(n); for (int i = 0; i < n; i++) v.Add(I32(b, p + 4 * i)); p += 4 * n; return v; }

    // байты графа из игры + свои банки (FileGuid, Guid корня банка) -> новые байты графа
    public static byte[] Patch(byte[] b, IList<Tuple<Guid, Guid>> banks)
    {
        if (Encoding.ASCII.GetString(b, 0, 4) != "RIFF" || Encoding.ASCII.GetString(b, 8, 4) != "EBX\0") throw new Exception("graph: not RIFF EBX");
        Tag(b, 12, "EBXD");
        const int B = 0x20;
        int dsz = I32(b, 16), pend = 20 + dsz - B;
        byte[] pay = Sub(b, B, pend);
        int o = 20 + dsz; Tag(b, o, "EFIX"); int fsz = I32(b, o + 4), f0 = o + 8;
        o = f0 + fsz; Tag(b, o, "EBXX"); int xsz = I32(b, o + 4), x0 = o + 8;
        byte[] tail = Sub(b, x0 + xsz, b.Length - (x0 + xsz));
        // EFIX
        int p = f0;
        byte[] part = Sub(b, p, 16); p += 16;
        int ntg = I32(b, p); p += 4; byte[] tg = Sub(b, p, 16 * ntg); p += 16 * ntg;
        int nsig = I32(b, p); p += 4; byte[] sig = Sub(b, p, 4 * nsig); p += 4 * nsig;
        int exp = I32(b, p); p += 4;
        List<int> dc = RList(b, ref p), ptr0 = RList(b, ref p), res = RList(b, ref p);
        int nimp = I32(b, p); p += 4; var imp = new List<byte[]>(); for (int i = 0; i < nimp; i++) { imp.Add(Sub(b, p, 32)); p += 32; }
        List<int> impoff0 = RList(b, ref p), typeoff = RList(b, ref p);
        int arrOff = I32(b, p), boxOff = I32(b, p + 4), strOff = I32(b, p + 8); p += 12;
        byte[] etail = Sub(b, p, f0 + fsz - p);   // у игры 4 нуля в конце EFIX
        if (!(etail.Length == 0 || (etail.Length == 4 && etail.All(x => x == 0)))) throw new Exception("graph: EFIX tail " + etail.Length);
        // EBXX
        int na = I32(b, x0), nb = I32(b, x0 + 4);
        var arrs = new List<Arr>();
        for (int i = 0; i < na; i++) { int q = x0 + 8 + 16 * i; arrs.Add(new Arr { Off = I32(b, q), Count = I32(b, q + 4), Rest = Sub(b, q + 8, 8) }); }
        byte[] boxs = Sub(b, x0 + 8 + 16 * na, xsz - 8 - 16 * na);
        if (nb != 0 || boxOff != strOff || res.Count != 0 || typeoff.Count != 0) throw new Exception("graph: layout not as in 1.0.8.2549");
        var P = new HashSet<int>(ptr0); var I = new HashSet<int>(impoff0);
        Func<string, int> impIdx = fg => { Guid g = new Guid(fg); for (int k = 0; k < imp.Count; k++) if (new Guid(Sub(imp[k], 0, 16)) == g) return k; throw new Exception("graph: no import " + fg); };
        int ev01 = impIdx("b26f98ca-bc72-11ea-ba5b-a8c72fc53543"), ev02 = impIdx("bff5b511-22c3-11ed-9f7c-c9c18c0b87f8"), cred = impIdx("d821d62f-2491-11ed-be06-fc1b0bfeb41d");
        Func<int, int> enc = k => (k << 1) | 1;
        // ReferencedData корня: поле корня -> массив SoundDataReference {указатель-владелец, импорт банка}
        int root = dc[0], rdf = -1; Arr rda = null;
        foreach (int fp in ptr0.OrderBy(x => x))
        {
            if (fp < root || fp >= root + 0x80) continue;
            int t = fp + I32(pay, fp);
            foreach (var a in arrs)
                if (a.Off == t && a.Count >= 3 && Enumerable.Range(0, a.Count).All(k => I.Contains(t + 16 * k + 8) && P.Contains(t + 16 * k))) { rdf = fp; rda = a; }
        }
        if (rda == null) throw new Exception("graph: ReferencedData not found");
        // Waves: массив импортов [ev01, ev02, титры]
        var was = arrs.Where(a => a.Count == 3 && I32(pay, a.Off) == enc(ev01) && I32(pay, a.Off + 8) == enc(ev02) && I32(pay, a.Off + 16) == enc(cred)
                                  && Enumerable.Range(0, 3).All(k => I.Contains(a.Off + 8 * k))).ToList();
        if (was.Count != 1) throw new Exception("graph: Waves found " + was.Count);
        Arr wa = was[0];
        var wfs = ptr0.Where(fp => fp + I32(pay, fp) == wa.Off).ToList();
        if (wfs.Count != 1) throw new Exception("graph: Waves field found " + wfs.Count);
        int wf = wfs[0];
        int owner = -1;
        for (int k = 0; k < rda.Count; k++) if (I32(pay, rda.Off + 16 * k + 8) == enc(ev02)) owner = rda.Off + 16 * k + I32(pay, rda.Off + 16 * k);
        int nxt = dc.Where(d => d > owner).DefaultIfEmpty(arrOff).Min();
        if (owner < 0 || !dc.Contains(owner) || !(owner < wf && wf < nxt)) throw new Exception("graph: WaveSwitcherNodeData not found");
        // новые импорты
        var newk = new List<int>();
        foreach (var bk in banks) { imp.Add(bk.Item1.ToByteArray().Concat(bk.Item2.ToByteArray()).ToArray()); newk.Add(imp.Count - 1); }
        // новые массивы — в конец области массивов (перед строками): счетчик за 4 байта до данных, данные по 8
        int S = strOff; if (S > pend) throw new Exception("graph: strings");
        var ins = new MemoryStream(); int pos = S;
        Func<int, int, int> place = (count, len) =>
        {
            int d = (pos + 4 + 7) & ~7;
            ins.Write(new byte[d - 4 - pos], 0, d - 4 - pos); ins.Write(BitConverter.GetBytes(count), 0, 4); ins.Write(new byte[len], 0, len);
            pos = d + len; return d;
        };
        int nrd = rda.Count + banks.Count, nw = wa.Count + banks.Count;
        int rdNew = place(nrd, 16 * nrd), wNew = place(nw, 8 * nw);
        int pad = ((pos + 15) & ~15) - pos; ins.Write(new byte[pad], 0, pad); pos += pad;
        int delta = pos - S;
        byte[] np = new byte[pend + delta];
        Buffer.BlockCopy(pay, 0, np, 0, S); ins.ToArray().CopyTo(np, S); Buffer.BlockCopy(pay, S, np, S + delta, pend - S);
        Action<int, int> put = (at, v) => { BitConverter.GetBytes(v).CopyTo(np, at); BitConverter.GetBytes(0).CopyTo(np, at + 4); };
        for (int k = 0; k < rda.Count; k++)
        {
            int so = rda.Off + 16 * k, no = rdNew + 16 * k;
            put(no, (so + I32(pay, so)) - no); put(no + 8, I32(pay, so + 8));
            P.Remove(so); P.Add(no); I.Remove(so + 8); I.Add(no + 8);
        }
        for (int j = 0; j < newk.Count; j++) { int no = rdNew + 16 * (rda.Count + j); put(no, owner - no); put(no + 8, enc(newk[j])); P.Add(no); I.Add(no + 8); }
        for (int k = 0; k < wa.Count; k++) { int so = wa.Off + 8 * k, no = wNew + 8 * k; put(no, I32(pay, so)); I.Remove(so); I.Add(no); }
        for (int j = 0; j < newk.Count; j++) { int no = wNew + 8 * (wa.Count + j); put(no, enc(newk[j])); I.Add(no); }
        put(rdf, rdNew - rdf); put(wf, wNew - wf);
        foreach (int fp in ptr0)   // указатели в строки (за точкой вставки) — сдвиг
        {
            if (fp >= S) throw new Exception("graph: pointer after strings");
            if (fp == rdf || fp == wf) continue;
            if (fp + I32(pay, fp) >= S) put(fp, I32(pay, fp) + delta);
        }
        arrs.Remove(rda); arrs.Remove(wa);
        arrs.Add(new Arr { Off = rdNew, Count = nrd, Rest = rda.Rest }); arrs.Add(new Arr { Off = wNew, Count = nw, Rest = wa.Rest });
        for (int i = 1; i < arrs.Count; i++) if (arrs[i].Off < arrs[i - 1].Off) throw new Exception("graph: EBXX order");
        boxOff += delta; strOff += delta;
        // сборка
        var e = new MemoryStream(); var ew = new BinaryWriter(e);
        ew.Write(part); ew.Write(ntg); ew.Write(tg); ew.Write(nsig); ew.Write(sig); ew.Write(exp);
        foreach (var l in new[] { dc, P.OrderBy(x => x).ToList(), res }) { ew.Write(l.Count); foreach (int v in l) ew.Write(v); }
        ew.Write(imp.Count); foreach (var x in imp) ew.Write(x);
        foreach (var l in new[] { I.OrderBy(x => x).ToList(), typeoff }) { ew.Write(l.Count); foreach (int v in l) ew.Write(v); }
        ew.Write(arrOff); ew.Write(boxOff); ew.Write(strOff); ew.Write(etail); ew.Flush();
        byte[] efx = e.ToArray();
        var x2 = new MemoryStream(); var xw = new BinaryWriter(x2);
        xw.Write(arrs.Count); xw.Write(0); foreach (var a in arrs) { xw.Write(a.Off); xw.Write(a.Count); xw.Write(a.Rest); } xw.Write(boxs); xw.Flush();
        byte[] ebxx = x2.ToArray();
        var outS = new MemoryStream(); var w = new BinaryWriter(outS);
        w.Write(Encoding.ASCII.GetBytes("EBX\0EBXD")); w.Write(12 + np.Length); w.Write(new byte[12]); w.Write(np);
        w.Write(Encoding.ASCII.GetBytes("EFIX")); w.Write(efx.Length); w.Write(efx);
        if (outS.Length % 2 != 0) w.Write((byte)0);
        w.Write(Encoding.ASCII.GetBytes("EBXX")); w.Write(ebxx.Length); w.Write(ebxx); w.Write(tail); w.Flush();
        byte[] body = outS.ToArray();
        var full = new MemoryStream(); var fw = new BinaryWriter(full);
        fw.Write(Encoding.ASCII.GetBytes("RIFF")); fw.Write(body.Length); fw.Write(body); fw.Flush();
        App.Log("graph patch: payload " + pend + " -> " + np.Length + " B, banks +" + banks.Count + ", pointers " + P.Count + ", imports " + imp.Count);
        return full.ToArray();
    }

    // данные одного EBX в готовом .fbmod -> наши байты (на месте: новые данные в конец файла, запись таблицы данных, sha1 и size ресурса)
    // возвращает откат (старые запись таблицы, sha1 / size и длина файла) — на случай, если проверка после подмены не пройдет
    public static Action ReplaceInMod(string modPath, string name, byte[] raw)
    {
        Frosty.Core.Mod.BaseModResource res; long dataOffset; int dataCount; byte[] oldData;
        using (Stream s = File.OpenRead(modPath))
        {
            var r = new Frosty.Core.IO.FrostyModReader(s);
            if (!r.IsValid) throw new Exception("mod: not valid");
            r.ReadModDetails();
            var all = r.ReadResources();
            var hits = all.Where(x => x.Type == Frosty.Core.Mod.ModResourceType.Ebx && string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase)).ToList();
            if (hits.Count != 1) throw new Exception("mod: " + name + " found " + hits.Count);
            res = hits[0];
            if (all.Count(x => x.ResourceIndex == res.ResourceIndex) != 1) throw new Exception("mod: data index shared");
            var bf = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
            dataOffset = (long)typeof(Frosty.Core.IO.FrostyModReader).GetField("dataOffset", bf).GetValue(r);
            dataCount = (int)typeof(Frosty.Core.IO.FrostyModReader).GetField("dataCount", bf).GetValue(r);
            oldData = r.GetResourceData(res);
        }
        long tablePos = dataOffset + (long)res.ResourceIndex * 16, dataStart = dataOffset + (long)dataCount * 16;
        byte[] head;
        using (var fs = File.OpenRead(modPath))
        {
            var br = new BinaryReader(fs); fs.Position = tablePos; long off = br.ReadInt64(), size = br.ReadInt64();
            fs.Position = dataStart + off; if (!br.ReadBytes((int)size).SequenceEqual(oldData)) throw new Exception("mod: data table not as expected");
            head = new byte[(int)dataOffset]; fs.Position = 0; if (fs.Read(head, 0, head.Length) != head.Length) throw new Exception("mod: short header");
        }
        // как Frosty: sha1 ресурса — от сжатых данных, size — несжатый размер
        string st = res.Sha1.ToString();
        if (st != Utils.GenerateSha1(oldData).ToString()) throw new Exception("mod: sha1 rule");
        byte[] plain; using (var cr = new CasReader(new MemoryStream(oldData), null)) plain = cr.Read();
        if (res.Size != plain.Length) throw new Exception("mod: size rule");
        byte[] nd = Utils.CompressFile(raw);
        byte[] back; using (var cr = new CasReader(new MemoryStream(nd), null)) back = cr.Read();
        if (!back.SequenceEqual(raw)) throw new Exception("mod: compress roundtrip");
        byte[] key = Encoding.ASCII.GetBytes(res.Name + "\0").Concat(Hex(st)).ToArray();
        var at = new List<int>();
        for (int i = 0; i + key.Length <= head.Length; i++) { int k = 0; while (k < key.Length && head[i + k] == key[k]) k++; if (k == key.Length) at.Add(i); }
        if (at.Count != 1) throw new Exception("mod: resource header found " + at.Count);
        int shaPos = at[0] + res.Name.Length + 1;
        if (BitConverter.ToInt64(head, shaPos + 20) != res.Size) throw new Exception("mod: size field");
        long oldLen = new FileInfo(modPath).Length;
        byte[] oldTab = new byte[16], oldSha = Sub(head, shaPos, 28);
        using (var fs = File.OpenRead(modPath)) { fs.Position = tablePos; fs.Read(oldTab, 0, 16); }
        Action undo = () =>
        {
            using (var fs = new FileStream(modPath, FileMode.Open, FileAccess.ReadWrite))
            { fs.Position = tablePos; fs.Write(oldTab, 0, 16); fs.Position = shaPos; fs.Write(oldSha, 0, 28); fs.SetLength(oldLen); }
            App.Log("mod: " + name + " restored (Frosty graph)");
        };
        using (var fs = new FileStream(modPath, FileMode.Open, FileAccess.ReadWrite))
        {
            var bw = new BinaryWriter(fs);
            long newOff = fs.Length - dataStart;
            fs.Position = fs.Length; bw.Write(nd); bw.Flush();                     // сначала данные: упало дальше — мод остается прежним
            fs.Position = tablePos; bw.Write(newOff); bw.Write((long)nd.Length);
            fs.Position = shaPos; bw.Write(Hex(Utils.GenerateSha1(nd).ToString())); bw.Write((long)raw.Length); bw.Flush();
        }
        App.Log("mod: " + name + " replaced (" + plain.Length + " -> " + raw.Length + " B)");
        return undo;
    }

    static byte[] Hex(string h) { h = h.Replace("-", ""); var b = new byte[h.Length / 2]; for (int i = 0; i < b.Length; i++) b[i] = Convert.ToByte(h.Substring(2 * i, 2), 16); return b; }

    // проверка мода: граф читается, банков в Waves сколько надо, у проигрывателей не меньше плагинов, чем у игры
    public static void Check(string modPath, string name, byte[] orig, int waves)
    {
        byte[] data = null;
        using (Stream s = File.OpenRead(modPath))
        {
            var r = new Frosty.Core.IO.FrostyModReader(s); r.ReadModDetails();
            var e = r.ReadResources().First(x => x.Type == Frosty.Core.Mod.ModResourceType.Ebx && string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
            using (var cr = new CasReader(new MemoryStream(r.GetResourceData(e)), null)) data = cr.Read();
        }
        Func<byte[], EbxAsset> read = bytes => { using (var er = EbxReader.CreateReader(new MemoryStream(bytes), Frosty.Core.App.FileSystemManager, true)) return er.ReadAsset<EbxAsset>(); };
        Func<EbxAsset, int[]> plugins = a => a.Objects.Where(x => x.GetType().Name == "SamplerNodeData")
            .Select(x => ((System.Collections.IList)x.GetType().GetProperty("Plugins").GetValue(x)).Count).ToArray();
        EbxAsset got = read(data), was = read(orig);
        int[] pg = plugins(got), pw = plugins(was);
        if (pg.Length != pw.Length || pg.Where((n, i) => n < pw[i]).Any()) throw new Exception("mod check: Plugins lost (" + pg.Sum() + " of " + pw.Sum() + ")");
        object sw = got.Objects.First(x => x.GetType().Name == "WaveSwitcherNodeData");
        int wn = ((System.Collections.IList)sw.GetType().GetProperty("Waves").GetValue(sw)).Count;
        if (wn != waves) throw new Exception("mod check: Waves " + wn + " != " + waves);
        App.Log("mod check: graph ok, samplers " + pg.Length + ", plugins " + pg.Sum() + ", banks in WaveSwitcherNodeData " + wn);
    }
}
