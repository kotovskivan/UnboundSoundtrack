using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web.Script.Serialization;

// Песни по ссылке Spotify: песня / альбом / плейлист. Звук из Spotify НЕ берем (DRM, правила Spotify) — только список:
// исполнитель, название, длина. Каждую песню ищем на YT Music (раздел «Песни», Ytm.Search), звук качает обычный Yt.Get.
// Список — страница встраивания open.spotify.com/embed/<тип>/<id> (ее Spotify дает вставлять на сайты): JSON __NEXT_DATA__,
// без ключа и без входа. Неофициально — Spotify может поменять страницу: тогда spFail, в лог — что пришло.
// Плейлист / альбом: embed отдает не больше 100 песен (16.11: «Лучшие хиты 90-2000» — ровно 100) — окно просит выделить песни
// в Spotify (Ctrl+A, Ctrl+C) и вставить ссылки: каждая ссылка песни — свой embed, повторы окно пропускает.
// Скрытые в стране песни (COUNTRY_RESTRICTED) у Spotify бывают без имени и исполнителя даже в embed песни — hidden, окно их пропускает.
// Сверено 16.11.2026: альбом GODHANDUSA 10/10; плейлист Вани — 8 песен (кириллица, «- Single Version», «- Remastered 2009») через поиск YT Music.
static class Spot
{
    const string UA = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/140.0.0.0 Safari/537.36";
    public const int EmbedMax = 100;   // больше embed плейлиста / альбома не отдает
    static readonly JavaScriptSerializer J = new JavaScriptSerializer { MaxJsonLength = int.MaxValue, RecursionLimit = 1000 };
    static readonly Regex IdRe = new Regex(@"^[A-Za-z0-9]{22}$");
    static readonly Regex NextData = new Regex("<script id=\"__NEXT_DATA__\" type=\"application/json\">(.*?)</script>", RegexOptions.Singleline);

    // open.spotify.com/(intl-xx/)(embed/)<тип>/<id>(?si=…) или spotify:<тип>:<id>; тип — любой (артист / подкаст -> spKind в List)
    static bool Parse(string url, out string kind, out string id)
    {
        kind = id = null;
        url = (url ?? "").Trim();
        if (url.Length == 0 || url.Length > 500) return false;
        var m = Regex.Match(url, @"^spotify:([a-z]+):([A-Za-z0-9]{22})$", RegexOptions.IgnoreCase);
        if (m.Success) { kind = m.Groups[1].Value.ToLowerInvariant(); id = m.Groups[2].Value; return true; }
        Uri u;
        if (!Uri.TryCreate(url, UriKind.Absolute, out u) || (u.Scheme != "https" && u.Scheme != "http")) return false;
        if (u.Host.ToLowerInvariant() != "open.spotify.com") return false;
        var seg = u.AbsolutePath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        if (seg.Count > 0 && seg[0].StartsWith("intl-", StringComparison.OrdinalIgnoreCase)) seg.RemoveAt(0);
        if (seg.Count > 0 && seg[0].Equals("embed", StringComparison.OrdinalIgnoreCase)) seg.RemoveAt(0);
        if (seg.Count < 2 || !IdRe.IsMatch(seg[1])) return false;
        kind = seg[0].ToLowerInvariant(); id = seg[1];
        return true;
    }

