// Рабочий процесс: "Unbound Soundtrack.exe --worker job.json". Пишет в stdout строки для окна:
//   STEP <ключ> [текст]   — шаг (ключ переводится в окне)
//   PCT <0..100>          — прогресс шага
//   ERR <ключ> [текст]    — ошибка, понятная человеку
//   OK <json>             — готово
// Задачи: extract — список треков, обложки и фоны из игры игрока (в zip нет файлов EA); build — собрать мод.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using FrostySdk;
using FrostySdk.Ebx;
using FrostySdk.IO;
using FrostySdk.Interfaces;
using FrostySdk.Managers;
using FrostySdk.Managers.Entries;
using FrostySdk.Resources;

class WLog : ILogger
{
    int lastPct = -1;
    public void Log(string t, params object[] v)
    {
        string s = Fmt(t, v);
        if (s.StartsWith("progress:"))
        {
            double d; if (double.TryParse(s.Substring(9).Trim(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out d))
            { int p = (int)d; if (p != lastPct) { lastPct = p; Worker.Say("PCT " + p); } }
            return;
        }
        App.Log("frosty: " + s);
    }
    public void LogWarning(string t, params object[] v) { App.Log("frosty WARN " + Fmt(t, v)); }
    public void LogError(string t, params object[] v) { App.Log("frosty ERR " + Fmt(t, v)); }
    static string Fmt(string t, object[] v) { try { return v == null || v.Length == 0 ? t : string.Format(t, v); } catch { return t; } }
}

static class Worker
{
    const string BANK = "audio/music/event/exc_music_event_02", BANK1 = "audio/music/event/exc_music_event_01";
    const string LOGIC = "audio/music/logic/pf_music_logic";
    const string CONTROL = "audio/music/logic/exc_music_control";   // звуковой граф музыки: список банков в WaveSwitcherNodeData
    // ГОНКИ — заезды, митапы, тейковеры; МІСТО — свободная езда и гараж (так написано в окне). Гараж был в RACE —
    // в гараже играли песни с галочкой «Гонки» (отзыв в Steam 12.11), теперь — «Місто»
    static readonly string[] RACE = { "IsMeetupTrack", "IsEventTrack", "IsTrialEscalationTrack", "IsTakeoverTrack" };
    static readonly string[] CITY = { "IsFreedriveTrack", "IsGarageTrack" };
    const string FREE = "IsFreedriveTrack";   // только для extract (tracks.json, справочно)
    // фоны окна: имя в игре -> файл
    static readonly string[][] BG = {
        new[] { "ui/textures/fullscreenbackgrounds/safehouses/exc_safehouse_garage_01", "photo.jpg", "1920" },
        new[] { "ui/textures/fullscreenbackgrounds/t_lightleak_01_bg", "leak.jpg", "1920" },
        new[] { "ui/textures/fullscreenbackgrounds/unbound/t_background_unbound_cmn_logo1", "logo.png", "0" },
        new[] { "ui/textures/fullscreenbackgrounds/t_halftone_01_bg", "halftone.png", "0" },
        new[] { "ui/textures/fullscreenbackgrounds/t_paintgrunge_bg", "grunge.png", "0" },
        new[] { "ui/textures/generic/dialog/dialog_dotoverlay", "dots.png", "0" },
        new[] { "ui/textures/generic/dialog/dialog_noiseoverlay", "noise.png", "2048" } };

    static AssetManager am;
    static Dictionary<string, object> job;

    public static void Say(string s) { Console.WriteLine(s); Console.Out.Flush(); App.Log("worker> " + s); }
    // прогресс по песням (счетчик + полоса в окне) — только окну, без log.txt (иначе +сотни строк на сборку)
    static void Tell(string step, int done, int all, bool log = false) { if (log) App.Log("worker> STEP " + step); Console.WriteLine("STEP " + step); Console.WriteLine("PCT " + (100 * done / Math.Max(1, all))); Console.Out.Flush(); }

    public static int Run(string jobPath)
    {
        try { Console.SetOut(new StreamWriter(Console.OpenStandardOutput(), new UTF8Encoding(false)) { AutoFlush = true }); } catch { }
        try
        {
            job = App.Json.Deserialize<Dictionary<string, object>>(File.ReadAllText(jobPath, Encoding.UTF8));
            string task = (string)job["task"];
            var mx = new System.Threading.Mutex(false, MutexName);
            bool got; try { got = mx.WaitOne(0); } catch (System.Threading.AbandonedMutexException) { got = true; }   // прошлый упал — мьютекс наш
            if (!got) { mx.Dispose(); throw new UserError("busyOld"); }
            busyMutex = mx;
            object par; if (job.TryGetValue("parent", out par) && par != null) WatchParent(Convert.ToInt32(par));
            App.Log("worker start: " + task);
            if (task == "extract") Extract();
            else if (task == "build") Build();
            else if (task == "bc7test")   // проверка декодера: raw BC7 -> png (сверка с Pillow)
                foreach (string f in Directory.GetFiles((string)job["dir"], "*.raw"))
                {
                    var m = App.Json.Deserialize<Dictionary<string, object>>(File.ReadAllText(Path.ChangeExtension(f, ".json")));
                    if (!((string)m["format"]).StartsWith("BC7")) continue;
                    int w = Convert.ToInt32(m["w"]), h = Convert.ToInt32(m["h"]);
                    Img.Save(Bc7.Decode(File.ReadAllBytes(f), w, h), w, h, Path.Combine((string)job["out"], Path.GetFileNameWithoutExtension(f) + ".png"), 0, true);
                }
            else throw new Exception("unknown task " + task);
            return 0;
        }
        catch (UserError e) { App.Log("worker user error: " + e); Say("ERR " + e.Key + (e.Arg != null ? " " + e.Arg.Replace("\n", " ") : "")); return 2; }
        catch (Exception e)
        {
            App.Log("worker crash: " + e);
            // P2-01: нет Visual C++ (FrostyHash.dll / libzstd / zlibwapi не грузятся; Oodle игры не загрузился — Frosty падает NullReference
            // в DecompressOodle) — окно предложит установщик из app\vcredist
            for (var x = e; x != null; x = x.InnerException)
                if ((x is FileNotFoundException && x.Message.Contains("FrostyHash") && File.Exists(Path.Combine(App.FrostyDir, "FrostyHash.dll")))
                    || (x is DllNotFoundException && (x.Message.Contains("zstd") || x.Message.Contains("zlib")))
                    || (x is NullReferenceException && (x.StackTrace ?? "").Contains("Oodle")))
                { Say("ERR vcMissing " + x.GetType().Name); return 1; }
            Say("ERR crash " + e.GetType().Name + ": " + e.Message.Replace("\n", " ")); return 1;
        }
        finally { if (busyMutex != null) try { busyMutex.ReleaseMutex(); } catch { } }
    }

