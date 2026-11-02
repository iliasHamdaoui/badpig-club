# Brief de design — site du club BadPig (Badminton Pignan)

## 1. Ce que je te demande
Concevoir l'UX et le design complet d'un site de club de badminton, **mobile first** et responsive.
Livre un système de design réutilisable **puis** les maquettes des écrans listés au point 6.
Je n'ai pas besoin de code de production : je l'implémenterai moi-même en Angular. J'ai besoin de
maquettes, de spécifications précises et de valeurs exactes (couleurs, tailles, espacements).

## 2. Contexte technique à respecter
- Angular 22, composants standalone avec templates et CSS **inline**, plus une feuille globale
  `styles.css` qui porte les variables CSS et les classes partagées.
- **Aucun framework CSS ni bibliothèque de composants** (pas de Tailwind, Bootstrap, Material).
  CSS natif uniquement : variables, flexbox, grid, container queries si utile.
- **Aucune ressource tierce chargée par le navigateur** (RGPD) : pas de CDN, pas de Google Fonts
  en lien distant, pas d'iframe Google Maps. La police Poppins sera **auto-hébergée** (fichiers
  woff2 servis par le site) : limite-toi à 2 graisses, 400 et 700. Pour la carte, propose une image
  statique ou un lien vers OpenStreetMap.
- Textes de l'interface **en français**.
- Pas de mode sombre dans cette version, mais structure les variables pour qu'il soit ajoutable.
- Largeurs de référence : **360 px**, 768 px, 1280 px. Le téléphone est le cas principal :
  la majorité des adhérents consultera le site depuis le gymnase ou le canapé.

## 3. Tout doit passer par des variables CSS
Exigence centrale : **je veux pouvoir changer les couleurs du site plus tard en modifiant quelques
lignes**, sans relire les styles de cent composants.

- **Aucune couleur écrite en dur** ailleurs que dans le bloc `:root` de la feuille globale : pas un
  seul code hexadécimal, `rgb()` ou nom de couleur dans le CSS d'un composant, d'un bouton, d'un
  badge ou d'une maquette. Partout ailleurs, uniquement `var(--nom-du-jeton)`.
- **Deux niveaux de jetons**, à livrer séparément :
  1. les **couleurs de la palette**, nommées par ce qu'elles sont
     (par exemple `--primary-base`, `--primary-tint`, `--primary-shade`, `--grey-base`…) ;
  2. les **rôles**, nommés par ce à quoi ils servent et définis à partir du niveau 1
     (par exemple `--color-text`, `--color-text-muted`, `--color-background`, `--color-surface`,
     `--color-border`, `--button-primary-background`, `--button-primary-text`,
     `--badge-success-background`, `--focus-ring`…).
  Les composants n'utilisent **que** les jetons de rôle. Ainsi, changer le rouge du club ne touche
  que le niveau 1, et changer l'usage d'une couleur ne touche que le niveau 2.
- Donne la **liste complète des jetons de rôle** dont le site a besoin, avec pour chacun sa valeur,
  son rôle en une phrase et où il s'applique. Si un écran a besoin d'une couleur qui n'est pas dans
  la liste, ajoute le jeton plutôt qu'une valeur en dur.
- **Dérivation** : si tu peux, fais dériver les teintes claires et les nuances foncées de la couleur
  de base avec `color-mix()` ou la syntaxe de couleur relative, pour qu'un seul changement suffise à
  recolorer toute la famille. Donne alors **aussi** les valeurs calculées, pour que je puisse les
  figer si le résultat ne me plaît pas.
- Applique la même règle aux autres décisions : espacements, tailles de texte, rayons, ombres,
  durées d'animation et largeurs de rupture passent aussi par des variables nommées.
- Montre en fin de livraison un **exemple de changement de couleur** : les lignes exactes à modifier
  pour passer le site du rouge à une autre couleur principale, et ce que cela change à l'écran.

## 4. Fondations CSS imposées
Je te donne ci-dessous les conventions à reprendre. Elles viennent d'une feuille de style que
j'aime bien ; garde la **méthode et les échelles**, adapte les valeurs au mobile first.

**Base de calcul** : `html { font-size: 62.5% }` donc 1rem = 10px. **Donne toutes tes valeurs en rem
sur cette base** (1.6rem = 16px). Tout le CSS du site sera réécrit, donc cette convention peut
s'appliquer partout.