    // ссылка -> {kind, title, by, items: [{sp, title, artist, dur, hidden}], cut — список мог упереться в предел embed}
    public static Dictionary<string, object> List(string url)
    {
        string kind, id;
        if (!Parse(url, out kind, out id)) throw new UserError("ytLink");
        if (kind != "track" && kind != "album" && kind != "playlist") throw new UserError("spKind");
        string page = Fetch("https://open.spotify.com/embed/" + kind + "/" + id);
        Dictionary<string, object> e = null;
        var m = NextData.Match(page);
        try { if (m.Success) e = At(J.Deserialize<Dictionary<string, object>>(m.Groups[1].Value), "props", "pageProps", "state", "data", "entity") as Dictionary<string, object>; }
        catch (Exception x) { App.Log("Spotify " + kind + " " + id + ": " + x.GetType().Name + " " + x.Message); }
        if (e == null)
        {
            App.Log("Spotify " + kind + " " + id + ": no __NEXT_DATA__ entity in the embed page (" + page.Length + " chars, data " + m.Success + ")");
            if (m.Success) throw new UserError("spGone");   // страница есть, списка нет: приватный / удаленный (проверено на выдуманном id)
            throw new UserError("spFail", "embed");         // страница другая — Spotify поменял embed
        }
        var items = new List<object>();
        if (kind == "track")
        {
            var arts = (At(e, "artists") as IEnumerable ?? new object[0]).OfType<Dictionary<string, object>>().Select(a => Str(a, "name")).Where(s => s != null);
            items.Add(Item(id, Str(e, "name") ?? Str(e, "title"), string.Join(", ", arts), Num(e, "duration")));
        }
        else
            foreach (var t in (At(e, "trackList") as IEnumerable ?? new object[0]).OfType<Dictionary<string, object>>())
            {
                string uri = Str(t, "uri") ?? "";
                if (!uri.StartsWith("spotify:track:")) continue;           // эпизоды подкастов в плейлисте — не песни
                items.Add(Item(uri.Substring(14), Str(t, "title"), Str(t, "subtitle"), Num(t, "duration")));
            }
        string title = Str(e, "name") ?? Str(e, "title") ?? "";
        int hidden = items.Count(x => (bool)((Dictionary<string, object>)x)["hidden"]);
        App.Log("Spotify " + kind + " " + id + " «" + title + "»: " + items.Count + " songs" + (hidden > 0 ? ", hidden in this country: " + hidden : ""));
        long free = -1;   // свободно на диске программы — окно сравнит с прикидкой веса (как у плейлиста YouTube)
        try { free = new DriveInfo(Path.GetPathRoot(Path.GetFullPath(App.Root))).AvailableFreeSpace; } catch { }
        return new Dictionary<string, object> { { "kind", kind }, { "title", title }, { "by", Str(e, "subtitle") ?? "" }, { "items", items },
                                                { "cut", kind != "track" && items.Count >= EmbedMax }, { "free", free } };
    }
    static Dictionary<string, object> Item(string sp, string title, string artist, double ms)
    {
        title = title ?? ""; artist = (artist ?? "").Replace(' ', ' ').Trim();
        return new Dictionary<string, object> { { "sp", sp }, { "title", title }, { "artist", artist }, { "dur", Math.Round(ms / 1000.0) }, { "hidden", title == "" } };
    }

    static string Fetch(string url)
    {
        // exe без app.config — .NET 4.8 по умолчанию говорит старым TLS; Spotify без TLS 1.2 рвет связь (SecureChannelFailure, 16.11), YouTube еще терпит
        ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
        for (int attempt = 0; ; attempt++)
        {
            try
            {
                var rq = (HttpWebRequest)WebRequest.Create(url);
                rq.UserAgent = UA; rq.Timeout = 15000; rq.ReadWriteTimeout = 15000;
                rq.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                using (var rs = (HttpWebResponse)rq.GetResponse())
                using (var sr = new StreamReader(rs.GetResponseStream(), Encoding.UTF8))
                    return sr.ReadToEnd();
            }
            catch (WebException x)
            {
                var hr = x.Response as HttpWebResponse;
                int code = hr != null ? (int)hr.StatusCode : 0;
                App.Log("Spotify " + url + " -> " + (code > 0 ? "HTTP " + code : x.Status.ToString()));
                if (code == 404 || code == 400) throw new UserError("spGone");                 // нет такого / приватный плейлист
                if (attempt == 0 && (code == 0 || code == 429 || code >= 500)) { Thread.Sleep(code == 429 ? 3000 : 1000); continue; }
                if (code == 0) throw new UserError("ytNet");
                throw new UserError("spFail", "HTTP " + code);
            }
        }
    }

