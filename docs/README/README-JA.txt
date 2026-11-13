UNBOUND SOUNDTRACK  v1.0.1
Need for Speed Unbound に自分の曲を — 自分のジャケット付きの新しい曲として。
Kitivski + Claude による無料のファンメイドツールです。Electronic Arts および Criterion Games とは一切関係ありません。
ウェブサイト：https://kotovskivan.github.io/UnboundSoundtrack/

英語版 — プログラムと同じフォルダーにある README.txt。
翻訳は AI（Claude）によるものです。

=====================================================================
できること
  - ゲームの曲を、レースと街それぞれでオン／オフできます。
  - 自分の曲（mp3、flac、wav、ogg、m4a）を、自分のジャケット付きの新しい曲として追加できます。
    ゲームの曲はそのまま残ります。
  - YouTube / YT Music の曲：曲・プレイリスト・アルバムのリンク、複数のリンクをまとめて、または .txt のリスト。
    リンクが MV の場合は、同じ曲を YT Music から取得します — MV のイントロや寸劇は入りません。
  - 曲ごとに、レースのスタート（17 秒のカットシーン + 3.45 秒のカウントダウン、GO の瞬間にドロップ）とフィニッシュのパートを設定できます。
  - カードの HIGH / LOW — 曲が流れる頻度です（ゲームは HIGH を優先します）。
  - プロファイル：曲のセットを使い分けて、ワンクリックで切り替えられます。
  - Frosty Mod Manager 用の MOD を 1 つだけ作成します。ゲームのファイルは変更しません。
  - ゲームの曲の ▶ で 30 秒試聴（Deezer、インターネット接続が必要）、YouTube ↗ で曲をフルで聴けます。
  - ウィンドウは 14 言語に対応しています。

必要なもの
  - Windows 10 または 11（Microsoft Edge が標準で入っています — ウィンドウの表示に使います）。
  - Need for Speed Unbound（Steam または EA app）。
  - Frosty Mod Manager の NFS Unbound 対応版（HarGabt 版）：
    https://github.com/HarGabt/FrostyToolsuite/releases
    このツールを使う前に、一度起動してゲームを選んでおいてください。
    初回起動時にゲームの暗号化キーを求められます — NFS Unbound Encryption Key：
    https://www.nexusmods.com/needforspeedunbound/mods/97
    最初に Launch するとき（と ModData フォルダーを削除した後）、Frosty はゲームデータへのリンクを作るため
    管理者権限を求めます（「New installation detected」）— これは正常なので、許可してください。

使い方
  1. zip を必ず丸ごと、普通のフォルダー（デスクトップ、ドキュメント、D:\Mods… など）に展開してください。
     Program Files の中には展開しないでください。
  2. 「Unbound Soundtrack.exe」を起動します。
  3. 言語を選び、利用条件に同意します。初回起動時はゲームを読み込みます — 数分かかりますが、最初の一回だけです。
  4. + 曲を追加 → ファイルを選ぶ → GO とフィニッシュを設定 → 保存。
     または ▶ YT Music から → リンクを貼り付け（曲、プレイリスト、アルバム）→ ダウンロード。
     曲はすべて、プログラムの隣にある「Music」フォルダーに入ります。1 曲につき番号付きのフォルダーが 1 つ：
     Music\1\、Music\2\… — ウィンドウでの並び順です。「+ 曲を追加」は次のフォルダーを自動で作ります。
     手動で追加する場合：新しい番号付きフォルダーに、曲のファイル + ジャケット用の画像（jpg、png）を入れます。
     GO とフィニッシュは自動で設定されます。フォルダーを削除すると、その曲はプログラムから消えます。
     画像：フォルダーに 1 枚だけなら名前は自由。複数ある場合は cover.jpg が優先されます。サイズも自由です（ゲームでは 256x256 で使われます）。
  5. 「MOD を作成」をクリックします。MOD が Frosty Mod Manager のリストに表示されます。
  6. Frosty Mod Manager で「Unbound Soundtrack」にチェックを入れ、ほかの音楽 MOD をオフにして、Launch を押します。

オフライン専用
  MOD はシングルプレイ専用です。オンラインで遊ぶ前に、MOD なしでゲームを起動してください。
  MOD を入れたままオンラインで遊ぶと、EA アカウントが BAN される可能性があります。

「WindowsによってPCが保護されました」／「不明な発行元」
  このプログラムは有料の証明書で署名されていないため、Windows SmartScreen が警告を表示します。
  「詳細情報」→「実行」をクリックしてください。ソースコードは「source」フォルダーにあります。

フォルダーの中身
  Unbound Soundtrack.exe   プログラム本体
  app\web                  ウィンドウ（ページ、14 言語のテキスト）
  app\frosty               Frosty Toolsuite のライブラリ — ゲームの読み込み、MOD の書き出し（GPL v3）
  app\encoder              EA オーディオエンコーダー（Frosty Editor の Sound Editor プラグインから）
  app\yt-dlp.exe           YouTube / YT Music から曲をダウンロード（Unlicense）
  source\                  このプログラムのソースコード（GPL v3）
  README\                  この説明の他言語版
  data\                    初回起動時に作成：設定、曲、お使いのゲームから読み込んだ画像、ログ
  Music\                   自分の曲。1 曲ごとに番号付きフォルダー：曲のファイル、ジャケット、song.json（曲名、GO、フィニッシュ）
  THIRD-PARTY.txt          使用しているもの、作者、ライセンス、リンク
  ゲームや EA のファイルは一切同梱していません。ジャケットや背景は、お使いのゲームから読み込みます。

うまく動かないとき
  - ログは data\log.txt です（ウィンドウの「ログ」ボタン）。
  - ゲームのアップデート後：ツールが自動でゲームを読み込み直します。MOD の作成が
    「ゲームが変わりました」で失敗する場合は、ウェブサイトで新しいバージョンを探してください。
  - 最初からやり直すには：プログラムを閉じて「data」フォルダーを削除します。
  - バグの報告：https://forms.gle/StNa4GFZRHckFVpi8  （何をしたかを書いて、data\log.txt の最後の部分を貼り付けてください）。

アンインストール
  Frosty Mod Manager で MOD をオフにして、このフォルダーを削除します。

---------------------------------------------------------------------
Need for Speed と NFS Unbound は Electronic Arts Inc. の商標です。
このツールは Electronic Arts および Criterion Games が制作・承認・サポートしたものではありません。
クレジットとライセンス：THIRD-PARTY.txt と、プログラム内の「クレジット」ボタン。