**Échelle typographique de référence** (desktop) : h1 62px / h2 48px / h3 40px / h4 32px / h5 24px /
p 20px / a 16px / small 12px. Graisses : titres 700, texte 400. Interlignes : titres 1.2, texte 1.5.
Interlettrage : titres resserrés.
À adapter : ces tailles sont faites pour le desktop. Donne une **échelle mobile** distincte
(ou des `clamp()`), avec un texte courant lisible sur téléphone, et exprime l'interlettrage des
titres en **em** et non en px, pour qu'il reste correct à toutes les tailles.

**Palette** : structure en triplets **base / tint (clair) / shade (foncé)**.
La couleur **principale reste le rouge du club** : base `#c0302b`, nuance foncée actuelle `#8f1f1b`,
teinte très claire actuelle `#fdf1f0` (le fond du site aujourd'hui). Complète ou corrige ce triplet
pour qu'il soit cohérent.
Propose en plus :
- une **couleur secondaire** et une **tertiaire** qui s'accordent avec ce rouge (pour les accents,
  les badges, les graphiques), chacune en triplet base/tint/shade ;
- une échelle de **gris** : base, teinte claire, nuance foncée, blanc, noir ;
- des couleurs **sémantiques** : succès, avertissement, erreur, information.
  Attention : la couleur principale étant rouge, l'erreur ne peut pas être un rouge voisin.
  Dis comment tu distingues « bouton principal » et « message d'erreur ».

**Rayons de bordure** : 3 valeurs, petite / moyenne / grande (références : 4px / 8px / 20px).

**Échelle d'espacement** : une seule échelle, en variables CSS, du plus petit au plus grand
(références : 5, 10, 15, 20, 25, 30, 40, 50, 60, 70, 80, 90, 100, 125, 150, 200, 250, 300, 400, 500 px).
Le rythme vertical entre grandes sections de la page d'accueil est très ample sur desktop
(200 à 300 px) : donne la valeur **mobile** correspondante et comment elle grandit aux paliers.

**Classes utilitaires attendues**, dans cet esprit :
- `.container` : largeur maximale ~1200px, largeur 90 %, centrée ;
- `.btn` et ses variantes : plein principal, contour principal, plein secondaire, plus le danger ;
- une classe de mise en évidence (`.highlight`) déclinée par couleur, pour les badges ;
- `.small-text` pour les mentions ;
- des tailles de logo nommées.
Tu peux proposer d'autres classes, dis lesquelles sont globales et lesquelles restent locales.

**Réinitialisation** : `* { box-sizing: border-box; margin: 0; padding: 0 }`.
Conséquence à traiter : les annonces du club contiennent du **texte riche saisi par le bureau**
(titres, paragraphes, listes à puces, gras, liens). Prévois une classe de contenu
(par exemple `.rich-text`) qui **rétablit** puces, numéros et espacements des paragraphes dans ce
contexte précis.

## 5. Identité visuelle existante
- Logo existant : un cochon ("BadPig" = badminton + Pignan), fichier `assets/logo.png`,
  utilisé aujourd'hui à 44 px de haut.
- Ton souhaité : club amateur convivial et familial, mais site net et moderne, pas enfantin.

## 6. Les trois types de visiteurs
Le même site sert trois publics, et la navigation change selon le cas :
1. **Visiteur non connecté** : accueil public, annonces publiques, bouton « Se connecter ».
2. **Adhérent à jour de cotisation** : en plus, espace membres, interclubs, boutique, son profil.
3. **Membre du bureau / administrateur** : en plus, toute l'administration.
La connexion se fait par un bouton qui redirige vers un fournisseur d'identité externe (pas de
formulaire e-mail/mot de passe à concevoir). Il n'y a pas d'inscription libre : on devient adhérent
sur invitation.

## 7. Écrans à concevoir, par priorité