    // Ваня 13.11: «вышел из программы — все останавливается». Программа закрылась / ее убили — выходим и мы (и гасим кодировщик Tool.exe).
    // Мод в Mods при этом цел: новый подменяет старый одним шагом в самом конце (Build, .part)
    static void WatchParent(int pid)
    {
        System.Diagnostics.Process parent;
        try { parent = System.Diagnostics.Process.GetProcessById(pid); } catch { App.Log("worker: program already closed - stopping"); Environment.Exit(3); return; }
        new System.Threading.Thread(() =>
        {
            try { parent.WaitForExit(); } catch { return; }
            App.Log("worker: program closed - stopping");
            try { var c = Audio.Current; if (c != null && !c.HasExited) c.Kill(); } catch { }
            Environment.Exit(3);
        }) { IsBackground = true }.Start();
    }

    // P2-05: рабочий процесс от прошлого запуска еще жив (не успел выйти / упала программа) — новая программа не должна запускать второй поверх (A.sek.tmp занят)
    public static string MutexName { get { return "UnboundSoundtrack-worker-" + (App.Root.ToLowerInvariant().GetHashCode() & 0x7fffffff); } }
    static System.Threading.Mutex busyMutex;

    // ---------- загрузка игры через Frosty ----------
    static void Load()
    {
        string game = (string)job["game"], fmm = (string)job["fmm"];
        if (!Find.IsGame(game)) throw new UserError("noGame");
        if (!Find.IsFmm(fmm)) throw new UserError("noFmm");
        string keyPath = Find.KeyPath(fmm);
        if (!File.Exists(keyPath)) throw new UserError("noKey", fmm);
        Directory.SetCurrentDirectory(App.FrostyDir);
        string caches = Path.Combine(App.FrostyDir, "Caches"); Directory.CreateDirectory(caches);
        string myCache = Path.Combine(caches, "nfsunbound.cache"), fmmCache = Path.Combine(fmm, "Caches", "nfsunbound.cache");
        bool fresh = false;
        if (!File.Exists(myCache) && File.Exists(fmmCache))
        {
            Say("STEP copyCache");
            try { File.Copy(fmmCache, myCache + ".tmp", true); File.Move(myCache + ".tmp", myCache); } catch (Exception e) { App.Log("copy cache: " + e.Message); }
        }
        if (!File.Exists(myCache)) fresh = true;
        Say(fresh ? "STEP firstLoad" : "STEP loadGame");

        File.WriteAllText(Path.Combine(App.FrostyDir, "config.json"),
            App.Json.Serialize(new Dictionary<string, object> {
                { "Games", new Dictionary<string, object> { { App.Profile, new Dictionary<string, object> { { "GamePath", game }, { "Options", new Dictionary<string, object>() }, { "Packs", new Dictionary<string, object>() } } } } },
                { "GlobalOptions", new Dictionary<string, object>() } }), new UTF8Encoding(false));

        var lg = new WLog();
        ProfilesLibrary.Initialize(new List<Profile>());
        if (!ProfilesLibrary.SelectProfile(App.Profile)) throw new Exception("profile " + App.Profile + " not selected");
        byte[] key = File.ReadAllBytes(keyPath);
        KeyManager.Instance.AddKey("Key1", key); KeyManager.Instance.AddKey("Key2", key); KeyManager.Instance.AddKey("Key3", key);
        TypeLibrary.Initialize();
        var fs = new FileSystemManager(game.TrimEnd('\\') + "\\");
        foreach (FileSystemSource s in ProfilesLibrary.Sources) fs.AddSource(s.Path, s.SubDirs);
        fs.Initialize(key);
        var rm = new ResourceManager(fs); rm.SetLogger(lg); rm.Initialize();
        am = new AssetManager(fs, rm); am.SetLogger(lg);
        am.Initialize(false, null);
        Frosty.Core.App.FileSystemManager = fs; Frosty.Core.App.ResourceManager = rm; Frosty.Core.App.AssetManager = am;
        Frosty.Core.App.Logger = new Lg();
        App.Log("game loaded: ebx " + am.GetEbxCount());
        if (am.GetEbxEntry(LOGIC) == null || am.GetResEntry(BANK) == null) throw new UserError("gameChanged", "no " + LOGIC);
    }

