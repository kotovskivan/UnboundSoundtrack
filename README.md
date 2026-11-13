# Unbound Soundtrack

Your own songs in **Need for Speed Unbound** (PC) — as new tracks with your own covers.
Free fan-made tool by **Kitivski + Claude** (Claude — AI by Anthropic). Not affiliated with Electronic Arts or Criterion Games.

- Add your songs (mp3, flac, wav, ogg, m4a) as **new tracks** with your own title and cover — the game's tracks stay.
- Songs from **YouTube / YT Music**: a link to a song, a playlist or an album, many links at once or a .txt list (via yt-dlp).
- For each song: the **race start** (17 s cutscene + 3.45 s countdown, the drop hits at GO) and the **finish** piece.
- Turn game tracks on/off separately for **races** and the **city**; profiles; HIGH / LOW; 30-second previews.
- Builds **one mod** for Frosty Mod Manager. Game files are not changed. Interface in **13 languages**.
- **Offline only** — mods are for single player.

**Download:** [website](https://kotovskivan.github.io/UnboundSoundtrack/) · [Nexus Mods](https://www.nexusmods.com/needforspeedunbound/mods/671) ·
**Guide with screenshots (UA/EN):** [Steam](https://steamcommunity.com/sharedfiles/filedetails/?id=3837419990)

## How it works

`Unbound Soundtrack.exe` is a small C# (.NET Framework 4.8) program: a local HTTP server on 127.0.0.1 plus a Microsoft Edge
`--app` window for the interface (`web/`). Heavy jobs run in the same exe as `--worker`: it reads the game with the
Frosty Toolsuite libraries, encodes your songs with the EA audio encoder and writes one `.fbmod` into Frosty Mod Manager's
`Mods\NeedForSpeedUnbound\` folder.

| Folder | What |
|---|---|
| `src/` | the program (C#) — server, game reading, audio cutting, BC7 cover encoder, sound bank writer, mod writer, YouTube / YT Music (`Yt.cs`) |
| `web/` | the window: HTML/JS/CSS, texts in 13 languages |
| `res/` | icon |
| `docs/` | README (13 languages) and THIRD-PARTY (shipped in the zip) |

Internet use: the ▶ previews (`api.deezer.com`, the audio streams from Deezer / iTunes) and, only when you add songs by a link,
YouTube / YT Music (`music.youtube.com` to find the song, `yt-dlp` to download it). Nothing about the user is sent anywhere.

## Build

Needs only Windows 10/11 (the C# compiler of .NET Framework 4.8 is built in) and **HarGabt's Frosty Mod Manager**
([releases](https://github.com/HarGabt/FrostyToolsuite/releases), version 1.0.7 v26.10.12 was used).

```bat
build.cmd "C:\path\to\Frosty Mod Manager" "C:\path\to\encoder files"
```

Output: `out\Unbound Soundtrack\` — the same layout as the release zip.

- The first argument is the Frosty Mod Manager folder: `FrostySdk.dll`, `FrostyCore.dll` are compile references;
  `FrostyHash.dll`, `FrostyModSupport.dll`, `FrostyControls.dll`, `Profiles\NFSUnboundSDK.dll` and `ThirdParty\` are copied next to the exe.
- The second argument (optional) is a folder with the EA audio encoder: `Tool.exe`, `eaxmaenc.dll`, `libatrac9.dll`,
  `libmp3lame.dll`, `msvcr120.dll`. They come from the Sound Editor plugin of Frosty Editor (HarGabt's fork) and are shipped
  unmodified in the release zip (`app\encoder\`) — you can copy them from there.
- `vcruntime140.dll` and `vcruntime140_1.dll` (Microsoft Visual C++ runtime) are copied from `System32` into `app\frosty\`
  (and `app\encoder\`) — so the program works on a clean Windows without the Visual C++ Redistributable.
- Not built by `build.cmd`: `app\yt-dlp.exe` ([yt-dlp releases](https://github.com/yt-dlp/yt-dlp/releases)) and
  `app\vcredist\VC_redist.x64.exe` (Microsoft's installer) — copy them from the release zip if you need them.

## Executables in the release zip

| File | Origin |
|---|---|
| `Unbound Soundtrack.exe` | this repository (`build.cmd`), not signed — no paid certificate |
| `app\encoder\Tool.exe` | EA audio encoder from Frosty Editor's Sound Editor plugin (HarGabt's fork), unmodified |
| `app\frosty\ThirdParty\fifaconfig.exe` | part of Frosty Mod Manager's `ThirdParty` folder, unmodified |
| `app\yt-dlp.exe` | [yt-dlp](https://github.com/yt-dlp/yt-dlp) (Unlicense), official release, unmodified |
| `app\vcredist\VC_redist.x64.exe` | Microsoft Visual C++ Redistributable x64 installer, signed by Microsoft, unmodified |

## Changelog

- **1.0.1** — songs from YouTube / YT Music (link, playlist, album, many links, .txt list; a music video link takes the same song from YT Music).
  13 languages. Race start: the drop hits at GO (17 s cutscene + 3.45 s countdown = 20.45 s). Hundreds of songs in one mod (tested with 277).
  ▶ for your own songs, covers (upload / generate / delete), HIGH / LOW, sorting / filters / list view, drag & drop, browser choice,
  Stop button for the build (closing the program stops it too). Visual C++ runtime included.
  Fixed: "Races only" songs also played in the garage; non-Latin titles were empty on the in-game track plate (now written in Latin letters).

- **1.0.0a** — fix: "Build mod" failed with `FileLoadException: Could not load file or assembly 'FrostySdk' ... (0x80131515)`
  when the zip was downloaded and unpacked with Windows Explorer (Windows marks every unpacked file as "from the internet",
  and .NET refused to load the Frosty libraries). Nothing else changed.
- **1.0** — first release.

## License

GNU GPL v3 (the program uses Frosty Toolsuite libraries, which are GPL v3). Use it, share it, change it, include it anywhere —
keep the credit "Kitivski + Claude", the GPL license and the source open.
Third-party parts keep their own licenses — see `docs/THIRD-PARTY.txt`.

## Credits

- [Frosty Toolsuite](https://github.com/CadeEvs/FrostyToolsuite) by CadeEvs and the
  [NFS Unbound fork by HarGabt](https://github.com/HarGabt/FrostyToolsuite) — libraries and the EA audio encoder.
- [Alternate Waves](https://www.nexusmods.com/needforspeedunbound/mods/340), [The Lakeshore Sound](https://nfsmods.xyz/mod/7400),
  [vgmstream](https://github.com/vgmstream/vgmstream) — we learned from them how the game's music works.

Need for Speed and NFS Unbound are trademarks of Electronic Arts Inc.
