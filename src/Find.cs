// Где игра и где Frosty Mod Manager. Ищем сами; не нашли — человек один раз выбирает папку, запоминаем.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.Win32;

static class Find
{
    public const string GameExe = "NeedForSpeedUnbound.exe";
    public const string FmmExe = "FrostyModManager.exe";

    public static bool IsGame(string d) { return !string.IsNullOrEmpty(d) && File.Exists(Path.Combine(d, GameExe)) && Directory.Exists(Path.Combine(d, "Data")); }
    public static bool IsFmm(string d) { return !string.IsNullOrEmpty(d) && File.Exists(Path.Combine(d, FmmExe)); }
    public static string KeyPath(string fmm) { return Path.Combine(fmm, "nfsunbound.key"); }
    public static string ModsDir(string fmm) { return Path.Combine(fmm, "Mods", App.Profile); }

    public static string Game()
    {
        var st = Settings.Get();
        if (IsGame(st.gamePath)) return st.gamePath;
        foreach (string c in GameCandidates())
            if (IsGame(c)) { string p = Path.GetFullPath(c); Settings.Edit(s => s.gamePath = p); App.Log("игра найдена: " + p); return p; }
        return null;
    }

    static IEnumerable<string> GameCandidates()
    {
        // 1) конфиг Frosty Mod Manager (там путь, который человек уже выбрал)
        string cfg = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Frosty", "manager_config.json");
        string fromCfg = null;
        try
        {
            if (File.Exists(cfg))
            {
                var j = App.Json.Deserialize<Dictionary<string, object>>(File.ReadAllText(cfg));
                var games = j["Games"] as Dictionary<string, object>;
                var g = games != null && games.ContainsKey(App.Profile) ? games[App.Profile] as Dictionary<string, object> : null;
                if (g != null && g.ContainsKey("GamePath")) fromCfg = g["GamePath"] as string;
            }
        }
        catch (Exception e) { App.Log("manager_config: " + e.Message); }
        if (fromCfg != null) yield return fromCfg;
        // 2) Steam: все библиотеки
        foreach (string lib in SteamLibraries()) yield return Path.Combine(lib, "steamapps", "common", "Excalibur");
        // 3) EA app / Origin: ключи реестра с папкой установки
        foreach (string p in EaInstallDirs()) yield return p;
        // 4) обычные места
        foreach (var dr in DriveInfo.GetDrives().Where(d => d.DriveType == DriveType.Fixed && d.IsReady))
        {
            yield return Path.Combine(dr.Name, "Program Files", "EA Games", "Need for Speed Unbound");
            yield return Path.Combine(dr.Name, "Program Files (x86)", "Steam", "steamapps", "common", "Excalibur");
            yield return Path.Combine(dr.Name, "SteamLibrary", "steamapps", "common", "Excalibur");
            yield return Path.Combine(dr.Name, "EA Games", "Need for Speed Unbound");
            yield return Path.Combine(dr.Name, "Games", "Need for Speed Unbound");
        }
    }

    static IEnumerable<string> SteamLibraries()
    {
        var roots = new List<string>();
        foreach (var k in new[] { Tuple.Create(Registry.CurrentUser, @"Software\Valve\Steam", "SteamPath"),
                                  Tuple.Create(Registry.LocalMachine, @"SOFTWARE\WOW6432Node\Valve\Steam", "InstallPath"),
                                  Tuple.Create(Registry.LocalMachine, @"SOFTWARE\Valve\Steam", "InstallPath") })
        {
            try { using (var rk = k.Item1.OpenSubKey(k.Item2)) { var v = rk == null ? null : rk.GetValue(k.Item3) as string; if (v != null) roots.Add(v.Replace('/', '\\')); } }
            catch { }
        }
        var libs = new List<string>();
        foreach (string r in roots.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            libs.Add(r);
            string vdf = Path.Combine(r, "steamapps", "libraryfolders.vdf");
            try
            {
                if (File.Exists(vdf))
                    foreach (Match m in Regex.Matches(File.ReadAllText(vdf), "\"path\"\\s*\"([^\"]+)\""))
                        libs.Add(m.Groups[1].Value.Replace("\\\\", "\\"));
            }
            catch { }
        }
        return libs.Distinct(StringComparer.OrdinalIgnoreCase);
    }