    // ---------- extract ----------
    static void Extract()
    {
        Load();
        Say("STEP readTracks");
        EbxAsset logic = am.GetEbx(am.GetEbxEntry(LOGIC));
        var list = TrackList(logic);
        var tracks = new List<object>();
        var arts = new List<Tuple<string, string>>();
        foreach (object t in list)
        {
            if (!Str(t, "TitleId").StartsWith("ID_MUSICTRACK_")) continue;   // сюжетные и титры не трогаем
            string slot = Str(t, "TrackEnumName");
            string art = ArtName(t);
            tracks.Add(new Dictionary<string, object> {
                { "slot", slot }, { "num", Convert.ToInt32(t.GetType().GetProperty("TrackEnumInt").GetValue(t)) },
                { "race", RACE.Select(f => EnumVal(t, f)).ToArray() }, { "free", EnumVal(t, FREE) }, { "art", art } });
            if (art != null) arts.Add(Tuple.Create(slot, art));
        }
        string ui = App.UiDir; Directory.CreateDirectory(Path.Combine(ui, "covers")); Directory.CreateDirectory(Path.Combine(ui, "bg"));
        Say("STEP covers");
        int i = 0;
        foreach (var a in arts)
        {
            Say("PCT " + (100 * i++ / Math.Max(1, arts.Count)));
            try { SaveTexture(a.Item2, Path.Combine(ui, "covers", a.Item1 + ".jpg"), 0, false); }
            catch (Exception e) { App.Log("cover " + a.Item1 + ": " + e.Message); }
        }
        Say("STEP backgrounds");
        i = 0;
        foreach (var b in BG)
        {
            Say("PCT " + (100 * i++ / BG.Length));
            try { SaveTexture(b[0], Path.Combine(ui, "bg", b[1]), int.Parse(b[2]), b[1].EndsWith(".png")); }
            catch (Exception e) { App.Log("background " + b[0] + ": " + e.Message); }
        }
        App.WriteJson(Path.Combine(ui, "tracks.json"), new Dictionary<string, object> {
            { "fingerprint", (string)job["fingerprint"] }, { "app", App.Version }, { "tracks", tracks }, { "when", DateTime.Now.ToString("s") } });
        Say("OK {\"tracks\":" + tracks.Count + "}");
    }

    static void SaveTexture(string ebxName, string path, int maxW, bool alpha)
    {
        EbxAssetEntry e = am.GetEbxEntry(ebxName);
        if (e == null) throw new Exception("missing " + ebxName);
        dynamic root = am.GetEbx(e).RootObject;
        ResAssetEntry re = am.GetResEntry((ulong)root.Resource);
        Texture t = am.GetResAs<Texture>(re);
        string fmt = t.PixelFormat.ToString();
        if (!fmt.StartsWith("BC7")) throw new Exception("format " + fmt);
        byte[] data; using (var ms = new MemoryStream()) { t.Data.Position = 0; t.Data.CopyTo(ms); data = ms.ToArray(); }
        int w = t.Width, h = t.Height;
        int mip0 = ((w + 3) / 4) * ((h + 3) / 4) * 16;
        if (data.Length < mip0) throw new Exception("not enough data " + data.Length + " < " + mip0);
        Img.Save(Bc7.Decode(data, w, h), w, h, path, maxW, alpha);
    }

