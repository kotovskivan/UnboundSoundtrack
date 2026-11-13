UNBOUND SOUNDTRACK  v1.0.1
Twoje piosenki w Need for Speed Unbound — jako nowe utwory z własnymi okładkami.
Darmowe narzędzie fanowskie od Kitivski + Claude. NIE jest powiązane z Electronic Arts ani Criterion Games.
Strona: https://kotovskivan.github.io/UnboundSoundtrack/

English — README.txt obok programu.
Tłumaczenia wykonane przez AI (Claude).

=====================================================================
CO ROBI
  - Włącza/wyłącza utwory gry osobno w wyścigach i w mieście.
  - Dodaje twoje piosenki (mp3, flac, wav, ogg, m4a) jako NOWE utwory z własną okładką.
    Utwory gry zostają bez zmian.
  - Piosenki z YouTube / YT Music: link do piosenki, playlisty lub albumu, wiele linków naraz albo lista w .txt.
    Jeśli link prowadzi do teledysku, program bierze tę samą piosenkę z YT Music — bez wstawek i scenek z klipu.
  - Dla każdej piosenki: start wyścigu (17 s przerywnika + 3,45 s odliczania, na GO wchodzi drop) i fragment na metę.
  - HIGH / LOW na karcie — jak często gra utwór (gra najpierw bierze HIGH).
  - Profile: różne zestawy utworów, przełączanie jednym kliknięciem.
  - Buduje JEDEN mod do Frosty Mod Manager. Pliki gry się nie zmieniają.
  - ▶ na utworze gry — 30 sekund odsłuchu (Deezer, potrzebny internet); YouTube ↗ — cała piosenka.
  - Okno mówi w 14 językach.

POTRZEBUJESZ
  - Windows 10 lub 11 (Microsoft Edge jest wbudowany — okno z niego korzysta).
  - Need for Speed Unbound (Steam lub EA app).
  - Frosty Mod Manager w wersji HarGabt dla NFS Unbound:
    https://github.com/HarGabt/FrostyToolsuite/releases
    Uruchom go raz i wybierz grę, zanim zaczniesz korzystać z tego programu.
    Przy pierwszym uruchomieniu prosi o klucz szyfrowania gry — NFS Unbound Encryption Key:
    https://www.nexusmods.com/needforspeedunbound/mods/97
    Gdy pierwszy raz naciśniesz Launch (i po usunięciu folderu ModData), Frosty prosi o uprawnienia administratora,
    żeby utworzyć łącza do danych gry ("New installation detected") — to normalne, zgódź się.

JAK UŻYWAĆ
  1. Rozpakuj CAŁY zip do zwykłego folderu (Pulpit, Dokumenty, D:\Mods…),
     nie do Program Files.
  2. Uruchom "Unbound Soundtrack.exe".
  3. Wybierz język, zaakceptuj warunki. Pierwsze uruchomienie czyta grę — kilka minut, tylko raz.
  4. + DODAJ PIOSENKĘ → wybierz plik → ustaw GO i metę → Zapisz.
     Albo ▶ Z YT MUSIC → wklej link (piosenka, playlista, album) → Pobierz.
     Wszystkie piosenki leżą w folderze "Music" obok programu, każda w folderze z numerem:
     Music\1\, Music\2\… — to kolejność w oknie. + DODAJ PIOSENKĘ sam tworzy kolejny folder.
     Ręcznie: nowy folder z numerem, w nim plik piosenki + obrazek (jpg, png) na okładkę;
     GO i meta ustawią się automatycznie. Usuniesz folder — piosenka zniknie z programu.
     Obrazek: dowolna nazwa, gdy jest w folderze jeden; kilka — brany jest cover.jpg. Rozmiar dowolny (w grze 256x256).
  5. ZBUDUJ MODA. Mod pojawi się na liście we Frosty Mod Manager.
  6. We Frosty Mod Manager: zaznacz "Unbound Soundtrack", wyłącz inne mody muzyczne, naciśnij Launch.

TYLKO OFFLINE
  Mody są do gry dla jednego gracza. Zanim wejdziesz do gry Online, uruchom grę bez modów.
  Granie Online z modami grozi banem konta EA.

"SYSTEM WINDOWS OCHRONIŁ TEN KOMPUTER" / "NIEZNANY WYDAWCA"
  Program nie jest podpisany płatnym certyfikatem, więc Windows SmartScreen przed nim ostrzega.
  Kliknij "Więcej informacji" → "Uruchom mimo to". Kod źródłowy jest w folderze "source".

CO JEST W FOLDERZE
  Unbound Soundtrack.exe   program
  app\web                  okno (strony, teksty w 14 językach)
  app\frosty               biblioteki Frosty Toolsuite — czytają grę, zapisują mod (GPL v3)
  app\encoder              koder audio EA z wtyczki Sound Editor do Frosty Editor
  app\yt-dlp.exe           pobiera piosenki z YouTube / YT Music (Unlicense)
  source\                  kod źródłowy tego programu (GPL v3)
  README\                  ten tekst w innych językach
  data\                    tworzony przy pierwszym uruchomieniu: twoje ustawienia, piosenki, obrazki z TWOJEJ gry, log
  Music\                   twoje piosenki, każda w folderze z numerem: plik piosenki, okładka, song.json (nazwa, GO, meta)
  THIRD-PARTY.txt          czego używamy, autorzy, licencje, linki
  W archiwum nie ma plików gry ani EA: okładki i tła są czytane z twojej własnej gry.

JEŚLI COŚ NIE DZIAŁA
  - Log to data\log.txt (przycisk "Log" w oknie).
  - Po aktualizacji gry program sam ponownie wczyta grę. Jeśli budowanie kończy się błędem
    "Gra się zmieniła" — poszukaj nowszej wersji na stronie.
  - Żeby zacząć od nowa: zamknij program i usuń folder "data".
  - Zgłoś błąd: https://forms.gle/StNa4GFZRHckFVpi8  (opisz, co robiłeś, wklej koniec data\log.txt).

JAK USUNĄĆ
  Wyłącz mod we Frosty Mod Manager i usuń folder z programem.

---------------------------------------------------------------------
Need for Speed i NFS Unbound to znaki towarowe Electronic Arts Inc.
To narzędzie nie zostało stworzone, zatwierdzone ani wspierane przez Electronic Arts ani Criterion Games.
Podziękowania i licencje: THIRD-PARTY.txt i przycisk "Podziękowania" w programie.