    static IEnumerable<string> EaInstallDirs()
    {
        var res = new List<string>();
        foreach (string root in new[] { @"SOFTWARE\WOW6432Node\EA Games", @"SOFTWARE\EA Games", @"SOFTWARE\WOW6432Node\Electronic Arts", @"SOFTWARE\Electronic Arts" })
        {
            try
            {
                using (var rk = Registry.LocalMachine.OpenSubKey(root))
                {
                    if (rk == null) continue;
                    foreach (string sub in rk.GetSubKeyNames())
                        using (var sk = rk.OpenSubKey(sub))
                        {
                            var v = sk == null ? null : (sk.GetValue("Install Dir") ?? sk.GetValue("InstallDir")) as string;
                            if (v != null) res.Add(v);
                        }
                }
            }
            catch { }
        }
        return res;
    }

    // ---- Frosty Mod Manager ----
    static string fmmFound; static bool fmmSearched; static readonly object fl = new object();

    public static string Fmm()
    {
        var st = Settings.Get();
        if (IsFmm(st.fmmPath)) return st.fmmPath;
        lock (fl)
        {
            if (!fmmSearched) { fmmFound = SearchFmm(); fmmSearched = true; }
            if (fmmFound != null) { string p = fmmFound; Settings.Edit(s => s.fmmPath = p); App.Log("Mod Manager найден: " + p); }
            return fmmFound;
        }
    }
    public static void ResetSearch() { lock (fl) fmmSearched = false; }

    static string SearchFmm()
    {
        // запущен прямо сейчас?
        try
        {
            foreach (var p in Process.GetProcessesByName("FrostyModManager"))
                try { string d = Path.GetDirectoryName(p.MainModule.FileName); if (IsFmm(d)) return d; } catch { }
        }
        catch { }
        var found = new List<string>();
        var deadline = DateTime.Now.AddSeconds(8);
        var starts = new List<string>();
        string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        foreach (string s in new[] { Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), Path.Combine(home, "Downloads"),
                                     Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), home })
            if (Directory.Exists(s)) starts.Add(s);
        foreach (var dr in DriveInfo.GetDrives().Where(d => d.DriveType == DriveType.Fixed && d.IsReady)) starts.Add(dr.Name);
        // рядом с самой программой (часто кладут в одну папку модов)
        starts.Insert(0, Path.GetDirectoryName(App.Root) ?? App.Root);
        foreach (string s in starts) { Walk(s, 0, 4, found, deadline); if (found.Count > 0 && found.Any(f => File.Exists(KeyPath(f)))) break; }
        // лучше тот, где уже есть ключ и папка модов Unbound
        return found.OrderByDescending(f => (File.Exists(KeyPath(f)) ? 2 : 0) + (Directory.Exists(ModsDir(f)) ? 1 : 0)).FirstOrDefault();
    }

    static readonly HashSet<string> skip = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    { "Windows", "$Recycle.Bin", "ProgramData", "System Volume Information", "AppData", "node_modules", ".git", "steamapps", "Recovery", "PerfLogs", "WindowsApps" };

    static void Walk(string d, int depth, int max, List<string> found, DateTime deadline)
    {
        if (DateTime.Now > deadline) return;
        try
        {
            if (File.Exists(Path.Combine(d, FmmExe))) { found.Add(d); return; }
            if (depth >= max) return;
            foreach (string sub in Directory.GetDirectories(d))
            {
                string n = Path.GetFileName(sub);
                if (skip.Contains(n) || n.StartsWith(".")) continue;
                if ((File.GetAttributes(sub) & FileAttributes.ReparsePoint) != 0) continue;
                Walk(sub, depth + 1, max, found, deadline);
            }
        }
        catch { }
    }

    // окно выбора папки (WinForms, отдельный STA-поток, поверх всех окон)
    public static string PickFolder(string description)
    {
        string res = null;
        var t = new Thread(() =>
        {
            using (var owner = new System.Windows.Forms.Form { TopMost = true, ShowInTaskbar = false, Width = 0, Height = 0, StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen })
            using (var dlg = new System.Windows.Forms.FolderBrowserDialog { Description = description, ShowNewFolderButton = false })
            {
                owner.Show(); owner.Activate();
                if (dlg.ShowDialog(owner) == System.Windows.Forms.DialogResult.OK) res = dlg.SelectedPath;
            }
        });
        t.SetApartmentState(ApartmentState.STA); t.Start(); t.Join();
        return res;
    }

    // отпечаток версии игры: если меняется — заново достаем список треков и картинки
    public static string Fingerprint(string game)
    {
        try
        {
            var a = new FileInfo(Path.Combine(game, GameExe));
            string toc = Path.Combine(game, "Data", "layout.toc");
            var b = File.Exists(toc) ? new FileInfo(toc) : a;
            return a.Length + "_" + a.LastWriteTimeUtc.Ticks + "_" + b.Length + "_" + b.LastWriteTimeUtc.Ticks;
        }
        catch { return "?"; }
    }
}