    // ---------- build ----------
    static void Build()
    {
        var songs = ((ArrayList)job["songs"]).Cast<Dictionary<string, object>>().Select(d => App.Json.ConvertToType<Song>(d)).ToList();
        var choiceRaw = (Dictionary<string, object>)job["choice"];
        var choice = choiceRaw.ToDictionary(k => k.Key, k => ((ArrayList)k.Value).Cast<object>().Select(Convert.ToBoolean).ToArray());
        string outPath = (string)job["out"];

        // 1) звук своих песен
        var encDirs = new List<string>();
        int n = 0;
        foreach (var s in songs)
        {
            n++;
            Tell("encode " + n + "/" + songs.Count + " · " + Clean(s.title), n - 1, songs.Count);   // у песни из кэша окно иначе ничего не видит
            encDirs.Add(Audio.Prepare(s, piece => Say("STEP encode " + n + "/" + songs.Count + " · " + Clean(s.title) + " · " + piece)));
        }

        // 2) игра
        Load();
        if (songs.Count > 0) AddBanks(songs, encDirs, GiveAway(choice));

        // 3) логика: галочки треков игры + свои треки с обложками
        Say("STEP logic");
        EbxAsset logic = am.GetEbx(am.GetEbxEntry(LOGIC));
        var list = TrackList(logic);
        int nr = 0, nf = 0;
        foreach (object t in list)
        {
            if (!Str(t, "TitleId").StartsWith("ID_MUSICTRACK_")) continue;
            bool[] c; if (!choice.TryGetValue(Str(t, "TrackEnumName"), out c) || c.Length < 2) c = new[] { true, true };
            // включено = High (все включенные треки и свои песни на равных), выключено = Never.
            // Раньше было «как в игре, Never -> Low»: тест Вани — из Siri (Low) и Palace (High) в игре звучал только Palace.
            int pv = c.Length > 2 && c[2] ? 1 : 2;   // приоритет: 2 High (по умолчанию) / 1 Low — Ваня 12.11 (Mid в игре нет: 0 Never, 1 Low, 2 High)
            foreach (string f in RACE) SetEnumInt(t, f, c[0] ? pv : 0);
            foreach (string f in CITY) SetEnumInt(t, f, c[1] ? pv : 0);
            // у 38 из 72 треков игры IsDirectlySelectable = False (Siri среди них). Тест Вани: из Siri + Palace звучал только Palace (True).
            // Мод перебивает игру: включенный трек — «выбираемый».
            if (c[0] || c[1]) t.GetType().GetProperty("IsDirectlySelectable").SetValue(t, true);
            if (c[0]) nr++; if (c[1]) nf++;
        }
        for (int i = 0; i < songs.Count; i++)
        {
            var s = songs[i];
            Tell("cover " + (i + 1) + "/" + songs.Count + " · " + Clean(s.title), i, songs.Count, true);   // в лог — как раньше: видно, на какой обложке упало
            string gt = Game(Music.Cut(Clean(s.title), Music.MaxTitle));   // песни, сохраненные до лимита (13.11), тоже не длиннее
            object nt = AppendTrack(list, "Siri", s.trackName, s.num, gt != "" ? gt : "Track " + s.num, Game(Music.Cut(Clean(s.artist), Music.MaxArtist)));
            int sv = s.low ? 1 : 2;
            foreach (string f in RACE) SetEnumInt(nt, f, s.race ? sv : 0);
            foreach (string f in CITY) SetEnumInt(nt, f, s.city ? sv : 0);
            if (s.race) nr++; if (s.city) nf++;
            var art = Cover("ui/textures/albumart/t_albumart_oki", "ui/textures/albumart/t_albumart_" + s.trackName.ToLowerInvariant(), CoverPixels(s));
            nt.GetType().GetProperty("AlbumArt").SetValue(nt, new PointerRef(new EbxImportReference { FileGuid = art.Item1, ClassGuid = art.Item2 }));
            logic.AddDependency(art.Item1);
        }
        // правила повтора движка (MinTracksBetween=5, HighSuitabilityMinTracksBetween=20) рассчитаны на 72 трека.
        // Тест Вани: из 2 включенных (Siri + Palace) везде играл один Palace. Подгоняем под свой плейлист: не больше N-1.
        int nOn = Math.Min(nr > 0 ? nr : int.MaxValue, nf > 0 ? nf : int.MaxValue);
        if (nOn != int.MaxValue)
            foreach (object o in logic.Objects)
                foreach (string f in new[] { "MinTracksBetween", "HighSuitabilityMinTracksBetween" })
                {
                    var pi = o.GetType().GetProperty(f); if (pi == null) continue;
                    long cur = Convert.ToInt64(pi.GetValue(o)), lim = Math.Max(0, nOn - 1);
                    if (cur > lim) { pi.SetValue(o, Convert.ChangeType(lim, pi.PropertyType)); App.Log("logic: " + f + " " + cur + " -> " + lim + " (tracks on: " + nOn + ")"); }
                }
        am.ModifyEbx(LOGIC, logic);

        // 4) мод
        Say("STEP writeMod");
        var ms = new Frosty.Core.Mod.ModSettings();
        ms.Title = "Unbound Soundtrack"; ms.Author = "Kitivski + Claude"; ms.Category = "Audio"; ms.Version = App.Version; ms.Link = "https://kotovskivan.github.io/UnboundSoundtrack/";   // Frosty Mod Manager показывает ссылку в описании мода
        ms.Description = (job.ContainsKey("preset") ? "Profile: " + Clean((string)job["preset"]) + ". " : "") + (songs.Count > 0 ? "Own songs: " + string.Join(", ", songs.Select(s => Clean(s.artist) + " - " + Clean(s.title))) + ". " : "")
            + "Game tracks in races: " + nr + ", in the city: " + nf + ". Made with Unbound Soundtrack (fan-made, not affiliated with EA).";
        ms.Icon = ModIcon();
        Frosty.Core.Config.Load(Path.Combine(App.FrostyDir, "config.json"));
        Frosty.Core.App.SelectedProfile = App.Profile;
        if (Frosty.Core.App.PluginManager == null) Frosty.Core.App.PluginManager = new Frosty.Core.PluginManager(new Lg(), (Frosty.Core.PluginManagerType)0);
        string tmp = Path.Combine(App.DataDir, "build.fbmod");
        if (File.Exists(tmp)) File.Delete(tmp);
        new Frosty.Core.FrostyProject().WriteToMod(tmp, ms);
        if (!File.Exists(tmp) || new FileInfo(tmp).Length < 1000) throw new Exception("mod was not written");
        Directory.CreateDirectory(Path.GetDirectoryName(outPath));
        // копия рядом (.part — Mod Manager его не видит) и подмена одним шагом: остановили / вышли посреди копирования — старый мод в Mods цел (Ваня 13.11: «Стоп»)
        string part = outPath + ".part";
        try { File.Copy(tmp, part, true); if (File.Exists(outPath)) File.Replace(part, outPath, null); else File.Move(part, outPath); }
        catch (IOException e) { App.Log("copy mod: " + e.Message); try { File.Delete(part); } catch { } throw new UserError("modLocked", outPath); }
        File.Delete(tmp);
        Say("OK " + App.Json.Serialize(new Dictionary<string, object> { { "path", outPath }, { "size", new FileInfo(outPath).Length }, { "race", nr }, { "city", nf }, { "songs", songs.Count } }));
    }

    static string Clean(string s) { s = (s ?? "").Replace('\r', ' ').Replace('\n', ' ').Trim(); return s.Length > 80 ? s.Substring(0, 80) : s; }

    // Текст на плашке в игре — только ASCII: кириллицу и значки шрифт игры / запись строки Frosty не проверены (Ваня: «давай транслитировать»).
    // Кириллица → латиница (укр. транслит), буквы с диакритикой → без нее (ë → e, ł → l), остальное (эмодзи, †) — выкинуть.
    // В программе и в song.json имена — как есть; это только для игры.
    static readonly Dictionary<char, string> Tr = new Dictionary<char, string> {
        {'а',"a"},{'б',"b"},{'в',"v"},{'г',"h"},{'ґ',"g"},{'д',"d"},{'е',"e"},{'є',"ye"},{'ж',"zh"},{'з',"z"},{'и',"y"},{'і',"i"},
        {'ї',"yi"},{'й',"y"},{'к',"k"},{'л',"l"},{'м',"m"},{'н',"n"},{'о',"o"},{'п',"p"},{'р',"r"},{'с',"s"},{'т',"t"},{'у',"u"},
        {'ф',"f"},{'х',"kh"},{'ц',"ts"},{'ч',"ch"},{'ш',"sh"},{'щ',"shch"},{'ь',""},{'ю',"yu"},{'я',"ya"},{'ы',"y"},{'э',"e"},
        {'ё',"yo"},{'ъ',""},{'ł',"l"},{'ß',"ss"},{'æ',"ae"},{'ø',"o"},{'đ',"d"},{'œ',"oe"},{'þ',"th"} };
    static string Game(string s)
    {
        s = s ?? "";
        var a = new System.Text.StringBuilder();
        for (int i = 0; i < s.Length; i++)
        {
            char c = s[i]; string t;
            if (!Tr.TryGetValue(char.ToLowerInvariant(c), out t)) { a.Append(c); continue; }
            if (char.IsUpper(c) && t.Length > 0)    // «Щ» в «ЩЕ» → SHCH, в «Ще» → Shch
                t = (i + 1 < s.Length && char.IsUpper(s[i + 1])) || (i > 0 && char.IsUpper(s[i - 1])) ? t.ToUpperInvariant() : char.ToUpperInvariant(t[0]) + t.Substring(1);
            a.Append(t);
        }
        var b = new System.Text.StringBuilder();
        foreach (char c in a.ToString().Normalize(System.Text.NormalizationForm.FormD))
        {
            if (c >= ' ' && c <= '~') b.Append(c);
            else if (char.IsWhiteSpace(c)) b.Append(' ');
            // знак над буквой (ë = e + ¨), эмодзи, значки, иероглифы — выкидываем
        }
        return System.Text.RegularExpressions.Regex.Replace(b.ToString(), @"\s+", " ").Trim();
    }

