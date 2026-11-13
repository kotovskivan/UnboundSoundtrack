UNBOUND SOUNDTRACK  v1.0.1
Need for Speed Unbound oyununda kendi şarkıların — kendi kapaklarınla, yeni parçalar olarak.
Kitivski + Claude tarafından yapılmış ücretsiz bir hayran aracı. Electronic Arts veya Criterion Games ile bağlantılı DEĞİLDİR.
Web sitesi: https://kotovskivan.github.io/UnboundSoundtrack/

İngilizcesi — programın yanındaki README.txt.
Çeviriler yapay zekâ ile yapıldı (Claude).

=====================================================================
NE YAPAR
  - Oyunun parçalarını yarışlarda ve şehirde açıp kapat.
  - Kendi şarkılarını (mp3, flac, wav, ogg, m4a) kendi kapağınla YENİ parçalar olarak ekle.
    Oyunun parçaları olduğu gibi kalır.
  - YouTube / YT Music'ten şarkılar: bir şarkının, çalma listesinin ya da albümün linki, aynı anda birçok link ya da bir .txt listesi.
    Link bir müzik videosuysa program aynı şarkıyı YT Music'ten alır — videodaki girişler ve skeçler olmadan.
  - Her şarkı için yarış başlangıcını (17 sn ara sahne + 3,45 sn geri sayım, drop tam GO'da gelir) ve bitiş bölümünü ayarla.
  - Karttaki HIGH / LOW — parçanın ne sıklıkla çaldığı (oyun önce HIGH olanları seçer).
  - Profiller: farklı parça setleri, tek tıkla geçiş.
  - Frosty Mod Manager için TEK bir mod oluşturur. Oyun dosyaları değişmez.
  - Oyun parçasındaki ▶ — 30 saniyelik önizleme (Deezer, internet gerekir); YouTube ↗ — şarkının tamamı.
  - Pencere 13 dil biliyor.

GEREKENLER
  - Windows 10 ya da 11 (Microsoft Edge yerleşik gelir — pencere onu kullanır).
  - Need for Speed Unbound (Steam ya da EA app).
  - Frosty Mod Manager, HarGabt'ın NFS Unbound sürümü:
    https://github.com/HarGabt/FrostyToolsuite/releases
    Bu aracı kullanmadan önce onu bir kez başlat ve oyunu seç.
    İlk açılışta oyunun şifreleme anahtarını ister — NFS Unbound Encryption Key:
    https://www.nexusmods.com/needforspeedunbound/mods/97
    Launch'a ilk bastığında (ve ModData klasörü silindikten sonra) Frosty, oyun verilerine link oluşturmak için
    yönetici izni ister ("New installation detected") — bu normal, onayla.

NASIL KULLANILIR
  1. Zip arşivinin TAMAMINI normal bir klasöre çıkar (Masaüstü, Belgeler, D:\Mods…),
     Program Files klasörüne değil.
  2. "Unbound Soundtrack.exe" dosyasını başlat.
  3. Dili seç, koşulları kabul et. İlk açılışta program oyunu okur — birkaç dakika, sadece bir kez.
  4. + ŞARKI EKLE → bir dosya seç → GO'yu ve bitişi ayarla → Kaydet.
     Ya da ▶ YT MUSIC'TEN → bir link yapıştır (şarkı, çalma listesi, albüm) → İndir.
     Tüm şarkılar programın yanındaki "Music" klasöründe durur, her şarkı için numaralı bir klasör:
     Music\1\, Music\2\… — penceredeki sıra. + ŞARKI EKLE sıradaki klasörü kendisi oluşturur.
     Elle eklemek için: şarkı dosyası + kapak için bir resim (jpg, png) olan yeni bir numaralı klasör;
     GO ve bitiş otomatik ayarlanır. Klasörü silersen şarkı programdan kaybolur.
     Resim: klasörde tek resim varsa adı önemli değil; birkaç tane varsa cover.jpg kazanır. Boyutu fark etmez (oyun 256x256 kullanır).
  5. MOD OLUŞTUR. Mod, Frosty Mod Manager'ın listesinde görünür.
  6. Frosty Mod Manager'da: "Unbound Soundtrack" modunu işaretle, diğer müzik modlarını kapat, Launch'a bas.

SADECE ÇEVRİMDIŞI
  Modlar tek oyunculu oyun içindir. Online'a girmeden önce oyunu modsuz başlat.
  Modlarla Online oynamak EA hesabının yasaklanmasına yol açabilir.

"WINDOWS BİLGİSAYARINIZI KORUDU" / "BİLİNMEYEN YAYIMCI"
  Program ücretli bir sertifikayla imzalanmadığı için Windows SmartScreen uyarı verir.
  "Daha fazla bilgi" → "Yine de çalıştır" seçeneğine tıkla. Kaynak kodu "source" klasöründe.

KLASÖRDE NELER VAR
  Unbound Soundtrack.exe   program
  app\web                  pencere (sayfalar, 13 dilde metinler)
  app\frosty               Frosty Toolsuite kütüphaneleri — oyunu okur, modu yazar (GPL v3)
  app\encoder              Frosty Editor'ün Sound Editor eklentisinden EA ses kodlayıcısı
  app\yt-dlp.exe           YouTube / YT Music'ten şarkı indirir (Unlicense)
  source\                  bu programın kaynak kodu (GPL v3)
  README\                  bu metin başka dillerde
  data\                    ilk açılışta oluşur: ayarların, şarkıların, SENİN oyunundan okunan resimler, log
  Music\                   şarkıların, her biri numaralı bir klasörde: şarkı dosyası, kapak, song.json (ad, GO, bitiş)
  THIRD-PARTY.txt          neler kullanıldı, yazarlar, lisanslar, linkler
  Oyunun ya da EA'nın hiçbir dosyası dağıtılmaz: kapaklar ve arka planlar senin oyunundan okunur.

BİR ŞEY ÇALIŞMAZSA
  - Log: data\log.txt (penceredeki "Log" düğmesi).
  - Oyun güncellemesinden sonra: araç oyunu kendisi yeniden okur. Mod oluşturma
    "Oyun değişti" hatasıyla başarısız olursa — web sitesinde daha yeni bir sürüm ara.
  - Baştan başlamak için: programı kapat ve "data" klasörünü sil.
  - Hata bildir: https://forms.gle/StNa4GFZRHckFVpi8  (ne yaptığını yaz, data\log.txt dosyasının sonunu yapıştır).

KALDIRMA
  Modu Frosty Mod Manager'da kapat ve bu klasörü sil.

---------------------------------------------------------------------
Need for Speed ve NFS Unbound, Electronic Arts Inc. şirketinin ticari markalarıdır.
Bu araç Electronic Arts ya da Criterion Games tarafından yapılmadı, onaylanmadı ve desteklenmiyor.
Emeği geçenler ve lisanslar: THIRD-PARTY.txt ve programdaki "Yapımcılar" düğmesi.
