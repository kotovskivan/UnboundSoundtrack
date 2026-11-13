UNBOUND SOUNDTRACK  v1.0.1
Omat biisisi Need for Speed Unboundiin — uusina kappaleina omilla kansikuvilla.
Ilmainen fanityökalu, tekijät Kitivski + Claude. EI yhteydessä Electronic Artsiin tai Criterion Gamesiin.
Verkkosivu: https://kotovskivan.github.io/UnboundSoundtrack/

English — README.txt ohjelman vieressä.
Käännökset tehty tekoälyllä (Claude).

=====================================================================
MITÄ SE TEKEE
  - Pelin kappaleet päälle/pois erikseen kisoihin ja kaupunkiin.
  - Omat biisit (mp3, flac, wav, ogg, m4a) UUSINA kappaleina omalla kansikuvalla.
    Pelin kappaleet pysyvät ennallaan.
  - Biisit YouTubesta / YT Musicista: linkki biisiin, soittolistaan tai albumiin, monta linkkiä kerralla tai .txt-lista.
    Jos linkki on musiikkivideo, ohjelma ottaa saman biisin YT Musicista — ilman videon introja ja sketsejä.
  - Jokaiselle biisille kisan lähtö (17 s välivideo + 3,45 s lähtölaskenta, drop iskee kohdassa GO) ja maalin pätkä.
  - HIGH / LOW kortissa — kuinka usein kappale soi (peli valitsee ensin HIGH).
  - Profiilit: eri kappalevalikoimat, vaihto yhdellä klikkauksella.
  - Rakentaa YHDEN modin Frosty Mod Manageriin. Pelin tiedostoja ei muuteta.
  - ▶ pelin kappaleessa — 30 sekunnin näyte (Deezer, tarvitaan internet); YouTube ↗ — koko kappale.
  - Ikkuna puhuu 14 kieltä.

TARVITSET
  - Windows 10 tai 11 (Microsoft Edge on valmiina — ikkuna käyttää sitä).
  - Need for Speed Unbound (Steam tai EA app).
  - Frosty Mod Manager, HarGabtin versio NFS Unboundille:
    https://github.com/HarGabt/FrostyToolsuite/releases
    Käynnistä se kerran ja valitse peli ennen kuin käytät tätä työkalua.
    Ensimmäisellä käynnistyksellä se kysyy pelin salausavainta — NFS Unbound Encryption Key:
    https://www.nexusmods.com/needforspeedunbound/mods/97
    Kun painat Launch ensimmäistä kertaa (ja kun ModData-kansio on poistettu), Frosty pyytää
    järjestelmänvalvojan oikeuksia luodakseen linkit pelin tietoihin ("New installation detected") —
    se on normaalia, hyväksy.

NÄIN KÄYTÄT
  1. Pura KOKO zip tavalliseen kansioon (Työpöytä, Tiedostot, D:\Mods…),
     ei Program Filesiin.
  2. Käynnistä "Unbound Soundtrack.exe".
  3. Valitse kieli, hyväksy ehdot. Ensimmäinen käynnistys lukee pelin — muutama minuutti, vain kerran.
  4. + LISÄÄ BIISI → valitse tiedosto → aseta GO ja maali → Tallenna.
     Tai ▶ YT MUSICISTA → liitä linkki (biisi, soittolista, albumi) → Lataa.
     Kaikki biisit ovat ohjelman vieressä olevassa "Music"-kansiossa, jokainen omassa numeroidussa kansiossaan:
     Music\1\, Music\2\… — järjestys ikkunassa. + LISÄÄ BIISI luo seuraavan kansion itse.
     Käsin: uusi numeroitu kansio, jossa on biisin tiedosto + kuva (jpg, png) kansikuvaksi;
     GO ja maali asetetaan automaattisesti. Poista kansio — biisi katoaa ohjelmasta.
     Kuva: nimi vapaa, jos se on kansion ainoa kuva; useita — cover.jpg voittaa. Koko vapaa (peli käyttää 256x256).
  5. RAKENNA MODI. Modi ilmestyy Frosty Mod Managerin listaan.
  6. Frosty Mod Managerissa: rastita "Unbound Soundtrack", kytke muut musiikkimodit pois, paina Launch.

VAIN OFFLINE
  Modit ovat yksinpeliin. Ennen kuin menet Online-tilaan, käynnistä peli ilman modeja.
  Online-pelaaminen modien kanssa voi johtaa EA-tilisi sulkemiseen.

"WINDOWS SUOJASI TIETOKONETTASI" / "TUNTEMATON JULKAISIJA"
  Ohjelmaa ei ole allekirjoitettu maksullisella varmenteella, siksi Windows SmartScreen varoittaa siitä.
  Klikkaa "Lisätietoja" → "Suorita silti". Lähdekoodi on "source"-kansiossa.

MITÄ KANSIOSSA ON
  Unbound Soundtrack.exe   ohjelma
  app\web                  ikkuna (sivut, tekstit 14 kielellä)
  app\frosty               Frosty Toolsuiten kirjastot — lukevat pelin, kirjoittavat modin (GPL v3)
  app\encoder              EA:n äänienkooderi Frosty Editorin Sound Editor -lisäosasta
  app\yt-dlp.exe           lataa biisejä YouTubesta / YT Musicista (Unlicense)
  source\                  tämän ohjelman lähdekoodi (GPL v3)
  README\                  tämä teksti muilla kielillä
  data\                    syntyy ensimmäisellä käynnistyksellä: asetuksesi, biisit, SINUN pelistäsi luetut kuvat, loki
  Music\                   biisisi, kukin omassa numeroidussa kansiossaan: biisin tiedosto, kansikuva, song.json (nimi, GO, maali)
  THIRD-PARTY.txt          mitä käytämme, tekijät, lisenssit, linkit
  Mukana ei ole pelin eikä EA:n tiedostoja: kansikuvat ja taustat luetaan omasta pelistäsi.

JOS JOKIN EI TOIMI
  - Loki on data\log.txt (painike "Loki" ikkunassa).
  - Pelin päivityksen jälkeen työkalu lukee pelin uudelleen itse. Jos rakentaminen epäonnistuu viestillä
    "Peli on muuttunut" — etsi uudempaa versiota verkkosivulta.
  - Aloita alusta: sulje ohjelma ja poista "data"-kansio.
  - Ilmoita virheestä: https://forms.gle/StNa4GFZRHckFVpi8  (kerro, mitä teit, liitä data\log.txt-tiedoston loppu).

POISTO
  Kytke modi pois Frosty Mod Managerissa ja poista tämä kansio.

---------------------------------------------------------------------
Need for Speed ja NFS Unbound ovat Electronic Arts Inc. -yhtiön tavaramerkkejä.
Tämä työkalu ei ole Electronic Artsin tai Criterion Gamesin tekemä, hyväksymä tai tukema.
Tekijät ja lisenssit: THIRD-PARTY.txt ja ohjelman "Kiitokset"-painike.