    // ---------- банки музыки ----------
    // Сетка Selection банка — на байтах: ≤ 255 строк. Песня = 3 строки (B старт, C финиш, A полная).
    // Сначала ev02 (проверено в игре), потом ev01 (у него своя сетка). Не хватает — забираем строки ВЫКЛЮЧЕННЫХ треков игры
    // из списка «можно отдавать» (их никто не вызывает напрямую — разбор FrostyReader trackscan), по одному, сколько нужно.
    class BankWork
    {
        public string Name; public ResAssetEntry Re; public byte[] Orig; public SbleBank Bank; public Func<byte[], Guid> NewChunk; public bool Changed; public int Songs;
        public bool IsNew; public List<long> Tpl; public long First = -1; public Guid FileGuid, Root;   // свой банк: треки образца, первый свой номер, GUID ассета
    }

    static BankWork OpenBank(string name)
    {
        var w = new BankWork { Name = name, Re = am.GetResEntry(name) };
        if (w.Re == null) throw new UserError("gameChanged", "no " + name);
        using (Stream s = am.GetRes(w.Re)) using (var ms = new MemoryStream()) { s.CopyTo(ms); w.Orig = ms.ToArray(); }
        w.Bank = SbleBank.Read(w.Orig);
        if (!w.Bank.Write().SequenceEqual(w.Orig)) throw new UserError("gameChanged", "bank round-trip " + name);
        var guids = w.Bank.F(SbleBank.DS_CHK, 0xF4369173).Vals;
        ChunkAssetEntry tc = am.GetChunkEntry(new Guid(w.Bank.Data.Skip((int)guids[0] - 1).Take(16).ToArray()));
        w.NewChunk = data =>
        {
            Guid g = am.AddChunk(data, null, null, tc.Bundles.ToArray());
            ChunkAssetEntry ce = am.GetChunkEntry(g);
            ce.AddedSuperBundles.AddRange(tc.SuperBundles);
            ce.H32 = tc.H32; ce.FirstMip = tc.FirstMip;
            return g;
        };
        return w;
    }
    // свободно песен по строкам Selection (≤ 255 на банк); у своего банка строки образца не считаются — их уберем перед записью
    static int Free(BankWork w) { return (255 - (w.IsNew ? 3 * w.Songs : w.Bank.SelRows())) / 3; }
    // влезет ли песня с этим номером: строки + номер в байт «значение − мин» (ev01 до 270, ev02 до 286, свой банк — от первого своего номера)
    static bool Fits(BankWork w, int num) { long mn = w.IsNew ? (w.First < 0 ? num : w.First) : w.Bank.MinTrack(); return Free(w) > 0 && num - mn <= 255; }

    static void SaveBank(BankWork w)
    {
        byte[] nb = w.Bank.Write();
        var check = SbleBank.Read(nb);
        if (!check.Write().SequenceEqual(nb)) throw new Exception("bank " + w.Name + " does not read back byte for byte");
        am.ModifyRes(w.Name, nb, w.Re.ResMeta);
        EbxAsset be = am.GetEbx(am.GetEbxEntry(w.Name));
        var chunks = (IList)be.RootObject.GetType().GetProperty("Chunks").GetValue(be.RootObject);
        Type ct = chunks[0].GetType();
        var gl = check.F(SbleBank.DS_CHK, 0xF4369173).Vals; var sl = check.F(SbleBank.DS_CHK, 0xDC19107B).Vals;
        for (int i = chunks.Count; i < check[SbleBank.DS_CHK].Count; i++)
        {
            object c = Activator.CreateInstance(ct);
            ct.GetProperty("ChunkId").SetValue(c, new Guid(check.Data.Skip((int)gl[i] - 1).Take(16).ToArray()));
            var psz = ct.GetProperty("ChunkSize"); psz.SetValue(c, Convert.ChangeType(sl[i], psz.PropertyType));
            chunks.Add(c);
        }
        am.ModifyEbx(w.Name, be);
        App.Log("bank " + w.Name + ": " + w.Orig.Length + " -> " + nb.Length + " B, Selection rows " + check.SelRows() + ", own songs " + w.Songs);
    }

