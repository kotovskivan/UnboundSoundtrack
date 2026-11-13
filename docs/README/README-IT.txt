UNBOUND SOUNDTRACK  v1.0.1
Le tue canzoni in Need for Speed Unbound — come brani nuovi, con le tue copertine.
Programma gratuito fatto da fan: Kitivski + Claude. NON affiliato con Electronic Arts o Criterion Games.
Sito: https://kotovskivan.github.io/UnboundSoundtrack/

English — README.txt accanto al programma.
Traduzioni fatte con l'IA (Claude).

=====================================================================
COSA FA
  - Attiva/disattiva i brani del gioco separatamente per le gare e per la città.
  - Aggiunge le tue canzoni (mp3, flac, wav, ogg, m4a) come brani NUOVI con la tua copertina.
    I brani del gioco restano come sono.
  - Canzoni da YouTube / YT Music: il link di una canzone, playlist o album, più link insieme o un elenco .txt.
    Se il link è un videoclip, il programma prende la stessa canzone da YT Music — senza intro e scenette del video.
  - Per ogni canzone imposti la partenza (17 s di filmato + 3,45 s di conto alla rovescia, il drop arriva sul GO)
    e il pezzo per l'arrivo.
  - HIGH / LOW sulla scheda — quanto spesso suona un brano (il gioco sceglie prima gli HIGH).
  - Profili: diversi set di brani, si cambiano con un clic.
  - Crea UN solo mod per Frosty Mod Manager. I file del gioco non vengono modificati.
  - ▶ su un brano del gioco — anteprima di 30 secondi (Deezer, serve internet); YouTube ↗ — la canzone intera.
  - La finestra parla 13 lingue.

COSA TI SERVE
  - Windows 10 o 11 (Microsoft Edge è integrato — la finestra lo usa).
  - Need for Speed Unbound (Steam o EA app).
  - Frosty Mod Manager, la versione di HarGabt per NFS Unbound:
    https://github.com/HarGabt/FrostyToolsuite/releases
    Avvialo una volta e scegli il gioco prima di usare questo programma.
    Al primo avvio chiede la chiave di crittografia del gioco — NFS Unbound Encryption Key:
    https://www.nexusmods.com/needforspeedunbound/mods/97
    Al suo primo Launch (e dopo aver cancellato la cartella ModData) Frosty chiede i diritti di amministratore
    per creare collegamenti ai dati del gioco ("New installation detected") — è normale, accetta.

COME SI USA
  1. Estrai TUTTO lo zip in una cartella normale (Desktop, Documenti, D:\Mods…),
     non in Program Files.
  2. Avvia "Unbound Soundtrack.exe".
  3. Scegli la lingua, accetta le condizioni. Il primo avvio legge il gioco — qualche minuto, solo una volta.
  4. + AGGIUNGI CANZONE → scegli un file → imposta GO e l'arrivo → Salva.
     Oppure ▶ DA YT MUSIC → incolla un link (canzone, playlist, album) → Scarica.
     Tutte le canzoni stanno nella cartella "Music" accanto al programma, una cartella numerata per canzone:
     Music\1\, Music\2\… — l'ordine nella finestra. + AGGIUNGI CANZONE crea da solo la cartella successiva.
     A mano: una nuova cartella numerata con il file della canzone + un'immagine (jpg, png) per la copertina;
     GO e l'arrivo vengono impostati in automatico. Cancelli una cartella — la canzone sparisce dal programma.
     Immagine: nome qualsiasi se è l'unica nella cartella; più di una — vince cover.jpg. Qualsiasi misura (il gioco usa 256x256).
  5. CREA MOD. Il mod compare nella lista di Frosty Mod Manager.
  6. In Frosty Mod Manager: spunta "Unbound Soundtrack", disattiva gli altri mod musicali, premi Launch.

SOLO OFFLINE
  I mod sono per il gioco in singolo. Prima di andare Online, avvia il gioco senza mod.
  Giocare Online con i mod può costarti il ban dell'account EA.

"PC PROTETTO DA WINDOWS" / "AUTORE SCONOSCIUTO"
  Il programma non è firmato con un certificato a pagamento, quindi Windows SmartScreen mostra un avviso.
  Clicca su "Ulteriori informazioni" → "Esegui comunque". Il codice sorgente è nella cartella "source".

COSA C'È NELLA CARTELLA
  Unbound Soundtrack.exe   il programma
  app\web                  la finestra (pagine, testi in 13 lingue)
  app\frosty               librerie di Frosty Toolsuite — leggono il gioco, scrivono il mod (GPL v3)
  app\encoder              encoder audio di EA, dal plugin Sound Editor di Frosty Editor
  app\yt-dlp.exe           scarica le canzoni da YouTube / YT Music (Unlicense)
  source\                  codice sorgente di questo programma (GPL v3)
  README\                  questo testo in altre lingue
  data\                    creata al primo avvio: le tue impostazioni, canzoni, immagini lette dal TUO gioco, log
  Music\                   le tue canzoni, ognuna nella sua cartella numerata: file, copertina, song.json (nome, GO, arrivo)
  THIRD-PARTY.txt          cosa usiamo, autori, licenze, link
  Nessun file del gioco o di EA è incluso: copertine e sfondi vengono letti dal tuo gioco.

SE QUALCOSA NON FUNZIONA
  - Il log è in data\log.txt (pulsante "Log" nella finestra).
  - Dopo un aggiornamento del gioco: il programma rilegge il gioco da solo. Se la creazione del mod fallisce con
    "Il gioco è cambiato" — cerca una versione più recente sul sito.
  - Per ricominciare da zero: chiudi il programma e cancella la cartella "data".
  - Segnala un bug: https://forms.gle/StNa4GFZRHckFVpi8  (descrivi cosa hai fatto, incolla la fine di data\log.txt).

RIMUOVERE
  Disattiva il mod in Frosty Mod Manager e cancella la cartella del programma.

---------------------------------------------------------------------
Need for Speed e NFS Unbound sono marchi commerciali di Electronic Arts Inc.
Questo programma non è stato creato, approvato né supportato da Electronic Arts o Criterion Games.
Crediti e licenze: THIRD-PARTY.txt e il pulsante "Crediti" nel programma.
