// Языки: украинский, польский, английский, немецкий, португальский (Бразилия). По умолчанию — язык системы, иначе английский.
// Ключи step_* — шаги сборки (приходят от программы), err_* — ошибки простыми словами.
const I18N = {
  en: {
    bugReport: "Report a bug", bugHint: "Didn't help? <a href=\"{url}\">Report a bug ↗</a> — describe what you did and paste the end of data\\log.txt (button “Show log”).",
    profiles: "Profiles", newProfile: "+ New", newProfileT: "New profile", profileName: "Profile name. What is on right now is copied into it.", renameT: "Rename profile", delProfT: "Delete profile?", delProfHtml: "<p>Profile “{n}” will be deleted. Your songs stay.</p>", secMine: "My songs", secGame: "Game tracks", profSwitched: "Profile “{n}”. Press <b>Build mod</b> to put it into the game.", profCreated: "Profile “{n}” created — a copy of the current set.",
    pvTip: "30-second preview (Deezer)", legendPv: "<b>▶</b> — 30-second preview right here, <b>YouTube ↗</b> — the full song.", err_noPreview: "No preview — check the internet connection.", cPrev: "Previews", cPrevTxt: "30-second previews of the game tracks, played from their servers (internet needed); nothing is stored",
    pickThis: "Use this folder", cancel: "Cancel", pickGameHint: "Open the folder with NeedForSpeedUnbound.exe and press “Use this folder”. ✓ green — the right one.", pickFmmHint: "Open the folder with FrostyModManager.exe and press “Use this folder”. ✓ green — the right one.", fpOkGame: "✓ The game is here", fpOkFmm: "✓ Frosty Mod Manager is here", fpNo: "Not here — open the right folder", fpDenied: "No access to this folder", fpPlaces: "Drives and quick places",
    folders: "Folders", whereT: "Game and Mod Manager", whereHtml: "<p>Game: <code>{g}</code></p><p>Frosty Mod Manager: <code>{f}</code></p><p>Is this right?</p>", whereYes: "Correct", whereNo: "Change", changeT: "What to change?", changeHtml: "<p>Choose which folder to show again.</p>", changeGame: "Game folder", changeFmm: "Mod Manager folder",
    docTitle: "Unbound Soundtrack", langT: "Language", titleA: "My", titleB: "soundtrack",
    musicBtn: "Music folder ↗", musicT: "Songs from a folder", musicOk: "Got it", musicOpen: "Open folder", musicHtml: "<p>Your songs live in the <b>Music</b> folder next to the program — each in its own numbered folder: <code>Music\\1\\</code>, <code>Music\\2\\</code>… They show up in “{mine}” in that order.</p><ul><li><b>Easiest — the “+ {add}” button:</b> pick a file, type the artist and title — the program creates the next folder by itself.</li><li><b>Or by hand:</b> make a folder with the next number and put the song file (mp3, flac, wav, ogg, m4a) and a picture for the cover (jpg, png) into it. No picture — we draw a cover.</li><li><b>Picture:</b> any name if it's the only one in the folder; if there are several — <code>cover.jpg</code> is used (or the one named like the mp3). Any size: in the game the cover is 256×256, a non-square one is cropped to the center.</li><li>Artist and title come from the mp3 tags or the file name; to fix them — click the song.</li><li>The race start (GO) and the finish are set automatically. To check — click the song.</li><li>Delete a folder from Music — the song disappears from the program too. To change the order — rename the folders.</li></ul>", musicImpT: "Songs from Music", musicStep: "{n} of {m}: {a}", musicAdded: "Added from Music: {n}. GO and finish were set automatically — click a song to check. Then <b>{build}</b>.", musicGone: "Removed: {n} — their files are no longer in Music. <b>{build}</b> to remove them from the game too.", musicFail: "Couldn't read: {a}", delMusic: "<p>The files in the Music folder stay — the program won't pick this song up again.</p>",
    all: "All tracks", allSub: "game soundtrack + your songs", on: "Playing", onSub: "enabled somewhere", off: "Disabled", offSub: "not playing anywhere",
    mine: "My songs", mineSub: "your own tracks", build: "Build mod", buildSub: ".fbmod for Frosty Mod Manager",
    inRaces: "in races", inCity: "in city · garage · menu", search: "SEARCH: TRACK OR ARTIST", enableAll: "Enable all", disableAll: "Disable all",
    legend: "Click a cover to toggle the whole track. <b>RACES</b> — events, meet-ups, takeovers. <b>CITY</b> — free roam, garage, menu. <b>+ ADD SONG</b> — your own track with its own cover, next to the game's ones.",
    races: "RACES", city: "CITY", offStamp: "OFF", yours: "YOURS", editTip: "Edit: name, cover, GO and finish", ytTip: "Listen on YouTube",
    madeBy: "made by", claudeRole: "AI · code", devRole: "developer", poweredBy: "runs with", about: "Credits", log: "Log",
    addSong: "Add song", addSongSub: "mp3 · flac · wav · ogg · m4a",
    panelNew: "New song", panelEdit: "Your song", dropHere: "Drop an audio file here — mp3, flac, wav, ogg, m4a", pickFile: "Choose file",
    artistF: "Artist", titleF: "Title", coverF: "Cover", coverPick: "Choose image", coverAuto: "No image? A cover is drawn from the title.", coverDrop: "or drop a picture here",
    playsIn: "Plays in",
    partA: "Full", partAsub: "the whole song — city, garage, menu", partB: "Race start", partBsub: "17 s build-up under the countdown, the drop hits at GO. Drag the green GO marker onto the drop.",
    partC: "Finish", partCsub: "short piece after the finish line, fades out. Drag the yellow block.",
    save: "Save", del: "Delete", decoding: "Reading the song…", uploading: "Saving the song…", badFile: "Can't read this file",
    delT: "Delete song?", delHtml: "<p>“{t}” will be removed from your soundtrack. Your music file stays where it is.</p><p>Build the mod again to remove it from the game.</p>", yes: "Yes", no: "No",
    savedNote: "Saved. Press <b>Build mod</b> to put it into the game.",
    agreeT: "Before you start", agreeYes: "I agree", agreeNo: "Exit",
    agreeHtml: `<p><b>Unbound Soundtrack</b> is a free fan-made tool by <b>Kitivski + Claude</b>. It is <b>not made, approved or supported by Electronic Arts or Criterion Games</b>, and has nothing to do with them. Need for Speed and NFS Unbound are trademarks of Electronic Arts Inc.</p>
<ul><li><b>Use at your own risk.</b> The tool comes as is, without any warranty. We are not responsible for game problems, lost saves or accounts.</li>
<li><span class="warn"><b>Offline only.</b></span> Don't play Online with mods enabled — EA can ban your account.</li>
<li><b>Your music, your responsibility.</b> Add only songs you have the right to use. Nothing is uploaded anywhere: everything stays on your PC.</li>
<li><b>Game files stay untouched.</b> The tool builds a mod for Frosty Mod Manager. To go back — turn the mod off there.</li>
<li>Back up your saves before trying new mods.</li></ul><p>Do you agree to use the tool on these terms?</p>`,
    offT: "Stay offline", offYes: "Got it",
    offHtml: `<p><span class="warn"><b>Mods are for the single-player game only.</b></span></p><p>Before you go Online — launch the game <b>without mods</b> (turn the mod off in Frosty Mod Manager, or start the game from Steam / EA app). Playing Online with mods can get your EA account banned.</p>`,
    gameT: "Where is the game?", gameHtml: "<p>I couldn't find <b>Need for Speed Unbound</b> on this PC.</p><p>Show me the game folder — the one with <code>NeedForSpeedUnbound.exe</code> inside.</p>",
    pickFolder: "Choose folder", exit: "Exit", notGame: "There is no NeedForSpeedUnbound.exe in this folder:", notFmm: "There is no FrostyModManager.exe in this folder:",
    fmmT: "Frosty Mod Manager", fmmHtml: "<p>The mod is turned on through <b>Frosty Mod Manager</b> (HarGabt's version for NFS Unbound) — I couldn't find it.</p><p>Download it, unpack it, start it once and pick the game. Then show me its folder — the one with <code>FrostyModManager.exe</code>.</p>",
    download: "Download page",
    keyT: "Start Mod Manager once", keyHtml: "<p>Frosty Mod Manager hasn't been set up for NFS Unbound yet.</p><p>Start it, pick the game, wait until it finishes loading, close it — then press <b>Retry</b>.</p>",
    retry: "Retry", openFmm: "Open Mod Manager",
    prepT: "Getting ready", prepFirst: "First start: reading the game. This can take a few minutes — only once.", buildT: "Building the mod",
    builtT: "Mod ready", builtHtml: "<p><b>UnboundSoundtrack.fbmod</b> is in Frosty Mod Manager's list.</p><ul><li>In Mod Manager: tick <b>Unbound Soundtrack</b> and press <b>Launch</b>. If it was open — restart it.</li><li>Turn off other music mods: they change the same files.</li></ul><p><small>{path}</small></p>",
    close: "Close", failT: "Something went wrong", openLog: "Show log",
    emptyT: "Silence?", emptyHtml: "<p>No tracks are enabled in races or in the city — it will be quiet there.</p><p>Build anyway?</p>",
    thanksT: "Thank you", cMade: "Made by", cInside: "Inside this tool", cNeed: "You will need", cLearned: "We learned from", cUsed: "Used while building", cSpecial: "Special thanks",
    cFrosty: "libraries to read the game and write mods (GPL v3)", cFork: "NFS Unbound fork by HarGabt", cEnc: "EA audio encoder — from the Sound Editor plugin of Frosty Editor (HarGabt fork), with its libraries", cResearch: "research scripts",
    cNotAff: "Not affiliated with Electronic Arts or Criterion Games. Need for Speed is a trademark of Electronic Arts Inc.", releases: "releases",
    step_copyCache: "Copying Frosty's game index…", step_firstLoad: "Reading the game for the first time — a few minutes…", step_loadGame: "Reading the game…",
    step_readTracks: "Reading the track list…", step_covers: "Taking covers from the game…", step_backgrounds: "Taking backgrounds from the game…",
    step_encode: "Encoding: {a}", step_bank: "Adding songs to the music bank…", step_logic: "Writing the track list…", step_cover: "Cover: {a}", step_writeMod: "Writing the mod…",
    err_noGame: "The game folder isn't set.", err_noFmm: "Frosty Mod Manager isn't found.", err_noKey: "Frosty Mod Manager isn't set up for NFS Unbound yet: start it once and pick the game.",
    err_gameChanged: "The game has changed (an update?) — this version of the tool may not fit it. Check for a newer Unbound Soundtrack.",
    err_tooManySongs: "Too many enabled songs: room for {a} right now. Turn some songs off — or turn off more game tracks: the program can give some of their places to your songs.", err_modLocked: "Can't write the mod file — close Frosty Mod Manager and try again.",
    err_encodeFailed: "The audio encoder failed on {a}.", err_encoderMissing: "The audio encoder is missing — unpack the whole zip again.", err_songMissing: "The song file is missing — add the song again.",
    err_busy: "Wait — the previous job isn't finished yet.", err_tooLong: "The song is too long (max 20 minutes).", err_badImage: "Can't use this picture.",
    err_crash: "Unexpected error. The log has the details.", err_net: "Lost connection to the program. Is it still running?"
  },
  uk: {
    bugReport: "Повідомити про баг", bugHint: "Не допомогло? <a href=\"{url}\">Повідомити про баг ↗</a> — опиши, що робив, і встав кінець data\\log.txt (кнопка «Показати лог»).",
    profiles: "Профілі", newProfile: "+ Новий", newProfileT: "Новий профіль", profileName: "Назва профілю. Те, що зараз увімкнено, скопіюється в нього.", renameT: "Перейменувати профіль", delProfT: "Видалити профіль?", delProfHtml: "<p>Профіль «{n}» буде видалено. Твої пісні залишаться.</p>", secMine: "Мої пісні", secGame: "Треки гри", profSwitched: "Профіль «{n}». Натисни <b>Зібрати мод</b>, щоб він потрапив у гру.", profCreated: "Профіль «{n}» створено — копія поточного набору.",
    pvTip: "30 секунд послухати (Deezer)", legendPv: "<b>▶</b> — 30 секунд прямо тут, <b>YouTube ↗</b> — пісня повністю.", err_noPreview: "Немає прев'ю — перевір інтернет.", cPrev: "Прев'ю", cPrevTxt: "30-секундні прев'ю треків гри, грають з їхніх серверів (потрібен інтернет); нічого не зберігається",
    pickThis: "Обрати цю папку", cancel: "Скасувати", pickGameHint: "Відкрий папку з NeedForSpeedUnbound.exe і натисни «Обрати цю папку». ✓ зелена — потрібна.", pickFmmHint: "Відкрий папку з FrostyModManager.exe і натисни «Обрати цю папку». ✓ зелена — потрібна.", fpOkGame: "✓ Гра тут", fpOkFmm: "✓ Frosty Mod Manager тут", fpNo: "Тут немає — відкрий потрібну папку", fpDenied: "Немає доступу до цієї папки", fpPlaces: "Диски й швидкі місця",
    folders: "Папки", whereT: "Гра і Mod Manager", whereHtml: "<p>Гра: <code>{g}</code></p><p>Frosty Mod Manager: <code>{f}</code></p><p>Все вірно?</p>", whereYes: "Вірно", whereNo: "Змінити", changeT: "Що змінити?", changeHtml: "<p>Обери, яку папку показати заново.</p>", changeGame: "Папку гри", changeFmm: "Папку Mod Manager",
    docTitle: "Unbound Soundtrack", langT: "Мова", titleA: "Мій", titleB: "саундтрек",
    musicBtn: "Папка Music ↗", musicT: "Пісні з папки", musicOk: "Зрозуміло", musicOpen: "Відкрити папку", musicHtml: "<p>Твої пісні лежать у папці <b>Music</b> поруч із програмою — кожна в своїй папці з номером: <code>Music\\1\\</code>, <code>Music\\2\\</code>… У такому порядку вони й показуються в «{mine}».</p><ul><li><b>Найпростіше — кнопка «+ {add}»:</b> обираєш файл, пишеш виконавця й назву — програма сама створить наступну папку.</li><li><b>Або вручну:</b> створи папку з наступним номером і поклади туди файл пісні (mp3, flac, wav, ogg, m4a) і картинку для обкладинки (jpg, png). Без картинки обкладинку намалюємо.</li><li><b>Картинка:</b> назва будь-яка, якщо вона в папці одна; якщо кілька — береться <code>cover.jpg</code> (або та, що зветься як mp3). Розмір будь-який: у грі обкладинка 256×256, не квадратну обріжемо по центру.</li><li>Виконавець і назва — з тегів mp3 або з імені файлу; виправити — клікни на пісню.</li><li>Старт гонки (GO) і фініш програма ставить сама. Перевірити — клікни на пісню.</li><li>Видалив папку з Music — пісня зникне і з програми. Змінити порядок — перейменуй папки.</li></ul>", musicImpT: "Пісні з Music", musicStep: "{n} з {m}: {a}", musicAdded: "З Music додано пісень: {n}. GO і фініш поставлені самі — перевір, клікнувши на пісню. Потім <b>{build}</b>.", musicGone: "Прибрано пісень: {n} — їхніх файлів більше немає в Music. <b>{build}</b>, щоб прибрати їх і з гри.", musicFail: "Не вдалося прочитати: {a}", delMusic: "<p>Файли в папці Music лишаються — програма цю пісню більше не підтягуватиме.</p>",
    all: "Усі треки", allSub: "саундтрек гри + твої пісні", on: "Грають", onSub: "увімкнені хоч десь", off: "Вимкнені", offSub: "не грають ніде",
    mine: "Мої пісні", mineSub: "твої власні треки", build: "Зібрати мод", buildSub: ".fbmod для Frosty Mod Manager",
    inRaces: "у гонках", inCity: "у місті · гаражі · меню", search: "ПОШУК: ТРЕК АБО ВИКОНАВЕЦЬ", enableAll: "Увімкнути всі", disableAll: "Вимкнути всі",
    legend: "Клік по обкладинці — трек увімк/вимк повністю. <b>ГОНКИ</b> — заїзди, мітапи, тейковери. <b>МІСТО</b> — вільна їзда, гараж, меню. <b>+ ДОДАТИ ПІСНЮ</b> — свій трек зі своєю обкладинкою, поруч із треками гри.",
    races: "ГОНКИ", city: "МІСТО", offStamp: "ВИМК", yours: "СВОЯ", editTip: "Редагувати: назва, обкладинка, GO і фініш", ytTip: "Слухати на YouTube",
    madeBy: "зробили", claudeRole: "ШІ · код", devRole: "розробник", poweredBy: "працює з", about: "Подяки", log: "Лог",
    addSong: "Додати пісню", addSongSub: "mp3 · flac · wav · ogg · m4a",
    panelNew: "Нова пісня", panelEdit: "Твоя пісня", dropHere: "Перетягни сюди аудіофайл — mp3, flac, wav, ogg, m4a", pickFile: "Обрати файл",
    artistF: "Виконавець", titleF: "Назва", coverF: "Обкладинка", coverPick: "Обрати картинку", coverAuto: "Немає картинки? Обкладинку намалюємо з назви.", coverDrop: "або перетягни картинку сюди",
    playsIn: "Де грає",
    partA: "Повна", partAsub: "вся пісня — місто, гараж, меню", partB: "Старт гонки", partBsub: "17 с нагнітання під відлік, на GO б'є дроп. Перетягни зелену мітку GO на дроп.",
    partC: "Фініш", partCsub: "короткий шматок після фінішу, затихає. Перетягни жовтий блок.",
    save: "Зберегти", del: "Видалити", decoding: "Читаю пісню…", uploading: "Зберігаю пісню…", badFile: "Не вдається прочитати файл",
    delT: "Видалити пісню?", delHtml: "<p>«{t}» зникне з твого саундтреку. Сам музичний файл залишиться на місці.</p><p>Щоб прибрати її з гри — збери мод ще раз.</p>", yes: "Так", no: "Ні",
    savedNote: "Збережено. Натисни <b>Зібрати мод</b>, щоб вона потрапила в гру.",
    agreeT: "Перш ніж почати", agreeYes: "Погоджуюсь", agreeNo: "Вийти",
    agreeHtml: `<p><b>Unbound Soundtrack</b> — безкоштовна фанатська програма від <b>Kitivski + Claude</b>. Її <b>не створювали, не схвалювали й не підтримують Electronic Arts чи Criterion Games</b>, і вона не має до них жодного стосунку. Need for Speed і NFS Unbound — торгові марки Electronic Arts Inc.</p>
<ul><li><b>На свій ризик.</b> Програма як є, без жодних гарантій. Ми не відповідаємо за проблеми з грою, втрачені збереження чи акаунти.</li>
<li><span class="warn"><b>Тільки офлайн.</b></span> Не заходь в онлайн з увімкненими модами — EA може забанити акаунт.</li>
<li><b>Твоя музика — твоя відповідальність.</b> Додавай лише пісні, які маєш право використовувати. Нічого нікуди не відправляється: усе лишається на твоєму ПК.</li>
<li><b>Файли гри не змінюються.</b> Програма збирає мод для Frosty Mod Manager. Щоб повернути все як було — вимкни мод там.</li>
<li>Перед новими модами зроби копію збережень.</li></ul><p>Погоджуєшся користуватися програмою на цих умовах?</p>`,
    offT: "Тільки офлайн", offYes: "Зрозуміло",
    offHtml: `<p><span class="warn"><b>Моди — тільки для одиночної гри.</b></span></p><p>Перш ніж іти в онлайн — запусти гру <b>без модів</b> (вимкни мод у Frosty Mod Manager або запусти гру зі Steam / EA app). Гра в онлайні з модами може закінчитися баном акаунта EA.</p>`,
    gameT: "Де гра?", gameHtml: "<p>Не знайшов <b>Need for Speed Unbound</b> на цьому ПК.</p><p>Покажи папку гри — ту, де лежить <code>NeedForSpeedUnbound.exe</code>.</p>",
    pickFolder: "Обрати папку", exit: "Вийти", notGame: "У цій папці немає NeedForSpeedUnbound.exe:", notFmm: "У цій папці немає FrostyModManager.exe:",
    fmmT: "Frosty Mod Manager", fmmHtml: "<p>Мод вмикається через <b>Frosty Mod Manager</b> (версія HarGabt для NFS Unbound) — я його не знайшов.</p><p>Завантаж, розпакуй, запусти один раз і обери гру. Потім покажи його папку — ту, де <code>FrostyModManager.exe</code>.</p>",
    download: "Сторінка завантаження",
    keyT: "Запусти Mod Manager один раз", keyHtml: "<p>Frosty Mod Manager ще не налаштований для NFS Unbound.</p><p>Запусти його, обери гру, дочекайся завантаження, закрий — і натисни <b>Ще раз</b>.</p>",
    retry: "Ще раз", openFmm: "Відкрити Mod Manager",
    prepT: "Готуюся", prepFirst: "Перший запуск: читаю гру. Це може зайняти кілька хвилин — лише один раз.", buildT: "Збираю мод",
    builtT: "Мод готовий", builtHtml: "<p><b>UnboundSoundtrack.fbmod</b> уже в списку Frosty Mod Manager.</p><ul><li>У Mod Manager: постав галочку на <b>Unbound Soundtrack</b> і тисни <b>Launch</b>. Якщо він був відкритий — перезапусти.</li><li>Інші музичні моди вимкни: вони змінюють ті самі файли.</li></ul><p><small>{path}</small></p>",
    close: "Закрити", failT: "Щось пішло не так", openLog: "Показати лог",
    emptyT: "Тиша?", emptyHtml: "<p>У гонках або в місті не увімкнено жодного треку — там буде тихо.</p><p>Все одно зібрати?</p>",
    thanksT: "Дякуємо", cMade: "Зробили", cInside: "Усередині програми", cNeed: "Знадобиться", cLearned: "Вчилися на", cUsed: "Під час розробки", cSpecial: "Окреме спасибі",
    cFrosty: "бібліотеки для читання гри й запису модів (GPL v3)", cFork: "форк для NFS Unbound від HarGabt", cEnc: "аудіокодувальник EA — з плагіна Sound Editor у Frosty Editor (форк HarGabt), з його бібліотеками", cResearch: "скрипти для досліджень",
    cNotAff: "Не пов'язано з Electronic Arts чи Criterion Games. Need for Speed — торгова марка Electronic Arts Inc.", releases: "релізи",
    step_copyCache: "Копіюю індекс гри з Frosty…", step_firstLoad: "Читаю гру вперше — кілька хвилин…", step_loadGame: "Читаю гру…",
    step_readTracks: "Читаю список треків…", step_covers: "Беру обкладинки з гри…", step_backgrounds: "Беру фони з гри…",
    step_encode: "Кодую: {a}", step_bank: "Додаю пісні в музичний банк…", step_logic: "Записую список треків…", step_cover: "Обкладинка: {a}", step_writeMod: "Записую мод…",
    err_noGame: "Папку гри не вказано.", err_noFmm: "Frosty Mod Manager не знайдено.", err_noKey: "Frosty Mod Manager ще не налаштований для NFS Unbound: запусти його раз і обери гру.",
    err_gameChanged: "Гра змінилася (оновлення?) — ця версія програми може їй не підходити. Пошукай новішу Unbound Soundtrack.",
    err_tooManySongs: "Забагато увімкнених пісень: зараз влазить {a}. Вимкни частину пісень — або вимкни більше треків гри: програма може віддати частину їхніх місць твоїм пісням.", err_modLocked: "Не вдається записати файл мода — закрий Frosty Mod Manager і спробуй ще раз.",
    err_encodeFailed: "Кодувальник не впорався з {a}.", err_encoderMissing: "Немає кодувальника — розпакуй увесь zip ще раз.", err_songMissing: "Файлу пісні немає — додай пісню ще раз.",
    err_busy: "Зачекай — попередня робота ще не закінчилась.", err_tooLong: "Пісня задовга (максимум 20 хвилин).", err_badImage: "Ця картинка не підходить.",
    err_crash: "Неочікувана помилка. Подробиці — в лозі.", err_net: "Немає зв'язку з програмою. Вона ще працює?"
  },
  pl: {
    bugReport: "Zgłoś błąd", bugHint: "Nie pomogło? <a href=\"{url}\">Zgłoś błąd ↗</a> — opisz, co robiłeś, i wklej koniec data\\log.txt (przycisk „Pokaż log”).",
    profiles: "Profile", newProfile: "+ Nowy", newProfileT: "Nowy profil", profileName: "Nazwa profilu. To, co jest teraz włączone, zostanie do niego skopiowane.", renameT: "Zmień nazwę profilu", delProfT: "Usunąć profil?", delProfHtml: "<p>Profil „{n}” zostanie usunięty. Twoje piosenki zostają.</p>", secMine: "Moje piosenki", secGame: "Utwory gry", profSwitched: "Profil „{n}”. Kliknij <b>Zbuduj moda</b>, żeby trafił do gry.", profCreated: "Utworzono profil „{n}” — kopia obecnego zestawu.",
    pvTip: "30 sekund odsłuchu (Deezer)", legendPv: "<b>▶</b> — 30 sekund odsłuchu tutaj, <b>YouTube ↗</b> — cała piosenka.", err_noPreview: "Brak podglądu — sprawdź internet.", cPrev: "Podgląd", cPrevTxt: "30-sekundowe podglądy utworów gry, grane z ich serwerów (potrzebny internet); nic nie jest zapisywane",
    pickThis: "Wybierz ten folder", cancel: "Anuluj", pickGameHint: "Otwórz folder z NeedForSpeedUnbound.exe i kliknij „Wybierz ten folder”. ✓ zielony — właściwy.", pickFmmHint: "Otwórz folder z FrostyModManager.exe i kliknij „Wybierz ten folder”. ✓ zielony — właściwy.", fpOkGame: "✓ Gra jest tutaj", fpOkFmm: "✓ Frosty Mod Manager jest tutaj", fpNo: "Tu nie ma — otwórz właściwy folder", fpDenied: "Brak dostępu do tego folderu", fpPlaces: "Dyski i szybkie miejsca",
    folders: "Foldery", whereT: "Gra i Mod Manager", whereHtml: "<p>Gra: <code>{g}</code></p><p>Frosty Mod Manager: <code>{f}</code></p><p>Wszystko się zgadza?</p>", whereYes: "Zgadza się", whereNo: "Zmień", changeT: "Co zmienić?", changeHtml: "<p>Wybierz, który folder wskazać ponownie.</p>", changeGame: "Folder gry", changeFmm: "Folder Mod Managera",
    docTitle: "Unbound Soundtrack", langT: "Język", titleA: "Mój", titleB: "soundtrack",
    musicBtn: "Folder Music ↗", musicT: "Piosenki z folderu", musicOk: "Rozumiem", musicOpen: "Otwórz folder", musicHtml: "<p>Twoje piosenki leżą w folderze <b>Music</b> obok programu — każda we własnym folderze z numerem: <code>Music\\1\\</code>, <code>Music\\2\\</code>… W tej kolejności pokazują się w „{mine}”.</p><ul><li><b>Najprościej — przycisk „+ {add}”:</b> wybierasz plik, wpisujesz wykonawcę i tytuł — program sam utworzy kolejny folder.</li><li><b>Albo ręcznie:</b> utwórz folder z kolejnym numerem i wrzuć do niego plik piosenki (mp3, flac, wav, ogg, m4a) i obrazek na okładkę (jpg, png). Bez obrazka — narysujemy okładkę.</li><li><b>Obrazek:</b> nazwa dowolna, jeśli jest w folderze jeden; jeśli kilka — brany jest <code>cover.jpg</code> (albo ten, który nazywa się jak mp3). Rozmiar dowolny: w grze okładka ma 256×256, niekwadratową przytniemy do środka.</li><li>Wykonawca i tytuł — z tagów mp3 albo z nazwy pliku; poprawisz, klikając piosenkę.</li><li>Start wyścigu (GO) i metę program ustawia sam. Sprawdzisz — klikając piosenkę.</li><li>Usuniesz folder z Music — piosenka zniknie też z programu. Kolejność zmienisz, zmieniając nazwy folderów.</li></ul>", musicImpT: "Piosenki z Music", musicStep: "{n} z {m}: {a}", musicAdded: "Dodano z Music: {n}. GO i meta ustawione automatycznie — kliknij piosenkę, żeby sprawdzić. Potem <b>{build}</b>.", musicGone: "Usunięto: {n} — ich plików nie ma już w Music. <b>{build}</b>, żeby usunąć je też z gry.", musicFail: "Nie udało się odczytać: {a}", delMusic: "<p>Pliki w folderze Music zostają — program nie będzie już pobierał tej piosenki.</p>",
    all: "Wszystkie utwory", allSub: "soundtrack gry + twoje piosenki", on: "Grają", onSub: "włączone gdziekolwiek", off: "Wyłączone", offSub: "nie grają nigdzie",
    mine: "Moje piosenki", mineSub: "twoje własne utwory", build: "Zbuduj moda", buildSub: ".fbmod do Frosty Mod Manager",
    inRaces: "w wyścigach", inCity: "w mieście · garażu · menu", search: "SZUKAJ: UTWÓR LUB WYKONAWCA", enableAll: "Włącz wszystkie", disableAll: "Wyłącz wszystkie",
    legend: "Kliknij okładkę — cały utwór wł./wył. <b>WYŚCIGI</b> — zawody, meet-upy, takeovery. <b>MIASTO</b> — swobodna jazda, garaż, menu. <b>+ DODAJ PIOSENKĘ</b> — własny utwór z własną okładką, obok utworów gry.",
    races: "WYŚCIGI", city: "MIASTO", offStamp: "WYŁ", yours: "TWOJA", editTip: "Edytuj: nazwa, okładka, GO i meta", ytTip: "Posłuchaj na YouTube",
    madeBy: "zrobili", claudeRole: "AI · kod", devRole: "twórca", poweredBy: "działa z", about: "Podziękowania", log: "Log",
    addSong: "Dodaj piosenkę", addSongSub: "mp3 · flac · wav · ogg · m4a",
    panelNew: "Nowa piosenka", panelEdit: "Twoja piosenka", dropHere: "Upuść tu plik audio — mp3, flac, wav, ogg, m4a", pickFile: "Wybierz plik",
    artistF: "Wykonawca", titleF: "Tytuł", coverF: "Okładka", coverPick: "Wybierz obrazek", coverAuto: "Brak obrazka? Okładkę narysujemy z tytułu.", coverDrop: "albo upuść tu obrazek",
    playsIn: "Gdzie gra",
    partA: "Pełna", partAsub: "cała piosenka — miasto, garaż, menu", partB: "Start wyścigu", partBsub: "17 s narastania pod odliczanie, na GO wchodzi drop. Przeciągnij zielony znacznik GO na drop.",
    partC: "Meta", partCsub: "krótki fragment po mecie, wycisza się. Przeciągnij żółty blok.",
    save: "Zapisz", del: "Usuń", decoding: "Czytam piosenkę…", uploading: "Zapisuję piosenkę…", badFile: "Nie można odczytać pliku",
    delT: "Usunąć piosenkę?", delHtml: "<p>„{t}” zniknie z twojego soundtracku. Sam plik muzyczny zostaje na miejscu.</p><p>Żeby usunąć ją z gry — zbuduj moda jeszcze raz.</p>", yes: "Tak", no: "Nie",
    savedNote: "Zapisano. Kliknij <b>Zbuduj moda</b>, żeby trafiła do gry.",
    agreeT: "Zanim zaczniesz", agreeYes: "Zgadzam się", agreeNo: "Wyjdź",
    agreeHtml: `<p><b>Unbound Soundtrack</b> to darmowe narzędzie fanowskie od <b>Kitivski + Claude</b>. <b>Nie zostało stworzone, zatwierdzone ani wspierane przez Electronic Arts ani Criterion Games</b> i nie ma z nimi nic wspólnego. Need for Speed i NFS Unbound to znaki towarowe Electronic Arts Inc.</p>
<ul><li><b>Na własne ryzyko.</b> Narzędzie jest takie, jakie jest, bez żadnej gwarancji. Nie odpowiadamy za problemy z grą, utracone zapisy ani konta.</li>
<li><span class="warn"><b>Tylko offline.</b></span> Nie graj Online z włączonymi modami — EA może zbanować konto.</li>
<li><b>Twoja muzyka, twoja odpowiedzialność.</b> Dodawaj tylko piosenki, do których masz prawo. Nic nie jest nigdzie wysyłane: wszystko zostaje na twoim PC.</li>
<li><b>Pliki gry zostają nietknięte.</b> Narzędzie buduje moda do Frosty Mod Manager. Żeby wrócić — wyłącz tam moda.</li>
<li>Przed nowymi modami zrób kopię zapisów.</li></ul><p>Zgadzasz się korzystać z narzędzia na tych warunkach?</p>`,
    offT: "Tylko offline", offYes: "Rozumiem",
    offHtml: `<p><span class="warn"><b>Mody są tylko do gry jednoosobowej.</b></span></p><p>Zanim wejdziesz Online — uruchom grę <b>bez modów</b> (wyłącz moda we Frosty Mod Manager albo uruchom grę ze Steam / EA app). Gra Online z modami może skończyć się banem konta EA.</p>`,
    gameT: "Gdzie jest gra?", gameHtml: "<p>Nie znalazłem <b>Need for Speed Unbound</b> na tym PC.</p><p>Pokaż folder gry — ten, w którym jest <code>NeedForSpeedUnbound.exe</code>.</p>",
    pickFolder: "Wybierz folder", exit: "Wyjdź", notGame: "W tym folderze nie ma NeedForSpeedUnbound.exe:", notFmm: "W tym folderze nie ma FrostyModManager.exe:",
    fmmT: "Frosty Mod Manager", fmmHtml: "<p>Moda włącza się przez <b>Frosty Mod Manager</b> (wersja HarGabt dla NFS Unbound) — nie znalazłem go.</p><p>Pobierz, rozpakuj, uruchom raz i wybierz grę. Potem pokaż jego folder — ten z <code>FrostyModManager.exe</code>.</p>",
    download: "Strona pobierania",
    keyT: "Uruchom Mod Manager raz", keyHtml: "<p>Frosty Mod Manager nie jest jeszcze ustawiony dla NFS Unbound.</p><p>Uruchom go, wybierz grę, poczekaj aż się wczyta, zamknij — i kliknij <b>Ponów</b>.</p>",
    retry: "Ponów", openFmm: "Otwórz Mod Manager",
    prepT: "Przygotowuję", prepFirst: "Pierwsze uruchomienie: czytam grę. To może potrwać kilka minut — tylko raz.", buildT: "Buduję moda",
    builtT: "Mod gotowy", builtHtml: "<p><b>UnboundSoundtrack.fbmod</b> jest już na liście Frosty Mod Manager.</p><ul><li>W Mod Managerze: zaznacz <b>Unbound Soundtrack</b> i kliknij <b>Launch</b>. Jeśli był otwarty — uruchom go ponownie.</li><li>Wyłącz inne mody muzyczne: zmieniają te same pliki.</li></ul><p><small>{path}</small></p>",
    close: "Zamknij", failT: "Coś poszło nie tak", openLog: "Pokaż log",
    emptyT: "Cisza?", emptyHtml: "<p>W wyścigach albo w mieście nie ma żadnego włączonego utworu — będzie tam cicho.</p><p>Zbudować mimo to?</p>",
    thanksT: "Dziękujemy", cMade: "Zrobili", cInside: "W środku narzędzia", cNeed: "Będzie potrzebny", cLearned: "Uczyliśmy się na", cUsed: "Podczas tworzenia", cSpecial: "Specjalne podziękowania",
    cFrosty: "biblioteki do czytania gry i zapisu modów (GPL v3)", cFork: "fork dla NFS Unbound od HarGabt", cEnc: "koder audio EA — z wtyczki Sound Editor we Frosty Editor (fork HarGabt), z jego bibliotekami", cResearch: "skrypty badawcze",
    cNotAff: "Niezwiązane z Electronic Arts ani Criterion Games. Need for Speed to znak towarowy Electronic Arts Inc.", releases: "wydania",
    step_copyCache: "Kopiuję indeks gry z Frosty…", step_firstLoad: "Czytam grę pierwszy raz — kilka minut…", step_loadGame: "Czytam grę…",
    step_readTracks: "Czytam listę utworów…", step_covers: "Biorę okładki z gry…", step_backgrounds: "Biorę tła z gry…",
    step_encode: "Koduję: {a}", step_bank: "Dodaję piosenki do banku muzyki…", step_logic: "Zapisuję listę utworów…", step_cover: "Okładka: {a}", step_writeMod: "Zapisuję moda…",
    err_noGame: "Nie wskazano folderu gry.", err_noFmm: "Nie znaleziono Frosty Mod Manager.", err_noKey: "Frosty Mod Manager nie jest jeszcze ustawiony dla NFS Unbound: uruchom go raz i wybierz grę.",
    err_gameChanged: "Gra się zmieniła (aktualizacja?) — ta wersja narzędzia może do niej nie pasować. Poszukaj nowszego Unbound Soundtrack.",
    err_tooManySongs: "Za dużo włączonych piosenek: teraz zmieści się {a}. Wyłącz część piosenek — albo wyłącz więcej utworów gry: program może oddać część ich miejsc twoim piosenkom.", err_modLocked: "Nie mogę zapisać pliku moda — zamknij Frosty Mod Manager i spróbuj ponownie.",
    err_encodeFailed: "Koder nie poradził sobie z {a}.", err_encoderMissing: "Brak kodera — rozpakuj cały zip jeszcze raz.", err_songMissing: "Brak pliku piosenki — dodaj ją jeszcze raz.",
    err_busy: "Poczekaj — poprzednia praca jeszcze trwa.", err_tooLong: "Piosenka jest za długa (maks. 20 minut).", err_badImage: "Ten obrazek się nie nadaje.",
    err_crash: "Nieoczekiwany błąd. Szczegóły są w logu.", err_net: "Brak połączenia z programem. Czy nadal działa?"
  },
  de: {
    bugReport: "Fehler melden", bugHint: "Hat nicht geholfen? <a href=\"{url}\">Fehler melden ↗</a> — beschreib, was du gemacht hast, und füg das Ende von data\\log.txt ein (Knopf „Log zeigen“).",
    profiles: "Profile", newProfile: "+ Neu", newProfileT: "Neues Profil", profileName: "Profilname. Was gerade an ist, wird hineinkopiert.", renameT: "Profil umbenennen", delProfT: "Profil löschen?", delProfHtml: "<p>Profil „{n}“ wird gelöscht. Deine Songs bleiben.</p>", secMine: "Meine Songs", secGame: "Spiel-Tracks", profSwitched: "Profil „{n}“. Drück <b>Mod bauen</b>, damit es ins Spiel kommt.", profCreated: "Profil „{n}“ erstellt — Kopie der aktuellen Auswahl.",
    pvTip: "30 Sekunden reinhören (Deezer)", legendPv: "<b>▶</b> — 30 Sekunden direkt hier, <b>YouTube ↗</b> — der ganze Song.", err_noPreview: "Keine Vorschau — prüf die Internetverbindung.", cPrev: "Vorschau", cPrevTxt: "30-Sekunden-Vorschauen der Spiel-Tracks, abgespielt von deren Servern (Internet nötig); nichts wird gespeichert",
    pickThis: "Diesen Ordner nehmen", cancel: "Abbrechen", pickGameHint: "Öffne den Ordner mit NeedForSpeedUnbound.exe und drück „Diesen Ordner nehmen“. ✓ grün — der richtige.", pickFmmHint: "Öffne den Ordner mit FrostyModManager.exe und drück „Diesen Ordner nehmen“. ✓ grün — der richtige.", fpOkGame: "✓ Das Spiel ist hier", fpOkFmm: "✓ Frosty Mod Manager ist hier", fpNo: "Nicht hier — öffne den richtigen Ordner", fpDenied: "Kein Zugriff auf diesen Ordner", fpPlaces: "Laufwerke und Schnellzugriff",
    folders: "Ordner", whereT: "Spiel und Mod Manager", whereHtml: "<p>Spiel: <code>{g}</code></p><p>Frosty Mod Manager: <code>{f}</code></p><p>Stimmt das?</p>", whereYes: "Stimmt", whereNo: "Ändern", changeT: "Was ändern?", changeHtml: "<p>Wähle, welchen Ordner du neu zeigen willst.</p>", changeGame: "Spielordner", changeFmm: "Mod-Manager-Ordner",
    docTitle: "Unbound Soundtrack", langT: "Sprache", titleA: "Mein", titleB: "Soundtrack",
    musicBtn: "Ordner Music ↗", musicT: "Songs aus dem Ordner", musicOk: "Verstanden", musicOpen: "Ordner öffnen", musicHtml: "<p>Deine Songs liegen im Ordner <b>Music</b> neben dem Programm — jeder in einem eigenen nummerierten Ordner: <code>Music\\1\\</code>, <code>Music\\2\\</code>… In dieser Reihenfolge erscheinen sie unter „{mine}“.</p><ul><li><b>Am einfachsten — der Button „+ {add}“:</b> Datei wählen, Interpret und Titel eintippen — das Programm legt den nächsten Ordner selbst an.</li><li><b>Oder von Hand:</b> einen Ordner mit der nächsten Nummer anlegen und die Musikdatei (mp3, flac, wav, ogg, m4a) und ein Bild fürs Cover (jpg, png) hineinlegen. Kein Bild — wir zeichnen ein Cover.</li><li><b>Bild:</b> beliebiger Name, wenn es das einzige im Ordner ist; bei mehreren wird <code>cover.jpg</code> genommen (oder das, das wie die mp3 heißt). Größe egal: im Spiel ist das Cover 256×256, ein nicht quadratisches wird mittig zugeschnitten.</li><li>Interpret und Titel kommen aus den mp3-Tags oder dem Dateinamen; korrigieren — auf den Song klicken.</li><li>Rennstart (GO) und Ziel setzt das Programm selbst. Prüfen — auf den Song klicken.</li><li>Ordner aus Music gelöscht — der Song verschwindet auch aus dem Programm. Reihenfolge ändern — Ordner umbenennen.</li></ul>", musicImpT: "Songs aus Music", musicStep: "{n} von {m}: {a}", musicAdded: "Aus Music hinzugefügt: {n}. GO und Ziel wurden automatisch gesetzt — klick auf einen Song zum Prüfen. Dann <b>{build}</b>.", musicGone: "Entfernt: {n} — ihre Dateien sind nicht mehr in Music. <b>{build}</b>, um sie auch aus dem Spiel zu entfernen.", musicFail: "Konnte nicht gelesen werden: {a}", delMusic: "<p>Die Dateien im Ordner Music bleiben — das Programm holt diesen Song nicht mehr.</p>",
    all: "Alle Tracks", allSub: "Spiel-Soundtrack + deine Songs", on: "Aktiv", onSub: "irgendwo eingeschaltet", off: "Aus", offSub: "läuft nirgends",
    mine: "Meine Songs", mineSub: "deine eigenen Tracks", build: "Mod bauen", buildSub: ".fbmod für Frosty Mod Manager",
    inRaces: "in Rennen", inCity: "in Stadt · Garage · Menü", search: "SUCHE: TRACK ODER KÜNSTLER", enableAll: "Alle an", disableAll: "Alle aus",
    legend: "Klick aufs Cover — ganzer Track an/aus. <b>RENNEN</b> — Events, Meet-ups, Takeovers. <b>STADT</b> — freie Fahrt, Garage, Menü. <b>+ SONG HINZUFÜGEN</b> — eigener Track mit eigenem Cover, neben denen des Spiels.",
    races: "RENNEN", city: "STADT", offStamp: "AUS", yours: "EIGENER", editTip: "Bearbeiten: Name, Cover, GO und Ziel", ytTip: "Auf YouTube anhören",
    madeBy: "gemacht von", claudeRole: "KI · Code", devRole: "Entwickler", poweredBy: "läuft mit", about: "Credits", log: "Log",
    addSong: "Song hinzufügen", addSongSub: "mp3 · flac · wav · ogg · m4a",
    panelNew: "Neuer Song", panelEdit: "Dein Song", dropHere: "Audiodatei hier ablegen — mp3, flac, wav, ogg, m4a", pickFile: "Datei wählen",
    artistF: "Künstler", titleF: "Titel", coverF: "Cover", coverPick: "Bild wählen", coverAuto: "Kein Bild? Dann wird ein Cover aus dem Titel gezeichnet.", coverDrop: "oder ein Bild hier ablegen",
    playsIn: "Läuft in",
    partA: "Voll", partAsub: "der ganze Song — Stadt, Garage, Menü", partB: "Rennstart", partBsub: "17 s Aufbau zum Countdown, bei GO kommt der Drop. Zieh die grüne GO-Marke auf den Drop.",
    partC: "Ziel", partCsub: "kurzes Stück nach der Ziellinie, wird ausgeblendet. Zieh den gelben Block.",
    save: "Speichern", del: "Löschen", decoding: "Lese den Song…", uploading: "Speichere den Song…", badFile: "Datei kann nicht gelesen werden",
    delT: "Song löschen?", delHtml: "<p>„{t}“ wird aus deinem Soundtrack entfernt. Deine Musikdatei bleibt, wo sie ist.</p><p>Bau den Mod neu, um ihn aus dem Spiel zu entfernen.</p>", yes: "Ja", no: "Nein",
    savedNote: "Gespeichert. Drück <b>Mod bauen</b>, damit er ins Spiel kommt.",
    agreeT: "Bevor du startest", agreeYes: "Ich stimme zu", agreeNo: "Beenden",
    agreeHtml: `<p><b>Unbound Soundtrack</b> ist ein kostenloses Fan-Tool von <b>Kitivski + Claude</b>. Es ist <b>weder von Electronic Arts noch von Criterion Games erstellt, genehmigt oder unterstützt</b> und hat nichts mit ihnen zu tun. Need for Speed und NFS Unbound sind Marken von Electronic Arts Inc.</p>
<ul><li><b>Nutzung auf eigenes Risiko.</b> Das Tool kommt ohne jede Garantie. Wir haften nicht für Probleme mit dem Spiel, verlorene Spielstände oder Konten.</li>
<li><span class="warn"><b>Nur offline.</b></span> Spiel nicht Online mit aktiven Mods — EA kann dein Konto sperren.</li>
<li><b>Deine Musik, deine Verantwortung.</b> Füge nur Songs hinzu, die du nutzen darfst. Nichts wird hochgeladen: alles bleibt auf deinem PC.</li>
<li><b>Spieldateien bleiben unverändert.</b> Das Tool baut einen Mod für den Frosty Mod Manager. Zum Zurücksetzen — Mod dort ausschalten.</li>
<li>Sichere deine Spielstände, bevor du neue Mods ausprobierst.</li></ul><p>Stimmst du zu, das Tool zu diesen Bedingungen zu nutzen?</p>`,
    offT: "Bleib offline", offYes: "Verstanden",
    offHtml: `<p><span class="warn"><b>Mods sind nur für das Einzelspieler-Spiel.</b></span></p><p>Bevor du Online gehst — starte das Spiel <b>ohne Mods</b> (Mod im Frosty Mod Manager ausschalten oder das Spiel über Steam / EA app starten). Online mit Mods kann dein EA-Konto gesperrt werden.</p>`,
    gameT: "Wo ist das Spiel?", gameHtml: "<p>Ich habe <b>Need for Speed Unbound</b> auf diesem PC nicht gefunden.</p><p>Zeig mir den Spielordner — den mit <code>NeedForSpeedUnbound.exe</code>.</p>",
    pickFolder: "Ordner wählen", exit: "Beenden", notGame: "In diesem Ordner gibt es keine NeedForSpeedUnbound.exe:", notFmm: "In diesem Ordner gibt es keine FrostyModManager.exe:",
    fmmT: "Frosty Mod Manager", fmmHtml: "<p>Der Mod wird über den <b>Frosty Mod Manager</b> (HarGabts Version für NFS Unbound) eingeschaltet — ich habe ihn nicht gefunden.</p><p>Lade ihn herunter, entpacke ihn, starte ihn einmal und wähle das Spiel. Dann zeig mir seinen Ordner — den mit <code>FrostyModManager.exe</code>.</p>",
    download: "Download-Seite",
    keyT: "Mod Manager einmal starten", keyHtml: "<p>Der Frosty Mod Manager ist noch nicht für NFS Unbound eingerichtet.</p><p>Starte ihn, wähle das Spiel, warte bis er geladen hat, schließe ihn — und drück <b>Nochmal</b>.</p>",
    retry: "Nochmal", openFmm: "Mod Manager öffnen",
    prepT: "Vorbereitung", prepFirst: "Erster Start: lese das Spiel. Das kann ein paar Minuten dauern — nur einmal.", buildT: "Baue den Mod",
    builtT: "Mod fertig", builtHtml: "<p><b>UnboundSoundtrack.fbmod</b> ist in der Liste des Frosty Mod Managers.</p><ul><li>Im Mod Manager: <b>Unbound Soundtrack</b> anhaken und <b>Launch</b> drücken. War er offen — neu starten.</li><li>Andere Musik-Mods ausschalten: sie ändern dieselben Dateien.</li></ul><p><small>{path}</small></p>",
    close: "Schließen", failT: "Etwas ist schiefgelaufen", openLog: "Log zeigen",
    emptyT: "Stille?", emptyHtml: "<p>In Rennen oder in der Stadt ist kein Track aktiv — dort bleibt es still.</p><p>Trotzdem bauen?</p>",
    thanksT: "Danke", cMade: "Gemacht von", cInside: "Im Tool", cNeed: "Du brauchst", cLearned: "Gelernt von", cUsed: "Beim Bauen benutzt", cSpecial: "Besonderer Dank",
    cFrosty: "Bibliotheken zum Lesen des Spiels und Schreiben von Mods (GPL v3)", cFork: "NFS-Unbound-Fork von HarGabt", cEnc: "EA-Audio-Encoder — aus dem Sound-Editor-Plugin des Frosty Editor (HarGabt-Fork), mit seinen Bibliotheken", cResearch: "Forschungs-Skripte",
    cNotAff: "Nicht mit Electronic Arts oder Criterion Games verbunden. Need for Speed ist eine Marke von Electronic Arts Inc.", releases: "Releases",
    step_copyCache: "Kopiere Frostys Spielindex…", step_firstLoad: "Lese das Spiel zum ersten Mal — ein paar Minuten…", step_loadGame: "Lese das Spiel…",
    step_readTracks: "Lese die Trackliste…", step_covers: "Hole Cover aus dem Spiel…", step_backgrounds: "Hole Hintergründe aus dem Spiel…",
    step_encode: "Kodiere: {a}", step_bank: "Füge Songs zur Musikbank hinzu…", step_logic: "Schreibe die Trackliste…", step_cover: "Cover: {a}", step_writeMod: "Schreibe den Mod…",
    err_noGame: "Der Spielordner ist nicht gesetzt.", err_noFmm: "Frosty Mod Manager nicht gefunden.", err_noKey: "Der Frosty Mod Manager ist noch nicht für NFS Unbound eingerichtet: einmal starten und das Spiel wählen.",
    err_gameChanged: "Das Spiel hat sich geändert (Update?) — diese Version des Tools passt evtl. nicht. Schau nach einem neueren Unbound Soundtrack.",
    err_tooManySongs: "Zu viele eingeschaltete Songs: gerade passen {a} hinein. Schalte einige Songs aus — oder mehr Spiel-Tracks: das Programm kann einen Teil ihrer Plätze deinen Songs geben.", err_modLocked: "Mod-Datei kann nicht geschrieben werden — schließ den Frosty Mod Manager und versuch es nochmal.",
    err_encodeFailed: "Der Encoder ist an {a} gescheitert.", err_encoderMissing: "Der Encoder fehlt — entpacke das ganze Zip nochmal.", err_songMissing: "Die Song-Datei fehlt — füge den Song nochmal hinzu.",
    err_busy: "Warte — die vorige Aufgabe läuft noch.", err_tooLong: "Der Song ist zu lang (max. 20 Minuten).", err_badImage: "Dieses Bild geht nicht.",
    err_crash: "Unerwarteter Fehler. Details im Log.", err_net: "Keine Verbindung zum Programm. Läuft es noch?"
  },
  pt: {
    bugReport: "Relatar um bug", bugHint: "Não ajudou? <a href=\"{url}\">Relatar um bug ↗</a> — descreva o que fez e cole o final de data\\log.txt (botão “Mostrar log”).",
    profiles: "Perfis", newProfile: "+ Novo", newProfileT: "Novo perfil", profileName: "Nome do perfil. O que está ativo agora é copiado para ele.", renameT: "Renomear perfil", delProfT: "Excluir perfil?", delProfHtml: "<p>O perfil “{n}” será excluído. Suas músicas ficam.</p>", secMine: "Minhas músicas", secGame: "Faixas do jogo", profSwitched: "Perfil “{n}”. Clique em <b>Gerar mod</b> para levá-lo ao jogo.", profCreated: "Perfil “{n}” criado — cópia da seleção atual.",
    pvTip: "Prévia de 30 segundos (Deezer)", legendPv: "<b>▶</b> — prévia de 30 segundos aqui, <b>YouTube ↗</b> — a música inteira.", err_noPreview: "Sem prévia — verifique a internet.", cPrev: "Prévias", cPrevTxt: "prévias de 30 segundos das faixas do jogo, tocadas dos servidores deles (precisa de internet); nada é guardado",
    pickThis: "Usar esta pasta", cancel: "Cancelar", pickGameHint: "Abra a pasta com NeedForSpeedUnbound.exe e clique em “Usar esta pasta”. ✓ verde — a certa.", pickFmmHint: "Abra a pasta com FrostyModManager.exe e clique em “Usar esta pasta”. ✓ verde — a certa.", fpOkGame: "✓ O jogo está aqui", fpOkFmm: "✓ O Frosty Mod Manager está aqui", fpNo: "Não está aqui — abra a pasta certa", fpDenied: "Sem acesso a esta pasta", fpPlaces: "Unidades e atalhos",
    folders: "Pastas", whereT: "Jogo e Mod Manager", whereHtml: "<p>Jogo: <code>{g}</code></p><p>Frosty Mod Manager: <code>{f}</code></p><p>Está certo?</p>", whereYes: "Está certo", whereNo: "Mudar", changeT: "O que mudar?", changeHtml: "<p>Escolha qual pasta mostrar de novo.</p>", changeGame: "Pasta do jogo", changeFmm: "Pasta do Mod Manager",
    docTitle: "Unbound Soundtrack", langT: "Idioma", titleA: "Minha", titleB: "trilha sonora",
    musicBtn: "Pasta Music ↗", musicT: "Músicas de uma pasta", musicOk: "Entendi", musicOpen: "Abrir pasta", musicHtml: "<p>Suas músicas ficam na pasta <b>Music</b> ao lado do programa — cada uma na sua pasta numerada: <code>Music\\1\\</code>, <code>Music\\2\\</code>… Nessa ordem elas aparecem em “{mine}”.</p><ul><li><b>O mais fácil — o botão “+ {add}”:</b> escolha o arquivo, digite artista e título — o programa cria a próxima pasta sozinho.</li><li><b>Ou à mão:</b> crie uma pasta com o próximo número e coloque nela o arquivo da música (mp3, flac, wav, ogg, m4a) e uma imagem para a capa (jpg, png). Sem imagem — desenhamos uma capa.</li><li><b>Imagem:</b> qualquer nome, se for a única na pasta; se houver várias — usa <code>cover.jpg</code> (ou a que tem o mesmo nome do mp3). Qualquer tamanho: no jogo a capa é 256×256; se não for quadrada, cortamos pelo centro.</li><li>Artista e título vêm das tags do mp3 ou do nome do arquivo; para corrigir — clique na música.</li><li>O início da corrida (GO) e o final são definidos automaticamente. Para conferir — clique na música.</li><li>Apagou uma pasta da Music — a música some do programa também. Para mudar a ordem — renomeie as pastas.</li></ul>", musicImpT: "Músicas da Music", musicStep: "{n} de {m}: {a}", musicAdded: "Adicionadas da Music: {n}. GO e final definidos automaticamente — clique numa música para conferir. Depois <b>{build}</b>.", musicGone: "Removidas: {n} — os arquivos não estão mais na Music. <b>{build}</b> para tirá-las do jogo também.", musicFail: "Não deu para ler: {a}", delMusic: "<p>Os arquivos na pasta Music ficam — o programa não vai mais puxar esta música.</p>",
    all: "Todas as faixas", allSub: "trilha do jogo + suas músicas", on: "Tocando", onSub: "ativadas em algum lugar", off: "Desativadas", offSub: "não tocam em lugar nenhum",
    mine: "Minhas músicas", mineSub: "suas próprias faixas", build: "Gerar mod", buildSub: ".fbmod para o Frosty Mod Manager",
    inRaces: "nas corridas", inCity: "na cidade · garagem · menu", search: "BUSCAR: FAIXA OU ARTISTA", enableAll: "Ativar todas", disableAll: "Desativar todas",
    legend: "Clique na capa — liga/desliga a faixa inteira. <b>CORRIDAS</b> — eventos, meet-ups, takeovers. <b>CIDADE</b> — rodar livre, garagem, menu. <b>+ ADICIONAR MÚSICA</b> — sua própria faixa com capa própria, junto das do jogo.",
    races: "CORRIDAS", city: "CIDADE", offStamp: "OFF", yours: "SUA", editTip: "Editar: nome, capa, GO e final", ytTip: "Ouvir no YouTube",
    madeBy: "feito por", claudeRole: "IA · código", devRole: "desenvolvedor", poweredBy: "roda com", about: "Créditos", log: "Log",
    addSong: "Adicionar música", addSongSub: "mp3 · flac · wav · ogg · m4a",
    panelNew: "Nova música", panelEdit: "Sua música", dropHere: "Solte um arquivo de áudio aqui — mp3, flac, wav, ogg, m4a", pickFile: "Escolher arquivo",
    artistF: "Artista", titleF: "Título", coverF: "Capa", coverPick: "Escolher imagem", coverAuto: "Sem imagem? A capa é desenhada a partir do título.", coverDrop: "ou solte uma imagem aqui",
    playsIn: "Toca em",
    partA: "Completa", partAsub: "a música inteira — cidade, garagem, menu", partB: "Largada", partBsub: "17 s de tensão na contagem, o drop entra no GO. Arraste o marcador verde GO até o drop.",
    partC: "Chegada", partCsub: "trecho curto depois da linha de chegada, com fade. Arraste o bloco amarelo.",
    save: "Salvar", del: "Excluir", decoding: "Lendo a música…", uploading: "Salvando a música…", badFile: "Não foi possível ler o arquivo",
    delT: "Excluir música?", delHtml: "<p>“{t}” sai da sua trilha. Seu arquivo de música continua onde está.</p><p>Gere o mod de novo para tirá-la do jogo.</p>", yes: "Sim", no: "Não",
    savedNote: "Salvo. Clique em <b>Gerar mod</b> para colocá-la no jogo.",
    agreeT: "Antes de começar", agreeYes: "Eu concordo", agreeNo: "Sair",
    agreeHtml: `<p><b>Unbound Soundtrack</b> é uma ferramenta gratuita feita por fãs: <b>Kitivski + Claude</b>. <b>Não foi feita, aprovada nem é apoiada pela Electronic Arts ou pela Criterion Games</b>, e não tem nada a ver com elas. Need for Speed e NFS Unbound são marcas da Electronic Arts Inc.</p>
<ul><li><b>Use por sua conta e risco.</b> A ferramenta vem como está, sem garantia. Não nos responsabilizamos por problemas no jogo, saves ou contas perdidos.</li>
<li><span class="warn"><b>Só offline.</b></span> Não jogue Online com mods ativos — a EA pode banir sua conta.</li>
<li><b>Sua música, sua responsabilidade.</b> Adicione só músicas que você tem direito de usar. Nada é enviado para lugar nenhum: tudo fica no seu PC.</li>
<li><b>Os arquivos do jogo não são alterados.</b> A ferramenta gera um mod para o Frosty Mod Manager. Para voltar — desative o mod lá.</li>
<li>Faça backup dos seus saves antes de testar mods novos.</li></ul><p>Você concorda em usar a ferramenta nestes termos?</p>`,
    offT: "Fique offline", offYes: "Entendi",
    offHtml: `<p><span class="warn"><b>Mods são só para o modo single-player.</b></span></p><p>Antes de entrar Online — abra o jogo <b>sem mods</b> (desative o mod no Frosty Mod Manager ou abra o jogo pela Steam / EA app). Jogar Online com mods pode banir sua conta EA.</p>`,
    gameT: "Onde está o jogo?", gameHtml: "<p>Não encontrei o <b>Need for Speed Unbound</b> neste PC.</p><p>Mostre a pasta do jogo — a que tem <code>NeedForSpeedUnbound.exe</code>.</p>",
    pickFolder: "Escolher pasta", exit: "Sair", notGame: "Não há NeedForSpeedUnbound.exe nesta pasta:", notFmm: "Não há FrostyModManager.exe nesta pasta:",
    fmmT: "Frosty Mod Manager", fmmHtml: "<p>O mod é ativado pelo <b>Frosty Mod Manager</b> (versão do HarGabt para NFS Unbound) — não o encontrei.</p><p>Baixe, descompacte, abra uma vez e escolha o jogo. Depois mostre a pasta dele — a que tem <code>FrostyModManager.exe</code>.</p>",
    download: "Página de download",
    keyT: "Abra o Mod Manager uma vez", keyHtml: "<p>O Frosty Mod Manager ainda não foi configurado para o NFS Unbound.</p><p>Abra-o, escolha o jogo, espere carregar, feche — e clique em <b>Tentar de novo</b>.</p>",
    retry: "Tentar de novo", openFmm: "Abrir Mod Manager",
    prepT: "Preparando", prepFirst: "Primeira vez: lendo o jogo. Pode levar alguns minutos — só uma vez.", buildT: "Gerando o mod",
    builtT: "Mod pronto", builtHtml: "<p><b>UnboundSoundtrack.fbmod</b> já está na lista do Frosty Mod Manager.</p><ul><li>No Mod Manager: marque <b>Unbound Soundtrack</b> e clique em <b>Launch</b>. Se estava aberto — reinicie.</li><li>Desative outros mods de música: eles mudam os mesmos arquivos.</li></ul><p><small>{path}</small></p>",
    close: "Fechar", failT: "Algo deu errado", openLog: "Mostrar log",
    emptyT: "Silêncio?", emptyHtml: "<p>Nenhuma faixa ativa nas corridas ou na cidade — vai ficar em silêncio.</p><p>Gerar mesmo assim?</p>",
    thanksT: "Obrigado", cMade: "Feito por", cInside: "Dentro da ferramenta", cNeed: "Você vai precisar", cLearned: "Aprendemos com", cUsed: "Usado no desenvolvimento", cSpecial: "Agradecimento especial",
    cFrosty: "bibliotecas para ler o jogo e gravar mods (GPL v3)", cFork: "fork para NFS Unbound do HarGabt", cEnc: "codificador de áudio da EA — do plugin Sound Editor do Frosty Editor (fork do HarGabt), com suas bibliotecas", cResearch: "scripts de pesquisa",
    cNotAff: "Sem vínculo com a Electronic Arts ou a Criterion Games. Need for Speed é marca da Electronic Arts Inc.", releases: "versões",
    step_copyCache: "Copiando o índice do jogo do Frosty…", step_firstLoad: "Lendo o jogo pela primeira vez — alguns minutos…", step_loadGame: "Lendo o jogo…",
    step_readTracks: "Lendo a lista de faixas…", step_covers: "Pegando capas do jogo…", step_backgrounds: "Pegando fundos do jogo…",
    step_encode: "Codificando: {a}", step_bank: "Adicionando músicas ao banco de música…", step_logic: "Gravando a lista de faixas…", step_cover: "Capa: {a}", step_writeMod: "Gravando o mod…",
    err_noGame: "A pasta do jogo não foi definida.", err_noFmm: "Frosty Mod Manager não encontrado.", err_noKey: "O Frosty Mod Manager ainda não foi configurado para o NFS Unbound: abra-o uma vez e escolha o jogo.",
    err_gameChanged: "O jogo mudou (atualização?) — esta versão da ferramenta pode não servir. Procure um Unbound Soundtrack mais novo.",
    err_tooManySongs: "Músicas ativas demais: agora cabem {a}. Desative algumas músicas — ou desative mais faixas do jogo: o programa pode dar parte dos lugares delas às suas músicas.", err_modLocked: "Não consigo gravar o arquivo do mod — feche o Frosty Mod Manager e tente de novo.",
    err_encodeFailed: "O codificador falhou em {a}.", err_encoderMissing: "Falta o codificador — descompacte o zip inteiro de novo.", err_songMissing: "O arquivo da música sumiu — adicione a música de novo.",
    err_busy: "Espere — o trabalho anterior ainda não terminou.", err_tooLong: "A música é longa demais (máx. 20 minutos).", err_badImage: "Esta imagem não serve.",
    err_crash: "Erro inesperado. Os detalhes estão no log.", err_net: "Sem conexão com o programa. Ele ainda está aberto?"
  }
};
const LANGS = [["uk", "UA"], ["pl", "PL"], ["en", "EN"], ["de", "DE"], ["pt", "PT"]];
const LANG_NAMES = { uk: "Українська", pl: "Polski", en: "English", de: "Deutsch", pt: "Português (Brasil)" };
let LANG = (() => {
  try { const s = localStorage.getItem("lang"); if (s && I18N[s]) return s; } catch (e) {}
  const n = (navigator.language || "en").slice(0, 2).toLowerCase();
  return I18N[n] ? n : "en";
})();

function t(k, p) {
  let s = (I18N[LANG] && I18N[LANG][k]) ?? I18N.en[k] ?? k;
  if (p) for (const [a, b] of Object.entries(p)) s = s.split("{" + a + "}").join(b);
  return s;
}

function applyI18n(titleKey) {
  document.documentElement.lang = LANG;
  document.querySelectorAll("[data-i18n]").forEach(e => e.innerHTML = t(e.dataset.i18n));
  document.querySelectorAll("[data-i18n-ph]").forEach(e => e.placeholder = t(e.dataset.i18nPh));
  document.querySelectorAll("[data-i18n-title]").forEach(e => e.title = t(e.dataset.i18nTitle));
  if (titleKey) document.title = t(titleKey);
}

function langSwitcher(el, onChange) {
  el.innerHTML = LANGS.map(([c, l]) => `<span data-l="${c}" class="${c === LANG ? "on" : ""}">${l}</span>`).join("");
  el.onclick = e => {
    const c = e.target.dataset.l; if (!c || c === LANG) return;
    LANG = c; try { localStorage.setItem("lang", c); } catch (err) {}
    langSwitcher(el, onChange); onChange && onChange();
  };
}