    // ---------- свои банки (третий, четвертый…): копия ev02 без ее треков — разбор 11.11, findings «Сколько банков можно» ----------
    // Игра подмешивает к запросам в ev01 строки всех загруженных банков, у которых раздел Selection помечен ключом ev01 (так слышны ev02
    // и титры). Свой банк: новый ассет NewWaveAsset (EBX + RES) в бандлах ev02, свой ключ (djb2 от GUID корня — как у банков игры),
    // Selection — с ключом ev01; в граф музыки — рядом с ev01 / ev02 / титрами. Банк титров не трогаем (Ваня).
    const int MaxNewBanks = 3;
    static BankWork NewBank(int k)
    {
        var w = OpenBank(BANK);   // свежая модель ev02 — образец
        w.Name = "audio/music/event/unbound_soundtrack_0" + (3 + k); w.IsNew = true; w.Tpl = w.Bank.Tracks();
        App.Log("own bank " + w.Name + " (template " + BANK + ")");
        return w;
    }
    static void SaveNewBank(BankWork w)
    {
        w.Bank.DropTracks(w.Tpl);   // треки образца — вон, потом их варианты / сегменты / чанки
        w.Bank.Compact();
        EbxAssetEntry srcE = am.GetEbxEntry(BANK);
        ResAssetEntry srcR = am.GetResEntry(BANK);
        // true = свежая копия ОРИГИНАЛА. Без него Frosty отдает рабочий объект ev02, если ev02 уже изменен (SaveBank) — и свой банк
        // перекрашивал сам ev02 (GUID файла, корень, имя, чанки): в сборках 23:21 / 23:33 у ev02, 03 и 04 был один объект → молчали все трое (разбор 12.11).
        EbxAsset be = am.GetEbx(srcE, true);
        be.SetFileGuid(Guid.NewGuid());
        dynamic root = be.RootObject;
        root.Name = w.Name;
        Guid rg = Utils.GenerateDeterministicGuid(be.Objects, (Type)root.GetType(), be.FileGuid);
        root.SetInstanceGuid(new AssetClassGuid(rg, -1));
        w.Bank.SetKey(SbleBank.KeyOf(rg));
        byte[] nb = w.Bank.Write();
        var check = SbleBank.Read(nb);
        if (!check.Write().SequenceEqual(nb)) throw new Exception("bank " + w.Name + " does not read back byte for byte");
        // чанки ассета = чанки банка (GUID + размер)
        var chunks = (IList)((object)root).GetType().GetProperty("Chunks").GetValue(root);
        Type ct = chunks[0].GetType(); chunks.Clear();
        var gl = check.F(SbleBank.DS_CHK, 0xF4369173).Vals; var sl = check.F(SbleBank.DS_CHK, 0xDC19107B).Vals;
        for (int i = 0; i < check[SbleBank.DS_CHK].Count; i++)
        {
            object c = Activator.CreateInstance(ct);
            ct.GetProperty("ChunkId").SetValue(c, new Guid(check.Data.Skip((int)gl[i] - 1).Take(16).ToArray()));
            var psz = ct.GetProperty("ChunkSize"); psz.SetValue(c, Convert.ChangeType(sl[i], psz.PropertyType));
            chunks.Add(c);
        }
        EbxAssetEntry newE = am.AddEbx(w.Name, be, srcE.Bundles.ToArray());
        ResAssetEntry newR = am.AddRes(w.Name, (ResourceType)srcR.ResType, rg.ToByteArray(), nb, srcR.Bundles.ToArray());   // ResMeta = GUID корня (как у ev01 / ev02)
        newE.LinkAsset(newR);   // поля Resource у NewWaveAsset нет: RES банка связан по имени и ResMeta (GUID корня)
        foreach (var lst in new[] { newE.AddedBundles, newR.AddedBundles }) { var u = lst.Distinct().ToList(); lst.Clear(); lst.AddRange(u); }
        am.ModifyEbx(w.Name, be);
        w.FileGuid = be.FileGuid; w.Root = rg;
        App.Log("own bank " + w.Name + ": " + nb.Length + " B, key " + check.Key.ToString("X8") + ", rows " + check.SelRows() + ", songs " + w.Songs + ", numbers " + check.MinTrack() + "-" + check.MaxTrack() + ", file " + w.FileGuid);
    }
    // граф музыки: свои банки — в список узла WaveSwitcherNodeData рядом с ev01 / ev02 / титрами (узел держит банки загруженными)
    static void AddToGraph(List<BankWork> mine)
    {
        EbxAsset g = am.GetEbx(am.GetEbxEntry(CONTROL));
        object sw = g.Objects.FirstOrDefault(o => o.GetType().Name == "WaveSwitcherNodeData");
        if (sw == null) throw new UserError("gameChanged", "no WaveSwitcherNodeData");
        var waves = (IList)sw.GetType().GetProperty("Waves").GetValue(sw);
        // у корня графа — ReferencedData: запись {DataOwner = узел, SoundData = банк} на каждый звуковой ассет узлов (ev02 и титры — с владельцем этим узлом).
        // Разбор 12.11 (FrostyReader patchrefs, 123 графа игры): у WaveSwitcherNodeData все банки всегда есть в ReferencedData.
        // Без записи (сборки 23:21 / 23:33) свои банки молчали и ev02 тоже; без своих банков ev02 играет (тест 131).
        object root = g.RootObject;
        var rd = (IList)root.GetType().GetProperty("ReferencedData").GetValue(root);
        Type rt = rd[0].GetType();
        foreach (var w in mine)
        {
            var bank = new PointerRef(new EbxImportReference { FileGuid = w.FileGuid, ClassGuid = w.Root });
            waves.Add(bank); g.AddDependency(w.FileGuid);
            object e = Activator.CreateInstance(rt);
            rt.GetProperty("DataOwner").SetValue(e, new PointerRef(sw));
            rt.GetProperty("SoundData").SetValue(e, bank);
            rd.Add(e);
        }
        am.ModifyEbx(CONTROL, g);
        App.Log("graph " + CONTROL + ": banks in WaveSwitcherNodeData " + waves.Count + ", ReferencedData " + rd.Count);
    }

