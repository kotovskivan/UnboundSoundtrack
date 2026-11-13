// Unbound Soundtrack — точка входа.
// Папка программы:  Unbound Soundtrack.exe, README.txt, app\ (web, frosty, encoder), data\ (настройки, песни, лог — создается сама).
// Режимы: без аргументов — окно (сервер 127.0.0.1 + окно браузера, Ui.OpenWindow); "--worker <job.json>" — тяжелая работа с игрой через Frosty
// (отдельный процесс: чистая память на каждую сборку, падение Frosty не роняет окно).
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Web.Script.Serialization;

static class App
{
    public const string Version = "1.0.2";
    public const string Profile = "NeedForSpeedUnbound";
    public static string Root, AppDir, WebDir, FrostyDir, EncDir, DataDir, SongsDir, UiDir, LogPath, MusicDir;
    public static readonly JavaScriptSerializer Json = new JavaScriptSerializer { MaxJsonLength = int.MaxValue, RecursionLimit = 64 };

    [STAThread]
    static int Main(string[] args)
    {
        Root = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        AppDir = Path.Combine(Root, "app");
        WebDir = Path.Combine(AppDir, "web");
        FrostyDir = Path.Combine(AppDir, "frosty");
        EncDir = Path.Combine(AppDir, "encoder");
        DataDir = Path.Combine(Root, "data");
        SongsDir = Path.Combine(DataDir, "songs");
        UiDir = Path.Combine(DataDir, "ui");
        LogPath = Path.Combine(DataDir, "log.txt");
        MusicDir = Path.Combine(Root, "Music");            // сюда человек кладет свои песни (Music.cs)
        // VCRUNTIME140.dll нужна FrostyHash / libzstd / zlibwapi; на чистой Windows ее нет (P2-01) — своя копия в app\frosty, ищется после папки exe
        try { SetDllDirectory(FrostyDir); } catch { }
        AppDomain.CurrentDomain.AssemblyResolve += Resolve;
        try
        {
            if (args.Length >= 2 && args[0] == "--worker") return RunWorker(args[1]);
            return RunUi(args);
        }
        catch (Exception e)
        {
            Log("FATAL " + e);
            if (args.Length == 0 || args[0] != "--worker")
                System.Windows.Forms.MessageBox.Show("Unbound Soundtrack could not start:\n\n" + e.Message + "\n\nDetails: data\\log.txt", "Unbound Soundtrack");
            return 1;
        }
    }

