UNBOUND SOUNDTRACK  v1.0.1
Deine eigenen Songs in Need for Speed Unbound — als neue Tracks mit eigenen Covern.
Kostenloses Fan-Tool von Kitivski + Claude. NICHT mit Electronic Arts oder Criterion Games verbunden.
Website: https://kotovskivan.github.io/UnboundSoundtrack/

English — README.txt neben dem Programm.
Übersetzungen mit KI erstellt (Claude).

=====================================================================
WAS ES MACHT
  - Spiel-Tracks getrennt für Rennen und Stadt an-/ausschalten.
  - Eigene Songs (mp3, flac, wav, ogg, m4a) als NEUE Tracks mit eigenem Cover hinzufügen.
    Die Tracks des Spiels bleiben, wie sie sind.
  - Songs von YouTube / YT Music: Link zu Song, Playlist oder Album, viele Links auf einmal oder eine .txt-Liste.
    Ist der Link ein Musikvideo, nimmt das Programm denselben Song von YT Music — ohne Intros und Szenen aus dem Video.
  - Pro Song den Rennstart (17 s Zwischensequenz + 3,45 s Countdown, bei GO kommt der Drop) und das Zielstück einstellen.
  - HIGH / LOW auf der Karte — wie oft ein Track läuft (das Spiel nimmt zuerst HIGH).
  - Profile: verschiedene Track-Sets, Umschalten mit einem Klick.
  - Baut EINEN Mod für den Frosty Mod Manager. Spieldateien bleiben unverändert.
  - ▶ bei einem Spiel-Track — 30 Sekunden reinhören (Deezer, braucht Internet); YouTube ↗ — der ganze Song.
  - Das Fenster spricht 13 Sprachen.

DU BRAUCHST
  - Windows 10 oder 11 (Microsoft Edge ist schon eingebaut — das Fenster läuft darauf).
  - Need for Speed Unbound (Steam oder EA app).
  - Frosty Mod Manager, die Version von HarGabt für NFS Unbound:
    https://github.com/HarGabt/FrostyToolsuite/releases
    Starte ihn einmal und wähle das Spiel, bevor du dieses Tool benutzt.
    Beim ersten Start fragt er nach dem Verschlüsselungsschlüssel des Spiels — NFS Unbound Encryption Key:
    https://www.nexusmods.com/needforspeedunbound/mods/97
    Wenn du zum ersten Mal Launch drückst (und nachdem der Ordner ModData gelöscht wurde), fragt Frosty
    nach Administratorrechten, um Links zu den Spieldaten anzulegen ("New installation detected") —
    das ist normal, bestätige es.

SO GEHT'S
  1. Entpacke das GANZE Zip in einen normalen Ordner (Desktop, Dokumente, D:\Mods…),
     nicht nach Program Files.
  2. Starte "Unbound Soundtrack.exe".
  3. Wähle die Sprache, nimm die Bedingungen an. Der erste Start liest das Spiel — ein paar Minuten, nur einmal.
  4. + SONG HINZUFÜGEN → Datei wählen → GO und Ziel setzen → Speichern.
     Oder ▶ AUS YT MUSIC → Link einfügen (Song, Playlist, Album) → Herunterladen.
     Alle Songs liegen im Ordner "Music" neben dem Programm, jeder in einem nummerierten Ordner:
     Music\1\, Music\2\… — die Reihenfolge im Fenster. + SONG HINZUFÜGEN legt den nächsten Ordner selbst an.
     Von Hand: neuer nummerierter Ordner mit der Musikdatei + einem Bild (jpg, png) fürs Cover;
     GO und Ziel werden automatisch gesetzt. Ordner gelöscht — der Song verschwindet aus dem Programm.
     Bild: Name egal, wenn es das einzige im Ordner ist; mehrere — cover.jpg gewinnt. Größe egal (im Spiel 256x256).
  5. MOD BAUEN. Der Mod erscheint in der Liste des Frosty Mod Manager.
  6. Im Frosty Mod Manager: hak "Unbound Soundtrack" an, schalte andere Musik-Mods aus, drück Launch.

NUR OFFLINE
  Mods sind fürs Einzelspieler-Spiel. Bevor du online gehst, starte das Spiel ohne Mods.
  Online mit Mods zu spielen kann zur Sperre deines EA-Kontos führen.

"DER COMPUTER WURDE DURCH WINDOWS GESCHÜTZT" / "UNBEKANNTER HERAUSGEBER"
  Das Programm ist nicht mit einem kostenpflichtigen Zertifikat signiert, deshalb warnt Windows SmartScreen davor.
  Klick auf "Weitere Informationen" → "Trotzdem ausführen". Der Quellcode liegt im Ordner "source".

WAS IM ORDNER IST
  Unbound Soundtrack.exe   das Programm
  app\web                  das Fenster (Seiten, Texte in 13 Sprachen)
  app\frosty               Bibliotheken von Frosty Toolsuite — lesen das Spiel, schreiben den Mod (GPL v3)
  app\encoder              EA-Audio-Encoder aus dem Plugin Sound Editor von Frosty Editor
  app\yt-dlp.exe           lädt Songs von YouTube / YT Music herunter (Unlicense)
  source\                  Quellcode dieses Programms (GPL v3)
  README\                  dieser Text in anderen Sprachen
  data\                    entsteht beim ersten Start: deine Einstellungen, Songs, Bilder aus DEINEM Spiel, Log
  Music\                   deine Songs, je ein nummerierter Ordner: Musikdatei, Cover, song.json (Name, GO, Ziel)
  THIRD-PARTY.txt          was wir nutzen, Autoren, Lizenzen, Links
  Keine Dateien des Spiels oder von EA dabei: Cover und Hintergründe werden aus deinem eigenen Spiel gelesen.

WENN ETWAS NICHT GEHT
  - Das Log ist data\log.txt (Button "Log" im Fenster).
  - Nach einem Spiel-Update liest das Tool das Spiel von selbst neu ein. Bricht das Bauen mit
    "Das Spiel hat sich geändert" ab — schau auf der Website nach einer neueren Version.
  - Neu anfangen: schließ das Programm und lösch den Ordner "data".
  - Fehler melden: https://forms.gle/StNa4GFZRHckFVpi8  (beschreib, was du gemacht hast, füg das Ende von data\log.txt ein).

ENTFERNEN
  Schalte den Mod im Frosty Mod Manager aus und lösch den Ordner mit dem Programm.

---------------------------------------------------------------------
Need for Speed und NFS Unbound sind Marken von Electronic Arts Inc.
Dieses Tool wurde weder von Electronic Arts noch von Criterion Games erstellt, genehmigt oder unterstützt.
Credits und Lizenzen: THIRD-PARTY.txt und der Button "Credits" im Programm.