    // выключенные треки игры (обе галочки сняты), чьи строки можно отдать своим песням.
    // Отдаем только тот, кого игра не может вызвать ничем: во всех режимах Never (выключен) и в ОРИГИНАЛЕ IsDirectlySelectable = False
    // (прямой вызов запрещен самой игрой; у сюжетных он True). Разбор 10.11 (FrostyReader trackscan, 254 876 EBX): имена и хэши треков
    // вне pf_music_logic не встречаются; логику подключает только audio/gameplay/prefabs/pf_gameplay_audio_hooks.
    // Сюжетные и титры (TitleId не ID_MUSICTRACK_) не трогаем никогда. Логика читается до правок сборки — флаг оригинальный.
    static List<long> GiveAway(Dictionary<string, bool[]> choice)
    {
        var res = new List<long>();
        foreach (object t in TrackList(am.GetEbx(am.GetEbxEntry(LOGIC))))
        {
            string name = Str(t, "TrackEnumName");
            if (!Str(t, "TitleId").StartsWith("ID_MUSICTRACK_")) continue;
            if (Convert.ToBoolean(t.GetType().GetProperty("IsDirectlySelectable").GetValue(t))) continue;
            bool[] c; if (!choice.TryGetValue(name, out c) || c.Length < 2 || c[0] || c[1]) continue;
            res.Add(Convert.ToInt64(t.GetType().GetProperty("TrackEnumInt").GetValue(t)));
        }
        return res;
    }

    // giveAway — номера выключенных треков игры, чьи строки можно забрать (по порядку: кого отдавать первым)
    static void AddBanks(List<Song> songs, List<string> encDirs, List<long> giveAway)
    {
        Say("STEP bank 0/" + songs.Count);
        var banks = new List<BankWork> { OpenBank(BANK), OpenBank(BANK1) };
        if (job.ContainsKey("ev01first") && Convert.ToBoolean(job["ev01first"])) banks.Reverse();   // только для теста ev01 в игре (задание вручную)
        foreach (long t in giveAway)
        {
            if (banks.Sum(x => Free(x)) >= songs.Count) break;
            foreach (var w in banks)
            {
                if (!w.Bank.Tracks().Contains(t)) continue;
                int n = w.Bank.RemoveTrack(t);
                if (n > 0) { w.Changed = true; App.Log("bank " + w.Name + ": disabled game track " + t + " freed rows " + n); }
            }
        }
        int num = (int)Math.Max(140, banks.Max(w => w.Bank.MaxTrack()) + 1);   // номер растет на все банки — не повторяется
        // родные банки полны — свой банк (копия ev02 без ее треков), до MaxNewBanks штук. Флаг задания ev03first — только для теста своего банка в игре
        bool newFirst = job.ContainsKey("ev03first") && Convert.ToBoolean(job["ev03first"]);
        int made = 0;
        for (int i = 0; i < songs.Count; i++)
        {
            var s = songs[i];
            Tell("bank " + (i + 1) + "/" + songs.Count + " · " + Clean(s.title), i, songs.Count);   // самый долгий шаг (~100 с на 277 песен)
            var w = banks.FirstOrDefault(x => (!newFirst || x.IsNew) && Fits(x, num));
            if (w == null && made < MaxNewBanks) { w = NewBank(made++); banks.Add(w); }
            if (w == null) throw new UserError("tooManySongs", i.ToString());   // влезло i
            if (w.IsNew && w.First < 0) w.First = num;
            s.num = num++;
            s.trackName = TrackName(s);
            var stream = new MemoryStream(); var pieces = new List<SbleBank.Piece>();
            foreach (var k in new[] { Tuple.Create("B", 0, true), Tuple.Create("C", 1, false), Tuple.Create("A", 3, false) })
            {
                byte[] a = File.ReadAllBytes(Path.Combine(encDirs[i], k.Item1 + ".sps")), m = File.ReadAllBytes(Path.Combine(encDirs[i], k.Item1 + ".sek"));
                if (a[0] != 0x48) throw new Exception(k.Item1 + ".sps: not SPS");
                uint rate = BE32(a, 4) & 0x3FFFF, samples = BE32(a, 8) & 0x1FFFFFFF;
                if (rate != Audio.SR) throw new Exception("sample rate " + rate);
                while (stream.Length % 16 != 0) stream.WriteByte(0);
                long off = stream.Length; stream.Write(a, 0, a.Length);
                pieces.Add(new SbleBank.Piece { Value = k.Item2, Intro = k.Item3, Offset = off, Length = samples / (float)rate, MemGuid = w.NewChunk(m), MemSize = (uint)m.Length });
            }
            byte[] sd = stream.ToArray();
            w.Bank.AddSong(s.num, pieces, w.NewChunk(sd), (uint)sd.Length);
            w.Changed = true; w.Songs++;
            App.Log("song " + s.trackName + " number " + s.num + " -> " + w.Name + ", audio " + sd.Length + " B");
        }
        foreach (var w in banks) if (w.IsNew) SaveNewBank(w); else if (w.Changed) SaveBank(w);
        var mine = banks.Where(w => w.IsNew).ToList();
        if (mine.Count > 0) AddToGraph(mine);
    }