    [System.Runtime.InteropServices.DllImport("kernel32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode, SetLastError = true)]
    static extern bool SetDllDirectory(string path);

    // настоящая версия Windows для лога: Environment.OSVersion без манифеста говорит 6.2.9200 (R1-13)
    public static string WinVersion()
    {
        try
        {
            using (var k = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion"))
            {
                object ma = k.GetValue("CurrentMajorVersionNumber"), mi = k.GetValue("CurrentMinorVersionNumber"), b = k.GetValue("CurrentBuild"), u = k.GetValue("UBR"), d = k.GetValue("DisplayVersion");
                return ma + "." + mi + "." + b + (u != null ? "." + u : "") + (d != null ? " (" + d + ")" : "");
            }
        }
        catch { return Environment.OSVersion.Version.ToString(); }
    }

    [MethodImpl(MethodImplOptions.NoInlining)] static int RunWorker(string job) { return Worker.Run(job); }
    [MethodImpl(MethodImplOptions.NoInlining)] static int RunUi(string[] args) { return Ui.Run(args); }

    // DLL Frosty лежат в app\frosty (и SDK игры — в app\frosty\Profiles), не рядом с exe
    static Assembly Resolve(object s, ResolveEventArgs ev)
    {
        string n = new AssemblyName(ev.Name).Name;
        string[] tries = n == "EbxClasses"
            ? new[] { Path.Combine(FrostyDir, "Profiles", "NFSUnboundSDK.dll") }
            : new[] { Path.Combine(FrostyDir, n + ".dll"), Path.Combine(FrostyDir, "ThirdParty", n + ".dll"), Path.Combine(FrostyDir, "Profiles", n + ".dll") };
        // UnsafeLoadFrom, не LoadFrom: zip из интернета → Проводник ставит метку на все DLL → LoadFrom падает 0x80131515 (баг 1.0, фикс 1.0.0a)
        foreach (string p in tries) if (File.Exists(p)) return Assembly.UnsafeLoadFrom(p);
        return null;
    }

    static readonly object logLock = new object();
    public static void Log(string s)
    {
        try
        {
            lock (logLock)
            {
                Directory.CreateDirectory(DataDir);
                var fi = new FileInfo(LogPath);
                if (fi.Exists && fi.Length > 2 * 1024 * 1024) { File.Copy(LogPath, LogPath + ".old", true); File.Delete(LogPath); }
                File.AppendAllText(LogPath, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " [" + System.Diagnostics.Process.GetCurrentProcess().Id + "] " + s + "\r\n", Encoding.UTF8);
            }
        }
        catch { }
    }

    public static void WriteJson(string path, object o)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        string tmp = path + ".tmp";
        File.WriteAllText(tmp, Json.Serialize(o), new UTF8Encoding(false));
        if (File.Exists(path)) File.Replace(tmp, path, null); else File.Move(tmp, path);
    }
    public static T ReadJson<T>(string path) where T : class
    {
        try { return File.Exists(path) ? Json.Deserialize<T>(File.ReadAllText(path, Encoding.UTF8)) : null; }
        catch (Exception e) { Log("json " + path + ": " + e.Message); return null; }
    }
}

// ---- настройки и песни (data\settings.json) ----
class Song
{
    public string id, title = "", artist = "", file = "";
    public double dur, drop, outro, cDur = 25;
    public bool race = true, city = true, draft = true, cover;
    public bool low;             // приоритет в игре: false — High (по умолчанию), true — Low (играет редко: движок сначала ищет High)
    public bool? garage;         // гараж (IsGarageTrack) — своя галочка с 1.0.2; null = песня сохранена до 1.0.2 → как city (тогда «Місто» = город + гараж)
    public bool Gar() { return garage ?? city; }   // метод, не свойство: JavaScriptSerializer не пишет его в settings.json
    public string src, asig, isig;   // песня из папки Music: путь звука относительно Music, подписи звука и картинки (размер:время)
    public long added;
    public int num;              // номер трека в игре (ставит сборка: 140, 141…)
    public string trackName;     // внутреннее имя трека в игре (ставит сборка)
}

// профиль = набор: какие треки игры где играют + какие свои песни включены и где. Сами песни — общая библиотека (songs).
class Preset
{
    public string id, name;
    // [гонки, город, low, гараж]: low нет = High; гаража нет (сохранено до 1.0.2) = как город
    public Dictionary<string, bool[]> choice = new Dictionary<string, bool[]>();   // слот игры -> [гонки, город, low, гараж]
    public Dictionary<string, bool[]> songs = new Dictionary<string, bool[]>();    // id своей песни -> [гонки, город, low, гараж]; нет в списке = выключена
}

class Settings
{
    public bool agreed, pathsOk;   // pathsOk — человек подтвердил папки игры и Mod Manager
    public string lang;            // язык окна (14 кодов — Ui.Langs); null — первый запуск, окно спросит
    public bool musicTold;         // окно «Пісні з папки» уже показали
    public List<string> skipped = new List<string>();   // песни из Music, удаленные в программе (папка осталась) — не подтягивать снова
    public string gamePath, fmmPath;
    // рабочее состояние = активный профиль (choice + race/city у песен); при каждом сохранении копируется в профиль
    public Dictionary<string, bool[]> choice = new Dictionary<string, bool[]>();
    public List<Song> songs = new List<Song>();
    public List<Preset> presets = new List<Preset>();
    public string activePreset;
    public string lastBuild;
    public string browser;                         // в чем открывать окно: путь exe браузера, "default" — браузер по умолчанию, null — Edge (Ui.OpenWindow)
    public Dictionary<string, string> view;        // вид главного окна: sort / art / where / view (P2-07: localStorage окна каждый запуск новый — порт другой)

    static readonly object l = new object();
    static Settings cur;
    static string P { get { return Path.Combine(App.DataDir, "settings.json"); } }
    public static Settings Get()
    {
        lock (l)
        {
            if (cur == null) cur = App.ReadJson<Settings>(P) ?? new Settings();
            if (cur.choice == null) cur.choice = new Dictionary<string, bool[]>();
            if (cur.songs == null) cur.songs = new List<Song>();
            if (cur.presets == null) cur.presets = new List<Preset>();
            if (cur.skipped == null) cur.skipped = new List<string>();
            if (cur.presets.Count == 0) { var p = new Preset { id = "p1", name = "Main" }; cur.presets.Add(p); cur.activePreset = p.id; cur.Snapshot(p); }
            if (cur.Active == null) cur.activePreset = cur.presets[0].id;
            return cur;
        }
    }
    public Preset Active { get { return presets.Find(p => p.id == activePreset); } }

    // рабочее состояние -> профиль
    public void Snapshot(Preset p)
    {
        p.choice = choice.ToDictionary(k => k.Key, k => (bool[])k.Value.Clone());
        p.songs = songs.Where(s => !s.draft).ToDictionary(s => s.id, s => new[] { s.race, s.city, s.low, s.Gar() });
    }
    // профиль -> рабочее состояние (песни, которых в профиле нет, — выключены, но остаются в библиотеке)
    public void Load(Preset p)
    {
        choice = p.choice.ToDictionary(k => k.Key, k => (bool[])k.Value.Clone());
        foreach (var s in songs) { bool[] f; if (p.songs.TryGetValue(s.id, out f) && f.Length >= 2) { s.race = f[0]; s.city = f[1]; s.low = f.Length > 2 && f[2]; s.garage = f.Length > 3 ? f[3] : f[1]; } else { s.race = false; s.city = false; s.low = false; s.garage = false; } }
        activePreset = p.id;
    }

    public static void Save() { lock (l) { var s = Get(); s.Snapshot(s.Active); App.WriteJson(P, s); } }
    public static void Edit(Action<Settings> f) { lock (l) { f(Get()); Save(); } }
}
