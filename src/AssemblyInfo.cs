using System.Reflection;
using System.Runtime.InteropServices;

// Свойства exe (Проводник -> Свойства -> Подробно): название, версия, автор, лицензия.
// 16.11.2026: без них антивирусы на VirusTotal давали 13/71 (Microsoft Wacatac!ml, BitDefender MSILHeracles + 7 на его движке),
// с ними — 3/70; код при этом не менялся. Версия — та же, что App.Version (при выпуске поднять обе).
[assembly: AssemblyTitle("Unbound Soundtrack")]
[assembly: AssemblyDescription("Your own songs in Need for Speed Unbound: builds one music mod for Frosty Mod Manager. Free, open source (GPL v3).")]
[assembly: AssemblyCompany("Kitivski + Claude")]
[assembly: AssemblyProduct("Unbound Soundtrack")]
[assembly: AssemblyCopyright("GPL v3 - Kitivski + Claude - github.com/kotovskivan/UnboundSoundtrack")]
[assembly: AssemblyVersion("1.0.4.0")]
[assembly: AssemblyFileVersion("1.0.4.0")]
[assembly: AssemblyInformationalVersion("1.0.4")]
[assembly: ComVisible(false)]