### Priorité 1 — la page d'accueil, dans ses deux états
**Visiteur non connecté** : c'est la vitrine du club. Elle doit contenir
- un en-tête visuel (photo ou aplat graphique) avec le nom du club et une phrase d'accroche ;
- **où nous jouons** : nom et adresse du gymnase, plan d'accès ou lien de carte ;
- **les séances** : les jours et les horaires, et la fermeture pendant les vacances scolaires ;
- **comment nous rejoindre** : la démarche et la période d'inscription, puis le contact ;
- **contact** : l'adresse e-mail du club ;
- les **annonces publiques** récentes (2 ou 3, avec affiche) ;
- un appel à l'action « Je suis adhérent » vers la connexion.
**N'affiche aucun tarif ni montant de cotisation** : ce n'est pas public pour le moment, et ne
réserve pas de bloc pour ça.
**Contenu réel à utiliser tel quel** (ne l'invente pas, ne le complète pas) :

> **Où nous jouons**
> Collège Marie Curie — Gymnase, Halle aux sports du Collège
> Rue du Collège
> 34570 Pignan
>
> **Les séances**
> Mardi 20h – 23h
> Mercredi 20h – 23h
> Vendredi 20h – 23h
> Fermé pendant les vacances scolaires.
>
> **Contact**
> contact@badpig.fr

Les trois séances sont identiques et ouvertes aux adhérents : il n'y a pas de créneau distinct par
niveau ou par âge, ne découpe donc pas cette section par public.
Le club n'a **ni numéro de téléphone ni réseau social** à afficher pour l'instant : n'en invente pas
et ne réserve pas de place pour eux, mais dis-moi où ils s'inséreraient le jour où ils existeront.
La phrase d'accroche, le texte de présentation du club et la démarche d'inscription sont à écrire par
toi : propose-les, je les corrigerai.

**Adhérent connecté** : la même page devient un tableau de bord. Dans l'ordre d'importance
- les **annonces** (publiques et réservées aux adhérents), les épinglées d'abord ;
- les **prochaines rencontres interclubs**, avec la mise en avant de celles de ses propres équipes ;
- les **sondages ou propositions de dates qui attendent sa réponse** (action en attente) ;
- des raccourcis vers l'espace membres, la boutique, son profil ;
- un rappel discret si sa cotisation n'est pas validée.
Conçois l'état « rien à afficher » pour chaque bloc.

Données réellement disponibles, n'en invente pas d'autres :
- annonce : titre, résumé, affiche (image), date de publication, badges « épinglée », « réservée aux
  adhérents », « sondage », nombre de pièces jointes, nombre de « j'aime », auteur ;
- rencontre interclubs : nom de l'équipe (D6, D5, D4, D1, PR), date, heure (parfois inconnue), club
  adverse, à domicile ou à l'extérieur, lieu, et le score en matchs gagnés (7 matchs par rencontre)
  quand elle est jouée.

### Priorité 2 — l'ossature commune
- **En-tête et navigation** : jusqu'à 7 entrées selon le rôle (Accueil, Annonces, Espace membres,
  Interclubs, Boutique, Mon profil, Administration) plus le nom de la personne et la déconnexion.
  Propose un motif mobile clair (menu déroulant, panneau latéral ou barre basse) et dis lequel tu
  recommandes et pourquoi. Prévois la place de l'état « non connecté ».
- **Pied de page** : contact, mentions, lien vers les annonces.
- **Carte / section** : le conteneur de base de toutes les pages.
- **États** : chargement, liste vide, erreur de chargement, message de succès, et **bouton en cours
  d'action** (aujourd'hui un petit cercle tournant dans le bouton, à conserver ou remplacer).
- Spécifie les boutons pour les deux cas du site : les vrais `<button>` et les liens `<a>` qui
  ressemblent à des boutons, avec survol, focus clavier, actif, désactivé et occupé.

### Priorité 3 — les motifs qui reviennent partout
- **Liste paginée** : toutes les listes du site ont une recherche, un tri par colonne et une
  pagination « Précédent / Suivant · Page 2 sur 5 · 42 résultats ». Conçois ce trio, et surtout sa
  version téléphone (le tri par en-tête de colonne ne marche pas à 360 px : propose une solution).
- **Tableau dense → cartes sur mobile** : la page d'administration des utilisateurs affiche par
  personne le nom, l'e-mail, le titre au bureau, le statut du compte, l'état de l'adhésion et **3 à 5
  boutons d'action**. C'est le pire cas du site : montre comment cette ligne devient lisible et
  utilisable sur un téléphone.
