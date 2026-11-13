UNBOUND SOUNDTRACK  v1.0.1
Tes propres chansons dans Need for Speed Unbound — sous forme de nouveaux titres, avec tes propres pochettes.
Outil gratuit fait par des fans : Kitivski + Claude. SANS lien avec Electronic Arts ou Criterion Games.
Site : https://kotovskivan.github.io/UnboundSoundtrack/

English — README.txt à côté du programme.
Traductions faites par IA (Claude).

=====================================================================
CE QU'IL FAIT
  - Active/désactive les titres du jeu séparément pour les courses et pour la ville.
  - Ajoute tes propres chansons (mp3, flac, wav, ogg, m4a) comme NOUVEAUX titres avec ta propre pochette.
    Les titres du jeu restent tels quels.
  - Chansons de YouTube / YT Music : lien d'une chanson, playlist ou album, plusieurs liens d'un coup ou une liste .txt.
    Si le lien est un clip, le programme prend la même chanson sur YT Music — sans les intros ni les sketchs du clip.
  - Pour chaque chanson, règle le départ (17 s de cinématique + 3,45 s de compte à rebours, le drop tombe sur le GO)
    et le passage d'arrivée.
  - HIGH / LOW sur la carte — à quelle fréquence un titre passe (le jeu prend d'abord les HIGH).
  - Profils : différentes sélections de titres, on change en un clic.
  - Crée UN seul mod pour Frosty Mod Manager. Les fichiers du jeu ne sont pas modifiés.
  - ▶ sur un titre du jeu — extrait de 30 secondes (Deezer, Internet nécessaire) ; YouTube ↗ — la chanson en entier.
  - La fenêtre parle 13 langues.

CE QU'IL TE FAUT
  - Windows 10 ou 11 (Microsoft Edge est intégré — la fenêtre l'utilise).
  - Need for Speed Unbound (Steam ou EA app).
  - Frosty Mod Manager, la version de HarGabt pour NFS Unbound :
    https://github.com/HarGabt/FrostyToolsuite/releases
    Lance-le une fois et choisis le jeu avant d'utiliser cet outil.
    Au premier lancement, il demande la clé de chiffrement du jeu — NFS Unbound Encryption Key :
    https://www.nexusmods.com/needforspeedunbound/mods/97
    À son premier Launch (et après la suppression du dossier ModData), Frosty demande les droits d'administrateur
    pour créer des liens vers les données du jeu ("New installation detected") — c'est normal, accepte.

COMMENT L'UTILISER
  1. Décompresse TOUT le zip dans un dossier normal (Bureau, Documents, D:\Mods…),
     pas dans Program Files.
  2. Lance "Unbound Soundtrack.exe".
  3. Choisis la langue, accepte les conditions. Le premier lancement lit le jeu — quelques minutes, une seule fois.
  4. + AJOUTER UNE CHANSON → choisis un fichier → règle le GO et l'arrivée → Enregistrer.
     Ou ▶ DEPUIS YT MUSIC → colle un lien (chanson, playlist, album) → Télécharger.
     Toutes les chansons sont dans le dossier "Music" à côté du programme, un dossier numéroté par chanson :
     Music\1\, Music\2\… — l'ordre dans la fenêtre. + AJOUTER UNE CHANSON crée lui-même le dossier suivant.
     À la main : un nouveau dossier numéroté avec le fichier de la chanson + une image (jpg, png) pour la pochette ;
     le GO et l'arrivée se règlent automatiquement. Supprime un dossier — la chanson disparaît du programme.
     Image : nom libre si c'est la seule du dossier ; plusieurs — cover.jpg l'emporte. Taille libre (le jeu utilise 256x256).
  5. CRÉER LE MOD. Le mod apparaît dans la liste de Frosty Mod Manager.
  6. Dans Frosty Mod Manager : coche "Unbound Soundtrack", désactive les autres mods de musique, appuie sur Launch.

HORS LIGNE UNIQUEMENT
  Les mods sont faits pour le jeu solo. Avant de jouer en ligne, lance le jeu sans mods.
  Jouer en ligne avec des mods peut faire bannir ton compte EA.

"WINDOWS A PROTÉGÉ VOTRE ORDINATEUR" / "ÉDITEUR INCONNU"
  Le programme n'est pas signé avec un certificat payant, donc Windows SmartScreen affiche un avertissement.
  Clique sur "Informations complémentaires" → "Exécuter quand même". Le code source est dans le dossier "source".

CONTENU DU DOSSIER
  Unbound Soundtrack.exe   le programme
  app\web                  la fenêtre (pages, textes en 13 langues)
  app\frosty               bibliothèques Frosty Toolsuite — lisent le jeu, écrivent le mod (GPL v3)
  app\encoder              encodeur audio EA, tiré du plugin Sound Editor de Frosty Editor
  app\yt-dlp.exe           télécharge les chansons depuis YouTube / YT Music (Unlicense)
  source\                  code source de ce programme (GPL v3)
  README\                  ce texte dans d'autres langues
  data\                    créé au premier lancement : tes réglages, chansons, images lues dans TON jeu, log
  Music\                   tes chansons, chacune dans un dossier numéroté : fichier, pochette, song.json (nom, GO, arrivée)
  THIRD-PARTY.txt          ce que nous utilisons, auteurs, licences, liens
  Aucun fichier du jeu ni d'EA n'est fourni : les pochettes et les fonds sont lus dans ton propre jeu.

SI QUELQUE CHOSE NE MARCHE PAS
  - Le log est dans data\log.txt (bouton "Log" dans la fenêtre).
  - Après une mise à jour du jeu : l'outil relit le jeu tout seul. Si la création du mod échoue avec
    "Le jeu a changé" — cherche une version plus récente sur le site.
  - Pour repartir de zéro : ferme le programme et supprime le dossier "data".
  - Signaler un bug : https://forms.gle/StNa4GFZRHckFVpi8  (décris ce que tu as fait, colle la fin de data\log.txt).

SUPPRIMER
  Désactive le mod dans Frosty Mod Manager et supprime le dossier du programme.

---------------------------------------------------------------------
Need for Speed et NFS Unbound sont des marques commerciales d'Electronic Arts Inc.
Cet outil n'a été ni créé, ni approuvé, ni soutenu par Electronic Arts ou Criterion Games.
Crédits et licences : THIRD-PARTY.txt et le bouton "Crédits" dans le programme.