    // ---- песня Spotify -> песня YT Music ----
    // Название: «Love Is A Shield - Single Version» (Spotify) = «Love Is A Shield (Single Version)» (YT Music) — без знаков одно и то же;
    // «Let It Be - Remastered 2009» и просто «Let It Be» — та же песня, другая версия (штраф 8, решает длина).
    // Исполнитель: у Spotify русские часто латиницей («Korni», «Gorod 312»), у YT Music — кириллицей («Корни», «Город 312»):
    // сравниваем «скелет» — транслит без гласных («krn» = «krn»). Чужой исполнитель — не отбрасываем (Шатунов у YT Music бывает
    // «Ласковый Май»), но тогда только «проверь». ok = исполнитель свой и длина ±3 с; иначе found, но ok = false (окно — желтым).
    public static Dictionary<string, object> Find(string artist, string title, double dur)
    {
        var no = new Dictionary<string, object> { { "found", false } };
        if (string.IsNullOrWhiteSpace(title)) return no;
        string full = T(title), bas = T(Base(title)), fullSk = Sk(title), basSk = Sk(Base(title));
        var arts = ArtSk(artist);
        Ytm.Info best = null, near = null; double bs = double.MaxValue, ns = double.MaxValue; bool bArt = false;
        string first = (artist ?? "").Split(',')[0].Trim();
        var qs = new List<string> { (first + " " + Base(title)).Trim() };
        if (Base(title) != title.Trim()) qs.Add((first + " " + title).Trim());   // ничего не нашлось — с полным названием
        foreach (string q in qs)
        {
            List<Ytm.Info> res = null;
            for (int attempt = 0; attempt < 2 && res == null; attempt++)
            {
                try { res = Ytm.Search(q, 10); }
                catch (Exception x) { App.Log("Spotify -> YT Music search «" + q + "»: " + x.GetType().Name + " " + x.Message); if (attempt == 0) Thread.Sleep(1200); }
            }
            if (res == null) throw new UserError("ytNet");
            foreach (var r in res)
            {
                string rt = T(r.Title), rb = T(Base(r.Title));
                double sc;
                // название другое, но исполнитель свой и длина ±2 с: у Spotify «Million scarlet roses» — у YT Music «Миллион алых роз»
                if (near == null && dur > 0 && r.Len > 0 && Math.Abs(r.Len - dur) <= 2 && arts.Count > 0 && r.Artists.Any(a => ArtSk(a).Overlaps(arts))
                    && Math.Abs(r.Len - dur) < ns) { ns = Math.Abs(r.Len - dur); near = r; }
                if (rt == full) sc = 0;
                else if (bas.Length > 0 && (rb == bas || rt == bas || rb == full)) sc = 8;
                else if (basSk.Length >= 3 && (Sk(r.Title) == fullSk || Sk(Base(r.Title)) == basSk)) sc = 12;   // транслит разный: «Nevesta?» / «Невеста?»
                else continue;
                bool art = arts.Count > 0 && r.Artists.Any(a => ArtSk(a).Overlaps(arts));
                if (!art) sc += 30;
                sc += dur > 0 && r.Len > 0 ? Math.Abs(r.Len - dur) : 10;
                if (sc < bs) { bs = sc; best = r; bArt = art; }
            }
            if (best != null) break;
        }
        bool byLen = best == null && near != null;
        if (byLen) { best = near; bArt = true; }
        if (best == null) { App.Log("Spotify -> YT Music «" + artist + " - " + title + "» " + dur + " s: not found"); return no; }
        double diff = dur > 0 && best.Len > 0 ? Math.Abs(best.Len - dur) : 99;
        bool ok = !byLen && bArt && diff <= 3;
        string why = ok ? "" : byLen ? "title" : !bArt ? "art" : "len";
        var have = Have();
        bool hv = have.Contains(best.Id) || have.Contains(Yt.Key(artist, title)) || have.Contains(Yt.Key(string.Join(", ", best.Artists), best.Title));
        App.Log("Spotify -> YT Music «" + artist + " - " + title + "» " + dur + " s -> " + best.Id + " «" + best.Title + "» " + string.Join(", ", best.Artists)
                + " " + best.Len + " s" + (ok ? "" : " (check: " + why + ")") + (hv ? " (already in)" : ""));
        return new Dictionary<string, object> { { "found", true }, { "id", best.Id }, { "title", best.Title ?? "" }, { "by", string.Join(", ", best.Artists) },
                                                { "len", best.Len }, { "ok", ok }, { "why", why }, { "have", hv } };
    }

    // что уже есть в программе (Yt.Have читает song.json всех папок Music) — на пачку поисков один раз в 15 с
    static HashSet<string> haveSet; static DateTime haveAt; static readonly object HaveLock = new object();
    static HashSet<string> Have()
    {
        lock (HaveLock)
        {
            if (haveSet == null || (DateTime.UtcNow - haveAt).TotalSeconds > 15) { haveSet = Yt.Have(); haveAt = DateTime.UtcNow; }
            return haveSet;
        }
    }

