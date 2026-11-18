# Unbound Soundtrack

Your own songs in **Need for Speed Unbound** (PC) — as new tracks with your own covers.
Free fan-made tool by **Kitivski + Claude** (Claude — AI by Anthropic). Not affiliated with Electronic Arts or Criterion Games.

- Add your songs (mp3, flac, wav, ogg, m4a) as **new tracks** with your own title and cover — the game's tracks stay.
- Songs from **YouTube / YT Music**: a link to a song, a playlist or an album, many links at once or a .txt list (via yt-dlp).
- From **Spotify**: a link to a song, an album or a playlist (up to 100 songs), or a .csv from exportify.app — only the list comes from Spotify, the songs are found and downloaded from YT Music.
- For each song: the **race start** (17 s cutscene + 3.45 s countdown, the drop hits at GO) with a smooth fade-in, and the **finish** piece with your own fade-out. Zoom and 0.1 s precision.
- Turn tracks on/off separately for **races**, the **city** and the **garage**; profiles; HIGH / LOW; 30-second previews.
- Builds **one mod** for Frosty Mod Manager. Game files are not changed. Interface in **14 languages**.
- **Offline only** — mods are for single player.

**Download:** [**UnboundSoundtrack.zip** (latest)](https://github.com/kotovskivan/UnboundSoundtrack/releases/latest/download/UnboundSoundtrack.zip) · [website](https://kotovskivan.github.io/UnboundSoundtrack/) · [Nexus Mods](https://www.nexusmods.com/needforspeedunbound/mods/671) · [NFSMods](https://nfsmods.xyz/mod/8161) · [NFSAddons](https://www.nfsaddons.com/downloads/unbound/tools/11657/unbound-soundtrack-add-your-own-music.html) ·
**Guide with screenshots (UA/EN):** [Steam](https://steamcommunity.com/sharedfiles/filedetails/?id=3837419990)

## How it works

`Unbound Soundtrack.exe` is a small C# (.NET Framework 4.8) program: a local HTTP server on 127.0.0.1 plus a Microsoft Edge
`--app` window for the interface (`web/`). Heavy jobs run in the same exe as `--worker`: it reads the game with the
Frosty Toolsuite libraries, encodes your songs with the EA audio encoder and writes one `.fbmod` into Frosty Mod Manager's
`Mods\NeedForSpeedUnbound\` folder.

| Folder | What |
|---|---|
| `src/` | the program (C#) — server, game reading, audio cutting, BC7 cover encoder, sound bank writer, mod writer, YouTube / YT Music (`Yt.cs`) |
| `web/` | the window: HTML/JS/CSS, texts in 14 languages |
| `res/` | icon |
| `docs/` | README (14 languages) and THIRD-PARTY (shipped in the zip) |

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
- Not built by `build.cmd`: `app\yt-dlp.exe` ([yt-dlp releases](https://github.com/yt-dlp/yt-dlp/releases)) — copy it from the release zip if you need it.
- Since 1.0.4 the zip has no Visual C++ installer: if the runtime still fails to load, the program opens Microsoft's download
  ([vc_redist.x64.exe](https://aka.ms/vs/17/release/vc_redist.x64.exe)).

## Executables in the release zip

| File | Origin |
|---|---|
| `Unbound Soundtrack.exe` | this repository (`build.cmd`), not signed — no paid certificate |
| `app\encoder\Tool.exe` | EA audio encoder from Frosty Editor's Sound Editor plugin (HarGabt's fork), unmodified |
| `app\frosty\ThirdParty\fifaconfig.exe` | part of Frosty Mod Manager's `ThirdParty` folder, unmodified |
| `app\yt-dlp.exe` | [yt-dlp](https://github.com/yt-dlp/yt-dlp) (Unlicense), official release, unmodified |

## Changelog

- **1.0.5**
  - Fixed: a long `nfsunbound.key` (16 416 bytes = Key1 16 + Key2 16 + Key3 16384) went whole into AES → `CryptographicException` in `LoadInitfs`.
    Now the first 16 bytes are used, as Frosty does (`Worker.Load`); shorter than 16 bytes → the `noKey` message. Reported by MGS3PLAYER.
  - Song volume 0–300 % (`Song.vol` / `Song.lim`): gain before cutting, soft limiter at −1 dBFS with 10 ms look-ahead / 300 ms release (`src/Limiter.cs`),
    hard clip without protection. Auto = −8 LUFS (BS.1770 in the window), never below 100 %. Cache keys change only for songs with non-default values.
  - RACES / CITY / GARAGE sections in My songs and Game tracks (copies of the cards), + per section (`/api/song/flagsMany` — one request), fold state in `settings.json`.
  - Small tiles; Unbound-style glitch on windows, section switches and the song editor (`web/dialog.js`).
  - Fixed: Enable all / Disable all sent one request per song in parallel — on hundreds of songs the browser dropped requests (error windows, partial save).
  - Help text, folder picker status line, delete dialog text.
- **1.0.4**
  - New: **From Spotify** — a link to a song, an album or a playlist (`src/Spotify.cs`). The list is read from Spotify's public embed page
    (`open.spotify.com/embed/...`, no login, no API key; it gives up to 100 songs), bigger playlists — a .csv from exportify.app (parsed in the window).
    Each song is searched on YT Music (title + artist, length within 3 s); doubtful matches are shown in yellow. Audio is never taken from Spotify.
  - Song editor: zoom (wheel to the cursor, overview strip, ruler), GO and the finish to 0.1 s with the arrow keys (Shift — 0.01 s), typed time fields.
    Idea by MtheAX.
  - Race start fade-in (`Song.bFade` / `bLevel`, `Audio.FadeIn`) and finish fade-out / length (`Song.cFade` / `cLevel` / `cDur`, `Audio.FadeOut`);
    the window preview uses the same formula. Cache keys change only for songs with non-default values — the rest aren't re-encoded.
  - Fixed: with an early GO the race start preview could jump back to the beginning (stale delayed starts).
  - TLS 1.2 is switched on for Spotify requests (the exe has no app.config; Spotify drops old TLS).
- **1.0.3**
  - Fixed: with a lot of your songs (about 100 and more — then the mod adds its own sound banks) the police chase music went silent.
    When the mod changed the game's music graph, Frosty's writer lost the plugins of the music players, including the chase ones.
    Now the graph is the game's own bytes plus your sound banks (`src/Graph.cs`), and the build checks it
    (if the check fails, it falls back to the old way and writes a WARNING to the log).
  - Window: NFS Own Music tile (your own songs for the whole NFS series), NFSAddons in "Our pages", straight buttons in the top bar,
    the version next to MY SOUNDTRACK, LOG in line with the other links.

- **1.0.2**
  - New: **Garage** — a third switch next to Races and City: a song (or a game track) can play only in the garage
    (suggested by Low Rider). Songs and profiles from 1.0.1 keep playing in the garage as before.
  - Finnish — 14 languages.
  - New look: straight plates and white selection like the game menu, wide headings, a dark background (Kitivski's Lancer),
    "where it plays" as icons (flag / city / garage).
  - Credits: Low Rider — thanks for the feedback and suggestions.
  - Fixed: a game track with only "Garage" on could be given away to your songs as "off"; LOW was lost when a song was
    re-imported from the Music folder.

- **1.0.1** — big update.
  - New: songs from YouTube / YT Music — a song, playlist, album, many links or a .txt list; a music video link takes the same song
    from YT Music; an unavailable video is replaced by the same song from YT Music. 13 languages (translated with AI), README in each.
  - Hundreds of songs in one mod (own sound banks; tested with 277).
  - Race start: the drop hits at GO (17 s cutscene + 3.45 s countdown = 20.45 s); GO is found by the bass drop; songs with an early
    drop get silence before them.
  - HIGH / LOW for every track; sorting, filters, list view (remembered); drag & drop and your own song order.
  - ▶ for your own songs; covers: upload / generate / delete; "leave without saving?" in the song editor.
  - Build progress for every song, Stop button (closing the program stops the build, a stopped build never leaves a broken mod).
  - Browser choice; "How it works" help; links to the NFS Unbound Encryption Key and the website; Visual C++ runtime included; log in English.
  - Fixed: building in a folder with non-Latin letters (the encoder failed — also in 1.0.0a); "FrostyHash.dll" error without
    Visual C++; "Races only" songs played in the garage; non-Latin titles were empty on the in-game plate; a very long song name
    broke saving; building again after closing the window mid-build; folder picker (quoted / wrong paths, English system folders);
    "Open Mod Manager" silent on failure; language on the first screens; window bigger than small screens.

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
- [Low Rider](https://steamcommunity.com/profiles/76561198421221810) — feedback and suggestions about the game.
- [MtheAX](https://steamcommunity.com/profiles/76561199074624815) — the idea to zoom the waveform and set GO to 0.1 s.

Need for Speed and NFS Unbound are trademarks of Electronic Arts Inc.