    static string TrackName(Song s)
    {
        var sb = new StringBuilder();
        bool up = true;
        foreach (char ch in (s.title ?? ""))
        {
            if ((ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z') || (ch >= '0' && ch <= '9')) { sb.Append(up ? char.ToUpperInvariant(ch) : ch); up = false; }
            else up = true;
            if (sb.Length >= 24) break;
        }
        return "US" + s.num + (sb.Length > 0 ? "_" + sb : "");
    }

    static uint BE32(byte[] b, int o) { return (uint)(b[o] << 24 | b[o + 1] << 16 | b[o + 2] << 8 | b[o + 3]); }

    static byte[] CoverPixels(Song s)
    {
        string p = Path.Combine(App.SongsDir, s.id, "cover.png");
        byte[] rgba;
        if (File.Exists(p)) using (var bmp = new Bitmap(p)) rgba = Img.ToRgba(bmp, 256, 256);
        else
        {
            rgba = new byte[256 * 256 * 4];
            for (int i = 0; i < rgba.Length; i += 4) { rgba[i] = 20; rgba[i + 1] = 21; rgba[i + 2] = 24; rgba[i + 3] = 255; }
        }
        return Bc7.Encode(rgba, 256, 256);
    }

    static byte[] ModIcon()
    {
        string p = Path.Combine(App.WebDir, "img", "modicon.png");
        if (File.Exists(p)) return File.ReadAllBytes(p);
        using (var bmp = new Bitmap(64, 64))
        using (var g = Graphics.FromImage(bmp))
        using (var ms = new MemoryStream())
        {
            g.Clear(Color.FromArgb(182, 255, 26));
            bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            return ms.ToArray();
        }
    }

    // ---------- pf_music_logic ----------
    static IList TrackList(EbxAsset a)
    {
        object ctl = a.Objects.First(o => o.GetType().GetProperty("MusicTrackInfoArray") != null);
        return (IList)ctl.GetType().GetProperty("MusicTrackInfoArray").GetValue(ctl);
    }

    static object AppendTrack(IList list, string template, string name, int num, string title, string artist)
    {
        object src = null;
        foreach (object t in list) if (Str(t, "TrackEnumName") == template) src = t;
        if (src == null) throw new UserError("gameChanged", "no track " + template);
        Type ty = src.GetType();
        object nt = Activator.CreateInstance(ty);
        foreach (var pi in ty.GetProperties()) if (pi.CanWrite && pi.GetIndexParameters().Length == 0) pi.SetValue(nt, pi.GetValue(src));
        SetStr(nt, "TrackEnumName", name); SetStr(nt, "TitleId", title); SetStr(nt, "ArtistId", artist);
        ty.GetProperty("TrackEnumInt").SetValue(nt, num);
        ty.GetProperty("IsDirectlySelectable").SetValue(nt, true);
        list.Add(nt);
        return nt;
    }

    static Dictionary<Guid, string> artByGuid;
    static string ArtName(object t)
    {
        try
        {
            object o = t.GetType().GetProperty("AlbumArt").GetValue(t);
            if (!(o is PointerRef)) return null;
            var p = (PointerRef)o;
            if (p.Type != PointerRefType.External) return null;
            if (artByGuid == null)
            {
                artByGuid = new Dictionary<Guid, string>();
                foreach (EbxAssetEntry ae in am.EnumerateEbx().Where(x => x.Name.ToLowerInvariant().StartsWith("ui/textures/albumart/")).ToList())
                    try { artByGuid[am.GetEbx(ae).FileGuid] = ae.Name; } catch { }
            }
            string n; return artByGuid.TryGetValue(p.External.FileGuid, out n) ? n : null;
        }
        catch (Exception ex) { App.Log("track cover: " + ex.Message); return null; }
    }

    static string Str(object o, string prop) { object v = o.GetType().GetProperty(prop).GetValue(o); return v == null ? "" : v.ToString(); }
    static void SetStr(object o, string prop, string val)
    {
        var pi = o.GetType().GetProperty(prop);
        pi.SetValue(o, pi.PropertyType == typeof(string) ? (object)val : Activator.CreateInstance(pi.PropertyType, new object[] { val }));
    }
    static int EnumVal(object o, string prop) { return Convert.ToInt32(o.GetType().GetProperty(prop).GetValue(o)); }
    static void SetEnumInt(object o, string prop, int v) { var pi = o.GetType().GetProperty(prop); pi.SetValue(o, Enum.ToObject(pi.PropertyType, v)); }

    // дубль ассета обложки (EBX + RES + чанк) со своими пикселями — проверено в игре (songlogic / addsong)
    static Tuple<Guid, Guid> Cover(string srcArt, string newArt, byte[] pix)
    {
        EbxAssetEntry srcE = am.GetEbxEntry(srcArt);
        if (srcE == null) throw new UserError("gameChanged", "no " + srcArt);
        EbxAsset tex = am.GetEbx(srcE);
        tex.SetFileGuid(Guid.NewGuid());
        dynamic troot = tex.RootObject;
        troot.Name = newArt;
        AssetClassGuid cg = new AssetClassGuid(Utils.GenerateDeterministicGuid(tex.Objects, (Type)troot.GetType(), tex.FileGuid), -1);
        troot.SetInstanceGuid(cg);
        ResAssetEntry srcR = am.GetResEntry((ulong)troot.Resource);
        Texture t = am.GetResAs<Texture>(srcR);
        ChunkAssetEntry srcC = am.GetChunkEntry(t.ChunkId);
        if (pix.Length != t.MipSizes[0]) throw new Exception("cover: " + pix.Length + " != " + t.MipSizes[0]);
        EbxAssetEntry newE = am.AddEbx(newArt, tex, srcE.Bundles.ToArray());
        t.AssetNameHash = (uint)Utils.HashString(newArt, true);
        Guid newChunk = am.AddChunk(pix, null, t, srcC.Bundles.ToArray());
        ChunkAssetEntry newC = am.GetChunkEntry(newChunk);
        newC.AddedSuperBundles.AddRange(srcC.SuperBundles);
        t.ChunkId = newChunk;
        ResAssetEntry newR = am.AddRes(newArt, ResourceType.Texture, srcR.ResMeta, t.SaveBytes(), srcR.Bundles.ToArray());
        troot.Resource = newR.ResRid;
        newR.LinkAsset(newC); newE.LinkAsset(newR);
        foreach (var lst in new[] { newE.AddedBundles, newR.AddedBundles, newC.AddedSuperBundles })
        { var u = lst.Distinct().ToList(); lst.Clear(); lst.AddRange(u); }
        am.ModifyEbx(newArt, tex);
        return Tuple.Create(tex.FileGuid, cg.ExportedGuid);
    }
}

class Lg : ILogger
{
    public void Log(string t, params object[] v) { }
    public void LogWarning(string t, params object[] v) { App.Log("frosty WARN " + t); }
    public void LogError(string t, params object[] v) { App.Log("frosty ERR " + t); }
}