    // название без версии и мусора: «X - Remastered 2009» -> «X», «X (Из к/ф …)» -> «X», «X (feat. Y)» -> «X»
    static string Base(string t)
    {
        t = t ?? "";
        int k = t.IndexOf(" - ");
        if (k > 0) t = t.Substring(0, k);
        t = Regex.Replace(t, @"\s*[\[(][^\])]*[\])]", "");
        t = Regex.Replace(t, @"\s+\b(?:feat|ft|featuring)\b\.?.*$", "", RegexOptions.IgnoreCase);
        return t.Trim(' ', '-', '–', '—');
    }

    // транслит кириллицы (рус / укр / бел) в латиницу, без ударений и знаков: «Капают слёзы» = «kapayutslezy»
    static readonly Dictionary<char, string> Tr = new Dictionary<char, string> {
        {'а',"a"},{'б',"b"},{'в',"v"},{'г',"g"},{'ґ',"g"},{'д',"d"},{'е',"e"},{'ё',"e"},{'є',"ye"},{'ж',"zh"},{'з',"z"},{'и',"i"},{'і',"i"},{'ї',"yi"},
        {'й',"y"},{'к',"k"},{'л',"l"},{'м',"m"},{'н',"n"},{'о',"o"},{'п',"p"},{'р',"r"},{'с',"s"},{'т',"t"},{'у',"u"},{'ў',"u"},{'ф',"f"},{'х',"h"},
        {'ц',"ts"},{'ч',"ch"},{'ш',"sh"},{'щ',"sch"},{'ъ',""},{'ы',"y"},{'ь',""},{'э',"e"},{'ю',"yu"},{'я',"ya"} };
    static string T(string s)
    {
        s = (s ?? "").ToLowerInvariant().Replace("&", "and").Replace("$", "s").Normalize(NormalizationForm.FormC);
        var sb = new StringBuilder();
        foreach (char c in s)
        {
            string r;
            if (Tr.TryGetValue(c, out r)) { sb.Append(r); continue; }                  // кириллица — целой буквой (й = y, не и + знак)
            foreach (char d in c.ToString().Normalize(NormalizationForm.FormD))         // ė -> e, é -> e, ñ -> n
                if (CharUnicodeInfo.GetUnicodeCategory(d) != UnicodeCategory.NonSpacingMark && char.IsLetterOrDigit(d)) sb.Append(d);
        }
        return sb.ToString();
    }
    // «скелет»: транслит без гласных, kh = h, w = v, двойные — одной: «Yuri Shatunov» = «Юрий Шатунов», «Ruki Vverh» = «Руки Вверх»
    static string Sk(string s)
    {
        string t = T(s).Replace("kh", "h").Replace("ph", "f").Replace("ck", "k").Replace('w', 'v').Replace('q', 'k').Replace("x", "ks");
        var sb = new StringBuilder();
        foreach (char c in t)
        {
            if ("aeiouyj".IndexOf(c) >= 0) continue;
            if (sb.Length > 0 && sb[sb.Length - 1] == c) continue;
            sb.Append(c);
        }
        return sb.ToString();
    }
    static readonly Regex ArtSplit = new Regex(@",|&| x | vs\.? |\bfeat\.?|\bft\.?|\bwith\b| и | і ", RegexOptions.IgnoreCase);
    static HashSet<string> ArtSk(string s)
    {
        var h = new HashSet<string>();
        foreach (string x in ArtSplit.Split(s ?? "")) { string k = Sk(x); if (k.Length >= 2) h.Add(k); else if (T(x).Length >= 2) h.Add(T(x)); }
        return h;
    }

    static object At(object o, params string[] path)
    {
        foreach (string k in path) { var d = o as Dictionary<string, object>; object v; if (d == null || !d.TryGetValue(k, out v)) return null; o = v; }
        return o;
    }
    static string Str(Dictionary<string, object> d, string k)
    {
        object v; string s = d.TryGetValue(k, out v) ? v as string : null;
        return string.IsNullOrWhiteSpace(s) ? null : s.Trim();
    }
    static double Num(Dictionary<string, object> d, string k)
    {
        object v; if (!d.TryGetValue(k, out v) || v == null) return 0;
        try { return Convert.ToDouble(v, CultureInfo.InvariantCulture); } catch { return 0; }
    }
}