- **Formulaires** : champs, libellés, aide, erreurs de validation (l'API renvoie des erreurs par
  champ), boutons Enregistrer / Annuler. Montre un formulaire long (créer une annonce : titre, texte
  riche, affiche, pièces jointes, visibilité, date d'expiration, sondage).
- **Badges et statuts** : adhésion à jour / en tolérance / à valider, compte actif ou désactivé,
  annonce brouillon / terminée, victoire / défaite.

### Priorité 4 — écrans métier
- **Liste des annonces** et **détail d'une annonce** (texte riche, affiche, pièces jointes,
  « j'aime », commentaires avec réponses citées, sondage avec résultats).
- **Interclubs — calendrier des rencontres** : liste filtrable par équipe et par période
  (à venir / passées), avec le score.
- **Interclubs — détail d'une rencontre** : l'équipe, l'adversaire, la date, le lieu, la composition
  (7 matchs : SD, MX1, MX2, DD, DH, SH1, SH2 ; simples à 1 joueur, doubles à 2) et le résultat.
- **Interclubs — saisie du résultat par le capitaine** : c'est le deuxième cas difficile. Pour chacun
  des 7 matchs, il faut saisir jusqu'à 3 sets × 2 scores, soit jusqu'à 42 petits champs numériques,
  et pour chaque match un déroulement « joué / forfait / abandon » avec le camp concerné. Aujourd'hui
  c'est un tableau. Trouve un motif utilisable au bord du terrain sur un téléphone, une main occupée
  (par exemple un match à la fois, avec progression).
- **Interclubs — disponibilités** : le capitaine propose 2 à 10 dates, chaque joueur de l'équipe
  répond « disponible / peut-être / indisponible ». Conçois la grille de réponses, côté joueur
  (répondre vite) et côté capitaine (voir qui peut, choisir la date).
- **Interclubs — équipes et effectif** : liste des 5 équipes avec leur capitaine ; page d'effectif
  d'une équipe où l'on ajoute des joueurs depuis une liste paginée d'adhérents.
- **Boutique** : catalogue d'articles avec image, variantes (taille, couleur), stock, panier,
  paiement, puis « mes commandes » et le détail d'une commande.
- **Espace membres** : page d'accueil des adhérents (aujourd'hui presque vide, propose ce qu'elle
  devrait contenir).
- **Mon profil** : coordonnées, état de l'adhésion, suppression de son compte.
- **Administration** : accueil du bureau avec ses 9 rubriques, aujourd'hui une simple ligne de liens
  séparés par des points. Propose une vraie page d'entrée.

## 8. Accessibilité et qualité, exigences fermes
- Contrastes conformes AA, y compris le rouge sur blanc, le blanc sur rouge et les badges.
  **Vérifie chaque couple texte/fond et donne le rapport de contraste** ; si une couleur de ma
  palette ne passe pas derrière du texte, corrige-la et dis-moi la nouvelle valeur.
- Zones tactiles de 44 px minimum, champs numériques avec le bon clavier mobile.
- Focus clavier visible partout, ordre de tabulation logique, anneau de focus défini par un jeton.
- Aucune information portée par la couleur seule (victoire/défaite, statuts) : c'est aussi ce qui
  permettra de changer les couleurs sans rendre le site incompréhensible.
- Le site doit rester lisible à 200 % de zoom et fonctionner sans JavaScript de mise en page.
- Pas d'animation gratuite ; respecte `prefers-reduced-motion`.
- Deux détails de la feuille de référence à **ne pas** reprendre tels quels, car ils cassent le
  retour à la ligne du texte : `a { display: inline-block }` et `span { display: inline-block }`.
  Réserve-les aux cas où c'est utile (boutons, badges) au lieu de les appliquer globalement.

## 9. Ce que j'attends comme livrables
1. **Le système de design** : les deux niveaux de jetons du point 3 (palette puis rôles), l'échelle
   typographique mobile et desktop, l'échelle d'espacement, les rayons, les ombres, les interlignes
   et les états (normal, survol, focus, actif, désactivé, occupé). Livre-le sous forme d'un bloc
   `:root` complet, directement reprenable dans `styles.css`, avec les classes utilitaires du point 4.
2. **Les maquettes** des écrans du point 7 aux trois largeurs, en commençant par 360 px, sans aucune
   couleur en dur dans leur CSS.
3. **Les spécifications d'interaction** des points difficiles : navigation mobile, tableau dense,
   saisie du résultat, grille de disponibilités.
4. **La répartition** entre ce qui va dans la feuille globale et ce qui reste propre à un composant.
5. **L'exemple de changement de couleur** demandé au point 3.
6. Les **écarts** que tu proposes par rapport aux fondations du point 4, avec la raison.

## 10. À ne pas faire
- N'écris aucune couleur en dur en dehors du bloc `:root`.
- N'invente pas de fonctionnalité ni de donnée qui n'existe pas dans ce brief.
- N'affiche aucun tarif.
- N'ajoute pas de dépendance, de police distante ni d'icônes d'une bibliothèque externe
  (propose des icônes SVG inline si tu en as besoin).
- Ne pars pas d'un gabarit de tableau de bord SaaS générique : c'est un site de club.
- Ne conçois pas de formulaire de connexion ni d'inscription.
