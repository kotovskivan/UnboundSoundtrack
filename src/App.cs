// Unbound Soundtrack — точка входа.
// Папка программы:  Unbound Soundtrack.exe, README.txt, app\ (web, frosty, encoder), data\ (настройки, песни, лог — создается сама).
// Режимы: без аргументов — окно (сервер 127.0.0.1 + Edge --app); "--worker <job.json>" — тяжелая работа с игрой через Frosty
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
    public const string Version = "1.0.0a";
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

    [MethodImpl(MethodImplOptions.NoInlining)] static int RunWorker(string job) { return Worker.Run(job); }
    [MethodImpl(MethodImplOptions.NoInlining)] static int RunUi(string[] args) { return Ui.Run(args); }

    // DLL Frosty лежат в app\frosty (и SDK игры — в app\frosty\Profiles), не рядом с exe
    static Assembly Resolve(object s, ResolveEventArgs ev)
    {
        string n = new AssemblyName(ev.Name).Name;
        string[] tries = n == "EbxClasses"
            ? new[] { Path.Combine(FrostyDir, "Profiles", "NFSUnboundSDK.dll") }
            : new[] { Path.Combine(FrostyDir, n + ".dll"), Path.Combine(FrostyDir, "ThirdParty", n + ".dll"), Path.Combine(FrostyDir, "Profiles", n + ".dll") };
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
    public string src, asig, isig;   // песня из папки Music: путь звука относительно Music, подписи звука и картинки (размер:время)
    public long added;
    public int num;              // номер трека в игре (ставит сборка: 140, 141…)
    public string trackName;     // внутреннее имя трека в игре (ставит сборка)
}

// профиль = набор: какие треки игры где играют + какие свои песни включены и где. Сами песни — общая библиотека (songs).
class Preset
{
    public string id, name;
    public Dictionary<string, bool[]> choice = new Dictionary<string, bool[]>();   // слот игры -> [гонки, город]
    public Dictionary<string, bool[]> songs = new Dictionary<string, bool[]>();    // id своей песни -> [гонки, город]; нет в списке = выключена
}

class Settings
{
    public bool agreed, pathsOk;   // pathsOk — человек подтвердил папки игры и Mod Manager
    public string lang;            // язык окна (uk/pl/en/de/pt); null — первый запуск, окно спросит
    public bool musicTold;         // окно «Пісні з папки» уже показали
    public List<string> skipped = new List<string>();   // песни из Music, удаленные в программе (папка осталась) — не подтягивать снова
    public string gamePath, fmmPath;
    // рабочее состояние = активный профиль (choice + race/city у песен); при каждом сохранении копируется в профиль
    public Dictionary<string, bool[]> choice = new Dictionary<string, bool[]>();
    public List<Song> songs = new List<Song>();
    public List<Preset> presets = new List<Preset>();
    public string activePreset;
    public string lastBuild;

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
        p.songs = songs.Where(s => !s.draft).ToDictionary(s => s.id, s => new[] { s.race, s.city });
    }
    // профиль -> рабочее состояние (песни, которых в профиле нет, — выключены, но остаются в библиотеке)
    public void Load(Preset p)
    {
        choice = p.choice.ToDictionary(k => k.Key, k => (bool[])k.Value.Clone());
        foreach (var s in songs) { bool[] f; if (p.songs.TryGetValue(s.id, out f) && f.Length >= 2) { s.race = f[0]; s.city = f[1]; } else { s.race = false; s.city = false; } }
        activePreset = p.id;
    }

    public static void Save() { lock (l) { var s = Get(); s.Snapshot(s.Active); App.WriteJson(P, s); } }
    public static void Edit(Action<Settings> f) { lock (l) { f(Get()); Save(); } }
}
