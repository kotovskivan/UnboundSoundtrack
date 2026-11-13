UNBOUND SOUNDTRACK  v1.0.1
Your own songs in Need for Speed Unbound — as new tracks with your own covers.
Free fan-made tool by Kitivski + Claude. NOT affiliated with Electronic Arts or Criterion Games.
Website: https://kotovskivan.github.io/UnboundSoundtrack/

Other languages — folder "README":
  README-UA.txt Українська · README-RU.txt Русский · README-DE.txt Deutsch · README-ES.txt Español
  README-FR.txt Français · README-IT.txt Italiano · README-PL.txt Polski · README-PT.txt Português (Brasil)
  README-TR.txt Türkçe · README-ZH.txt 简体中文 · README-JA.txt 日本語 · README-KO.txt 한국어
Translations made with AI (Claude).

=====================================================================
WHAT IT DOES
  - Turn game tracks on/off for races and for the city.
  - Add your own songs (mp3, flac, wav, ogg, m4a) as NEW tracks with your own cover.
    The game's tracks stay as they are.
  - Songs from YouTube / YT Music: a link to a song, a playlist or an album, many links at once or a .txt list.
    If the link is a music video, the program takes the same song from YT Music — without the video's intros and skits.
  - For each song set the race start (17 s cutscene + 3.45 s countdown, the drop hits at GO) and the finish piece.
  - HIGH / LOW on a card — how often a track plays (the game picks HIGH first).
  - Profiles: different sets of tracks, switch in one click.
  - Builds ONE mod for Frosty Mod Manager. Game files are not changed.
  - ▶ on a game track — 30-second preview (Deezer, needs internet); YouTube ↗ — the full song.
  - The window speaks 13 languages.

YOU NEED
  - Windows 10 or 11 (Microsoft Edge is built in — the window uses it).
  - Need for Speed Unbound (Steam or EA app).
  - Frosty Mod Manager, HarGabt's version for NFS Unbound:
    https://github.com/HarGabt/FrostyToolsuite/releases
    Start it once and pick the game before using this tool.
    On first start it asks for the game's encryption key — NFS Unbound Encryption Key:
    https://www.nexusmods.com/needforspeedunbound/mods/97
    On its first Launch (and after the ModData folder is deleted) Frosty asks for administrator rights
    to create links to the game data ("New installation detected") — that's normal, accept it.

HOW TO USE
  1. Unpack the WHOLE zip to a normal folder (Desktop, Documents, D:\Mods…),
     not into Program Files.
  2. Start "Unbound Soundtrack.exe".
  3. Choose the language, accept the terms. The first start reads the game — a few minutes, only once.
  4. + ADD SONG → choose a file → set GO and the finish → Save.
     Or ▶ FROM YT MUSIC → paste a link (song, playlist, album) → Download.
     All songs live in the "Music" folder next to the program, one numbered folder per song:
     Music\1\, Music\2\… — the order in the window. + ADD SONG creates the next folder itself.
     By hand: a new numbered folder with the song file + a picture (jpg, png) for the cover;
     GO and the finish are set automatically. Delete a folder — the song disappears from the program.
     Picture: any name if it's the only one in the folder; several — cover.jpg wins. Any size (the game uses 256x256).
  5. BUILD MOD. The mod appears in Frosty Mod Manager's list.
  6. In Frosty Mod Manager: tick "Unbound Soundtrack", turn off other music mods, press Launch.

OFFLINE ONLY
  Mods are for the single-player game. Before you go Online, start the game without mods.
  Playing Online with mods can get your EA account banned.

"WINDOWS PROTECTED YOUR PC" / "UNKNOWN PUBLISHER"
  The program is not signed with a paid certificate, so Windows SmartScreen warns about it.
  Click "More info" → "Run anyway". The source code is in the "source" folder.

WHAT IS IN THE FOLDER
  Unbound Soundtrack.exe   the program
  app\web                  the window (pages, texts in 13 languages)
  app\frosty               Frosty Toolsuite libraries — read the game, write the mod (GPL v3)
  app\encoder              EA audio encoder from Frosty Editor's Sound Editor plugin
  app\yt-dlp.exe           downloads songs from YouTube / YT Music (Unlicense)
  source\                  source code of this program (GPL v3)
  README\                  this text in other languages
  data\                    created on first start: your settings, songs, pictures read from YOUR game, log
  Music\                   your songs, one numbered folder each: song file, cover, song.json (name, GO, finish)
  THIRD-PARTY.txt          what we use, authors, licenses, links
  No files of the game or of EA are shipped: covers and backgrounds are read from your own game.

IF SOMETHING DOESN'T WORK
  - The log is data\log.txt (button "Log" in the window).
  - After a game update: the tool re-reads the game by itself. If building fails with
    "the game has changed" — look for a newer version on the website.
  - To start over: close the program and delete the "data" folder.
  - Report a bug: https://forms.gle/StNa4GFZRHckFVpi8  (describe what you did, paste the end of data\log.txt).

REMOVE
  Turn the mod off in Frosty Mod Manager and delete this folder.

---------------------------------------------------------------------
Need for Speed and NFS Unbound are trademarks of Electronic Arts Inc.
This tool is not made, approved or supported by Electronic Arts or Criterion Games.
Credits and licenses: THIRD-PARTY.txt and the "Credits" button in the program.
