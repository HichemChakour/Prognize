---
name: Prognize
description: Interface de planification qui disparaît derrière le planning — gris Apple, hairlines, un seul pigment (coral) qui ne dit que « agis ici ».
colors:
  canvas: "#f5f5f7"
  surface: "#ffffff"
  surface-2: "#fafafa"
  surface-hover: "rgba(0, 0, 0, 0.035)"
  surface-pressed: "rgba(0, 0, 0, 0.06)"
  glass: "rgba(246, 246, 248, 0.78)"
  separator: "rgba(0, 0, 0, 0.08)"
  separator-strong: "rgba(0, 0, 0, 0.14)"
  ink: "#1d1d1f"
  ink-2: "#6e6e73"
  ink-3: "#86868b"
  coral: "#ff7f50"
  coral-hover: "#ff6f3a"
  coral-mark: "#e8622f"
  coral-ink: "#c2410c"
  coral-tint: "rgba(255, 127, 80, 0.14)"
  coral-tint-strong: "rgba(255, 127, 80, 0.24)"
  on-coral: "#1d1d1f"
  red: "#d70015"
  red-tint: "rgba(215, 0, 21, 0.1)"
  green: "#147a44"
  green-tint: "rgba(20, 122, 68, 0.12)"
  amber: "#b25a00"
  amber-tint: "rgba(178, 90, 0, 0.12)"
  series-1: "#2a78d6"
  series-2: "#1baf7a"
  series-3: "#eda100"
  series-4: "#e87ba4"
  series-5: "#008300"
  series-6: "#4a3aa7"
  series-other: "#86868b"
typography:
  display:
    fontFamily: "-apple-system, BlinkMacSystemFont, Inter, Segoe UI, Roboto, Helvetica, Arial, sans-serif"
    fontSize: "2.6rem"
    fontWeight: 600
    lineHeight: 1
    letterSpacing: "-0.03em"
  headline:
    fontFamily: "-apple-system, BlinkMacSystemFont, Inter, Segoe UI, Roboto, Helvetica, Arial, sans-serif"
    fontSize: "1.75rem"
    fontWeight: 600
    lineHeight: 1.15
    letterSpacing: "-0.02em"
  title:
    fontFamily: "-apple-system, BlinkMacSystemFont, Inter, Segoe UI, Roboto, Helvetica, Arial, sans-serif"
    fontSize: "1.125rem"
    fontWeight: 600
    lineHeight: 1.15
    letterSpacing: "-0.012em"
  body:
    fontFamily: "-apple-system, BlinkMacSystemFont, Inter, Segoe UI, Roboto, Helvetica, Arial, sans-serif"
    fontSize: "15px"
    fontWeight: 400
    lineHeight: 1.47
    letterSpacing: "-0.008em"
  label:
    fontFamily: "-apple-system, BlinkMacSystemFont, Inter, Segoe UI, Roboto, Helvetica, Arial, sans-serif"
    fontSize: "0.85rem"
    fontWeight: 500
    letterSpacing: "normal"
  caption:
    fontFamily: "-apple-system, BlinkMacSystemFont, Inter, Segoe UI, Roboto, Helvetica, Arial, sans-serif"
    fontSize: "0.78rem"
    fontWeight: 400
    letterSpacing: "normal"
  micro:
    fontFamily: "-apple-system, BlinkMacSystemFont, Inter, Segoe UI, Roboto, Helvetica, Arial, sans-serif"
    fontSize: "0.7rem"
    fontWeight: 400
    letterSpacing: "normal"
rounded:
  inner: "6px"
  sm: "7px"
  md: "10px"
  lg: "14px"
  pill: "999px"
spacing:
  xs: "0.35rem"
  sm: "0.6rem"
  md: "1rem"
  lg: "1.5rem"
  xl: "1.75rem"
  2xl: "2rem"
  control-h: "34px"
components:
  button-primary:
    backgroundColor: "{colors.coral}"
    textColor: "{colors.on-coral}"
    rounded: "{rounded.sm}"
    padding: "0 0.95rem"
    height: "{spacing.control-h}"
  button-primary-hover:
    backgroundColor: "{colors.coral-hover}"
    textColor: "{colors.on-coral}"
  button-ghost:
    backgroundColor: "{colors.surface}"
    textColor: "{colors.ink}"
    rounded: "{rounded.sm}"
    padding: "0 0.95rem"
    height: "{spacing.control-h}"
  button-ghost-hover:
    backgroundColor: "{colors.surface-hover}"
    textColor: "{colors.ink}"
  button-plain:
    backgroundColor: "transparent"
    textColor: "{colors.coral-ink}"
    rounded: "{rounded.sm}"
    padding: "0 0.95rem"
    height: "{spacing.control-h}"
  button-plain-hover:
    backgroundColor: "{colors.coral-tint}"
    textColor: "{colors.coral-ink}"
  button-danger:
    backgroundColor: "transparent"
    textColor: "{colors.red}"
    rounded: "{rounded.sm}"
    padding: "0 0.95rem"
    height: "{spacing.control-h}"
  button-danger-hover:
    backgroundColor: "{colors.red-tint}"
    textColor: "{colors.red}"
  input:
    backgroundColor: "{colors.surface}"
    textColor: "{colors.ink}"
    rounded: "{rounded.sm}"
    padding: "0 0.7rem"
    height: "{spacing.control-h}"
  badge:
    backgroundColor: "{colors.surface-hover}"
    textColor: "{colors.ink-2}"
    rounded: "{rounded.pill}"
    padding: "0 0.6rem"
    height: "22px"
  badge-ok:
    backgroundColor: "{colors.green-tint}"
    textColor: "{colors.green}"
    rounded: "{rounded.pill}"
    padding: "0 0.6rem"
    height: "22px"
  tag:
    backgroundColor: "{colors.coral-tint}"
    textColor: "{colors.coral-ink}"
    rounded: "{rounded.inner}"
    padding: "0 0.55rem"
    height: "22px"
  panel:
    backgroundColor: "{colors.surface}"
    textColor: "{colors.ink}"
    rounded: "{rounded.lg}"
    padding: "1.5rem"
  nav-item:
    backgroundColor: "transparent"
    textColor: "{colors.ink}"
    rounded: "{rounded.sm}"
    padding: "0 0.6rem"
    height: "34px"
  nav-item-active:
    backgroundColor: "{colors.coral-tint}"
    textColor: "{colors.coral-ink}"
    rounded: "{rounded.sm}"
    padding: "0 0.6rem"
    height: "34px"
---

# Design System: Prognize

## Overview

**Creative North Star: "Le Calendrier qui s'efface"**

Prognize est un outil d'exploitation (mode Operate) pour un responsable planning sous pression : l'interface disparaît derrière le planning. Le monde visuel est celui d'Apple — Calendrier macOS, Réglages Système, apple.com — transposé au web : une toile gris très clair, des surfaces blanches délimitées par des hairlines à 8 % plutôt que par des bordures, des rayons continus, du verre dépoli uniquement sur le chrome (sidebar et topbar), et un sans-serif système (SF sur Apple, Inter auto-hébergé ailleurs) réglé avec un tracking légèrement négatif.

Il n'y a qu'un pigment : le coral. Il ne signale qu'une chose — « agis ici » — et il ne s'exprime jamais en texte clair sur un aplat : en aplat il porte de l'encre sombre, en trait il s'assombrit, en texte il devient brique. Tout le reste est gris. Le tableau de bord refuse la grille de cartes SaaS (cartes-icônes, halos, gradients) : trois compteurs en grande typographie posés sur la toile, un seul panneau bordé (la carte de chaleur), puis des sections séparées par des hairlines. Le calendrier ressemble à Calendrier macOS : blocs teintés sans barre latérale, hairlines, aujourd'hui marqué par une pastille.

Le mouvement est réduit à un moment : un blur-fade à l'arrivée d'une route. Les survols durent 180 ms. Rien de permanent, rien de cascadé ; `prefers-reduced-motion` coupe tout.

**Key Characteristics:**
- Gris Apple (toile `#f5f5f7`, surface blanche, encre `#1d1d1f`), thème sombre miroir (toile `#000`, surface `#1c1c1e`).
- Hairlines à 8 % (14 % pour les contours de contrôles) au lieu de bordures ; pas d'ombre au repos.
- Un seul accent, le coral, en trois états de contraste : aplat + encre sombre, trait `#e8622f`, texte `#c2410c`.
- Chiffres tabulaires partout où il y a des données ; grande typographie 2.6rem pour les compteurs, sans carte.
- Icônes SVG inline 24×24, trait 1.6, héritant de la couleur du texte ; aucun glyphe, aucune police d'icônes.
- Trois niveaux de profondeur : plat (hairline), flottant (feuille/tiroir), menu (popover).
- Un seul mouvement : `view-in` 400 ms / 6 px / blur 6 px, ease-out, au changement de route.

## Colors

Une gamme de gris Apple, un accent coral décliné par contraste plutôt que par teinte, trois couleurs d'état tintées, et une palette catégorielle de six couleurs réservée au calendrier.

### Primary
- **Coral** (`coral`, `#ff7f50`) : aplat des actions primaires (bouton `.btn--primary`, submit d'auth, logo, pastille « aujourd'hui » du calendrier). Toujours accompagné de l'encre sombre `on-coral` ; jamais de texte blanc dessus. Survol : `coral-hover` `#ff6f3a`.
- **Coral trait** (`coral-mark`, `#e8622f`) : la version pour les traits et les remplissages fins — icône de nav active, `accent-color` des cases à cocher, bordure des champs au focus, remplissage des jauges, cellules de la carte de chaleur (via `color-mix` avec `surface-hover`).
- **Coral encre** (`coral-ink`, `#c2410c`) : la version lisible en texte sur fond clair — liens, `.btn--plain`, libellé de nav active, `.tag`, le score du dernier planning (seul chiffre coloré du dashboard), jauges saturées (`is-high`).
- **Coral teinte** (`coral-tint` 14 %, `coral-tint-strong` 24 %) : fond de l'élément de nav actif, survol des boutons plain, fond des tags et des numéros d'étapes ; la version forte sert à `::selection`.
- En thème sombre, les trois variantes convergent vers `#ff8f66` (hover `#ffa07f`) ; `on-coral` reste `#1d1d1f`.

### Neutral
- **Toile** (`canvas`, `#f5f5f7`) : fond de l'application ; sombre `#000000`.
- **Surface** (`surface`, `#ffffff`) : panneaux, tables, calendrier, champs, boutons ghost ; sombre `#1c1c1e`. **Surface 2** (`#fafafa` / `#2c2c2e`) : avatar, fonds secondaires.
- **Survol / pression** (`surface-hover` 3.5 % noir, `surface-pressed` 6 %) : états de survol des lignes, nav, boutons ghost et icon-buttons ; fond des jauges vides, des capsules neutres, du contrôle segmenté et des placeholders de chargement. Sombre : 6 % / 10 % blanc.
- **Verre** (`glass`, `rgba(246,246,248,0.78)`) : sidebar et topbar uniquement, avec `backdrop-filter: saturate(180%) blur(20px)` ; sombre `rgba(28,28,30,0.72)`.
- **Séparateur** (`separator` 8 %, `separator-strong` 14 %) : la hairline universelle (bordures de panneaux, lignes de table, en-têtes, `form-actions`) ; la forte trace les contours des champs, des boutons ghost et des scrollbars. Sombre : 10 % / 18 %.
- **Encre** (`ink` `#1d1d1f`, `ink-2` `#6e6e73`, `ink-3` `#86868b`) : texte principal, texte secondaire (sous-titres, libellés de champs, en-têtes de table, méta), texte tertiaire (placeholders, heures, chevrons). Sombre : `#f5f5f7` / `#a1a1a6` / `#7c7c81`.

### Tertiary
- **États** : rouge `#d70015` (erreurs, conflits, actions destructrices), vert `#147a44` (succès, bon score, statut actif), ambre `#b25a00` (score moyen). Chacun a une teinte à 10–12 % pour les fonds de capsules et d'alertes ; le texte reste dans la couleur pleine. Sombre : `#ff6b6b` / `#4cd08a` / `#f0a64a`, teintes à 16 %.
- **Palette catégorielle du calendrier** (`series-1..6` : bleu `#2a78d6`, vert `#1baf7a`, jaune `#eda100`, rose `#e87ba4`, vert foncé `#008300`, indigo `#4a3aa7` ; `series-other` `#86868b`). Les blocs d'événement en prennent une teinte à 16 % sur la surface (24 % au survol), le titre à 70 % mixé avec l'encre, la puce de légende en plein. Thème sombre : variantes ajustées (`#3987e5`, `#199e70`, `#c98500`, `#d55181`, `#2fa02f`, `#9085e9`).

### Named Rules
**The One Pigment Rule.** Le coral est le seul accent de l'interface et il ne signifie qu'« agis ici » : actions primaires, élément de navigation actif, liens, focus, le score du planning. Le chrome (sidebar, topbar, en-têtes, tables) reste gris. Aucune autre couleur de marque, aucun gradient décoratif.

**The Coral Contrast Rule.** Le coral `#ff7f50` est réservé aux aplats et porte toujours l'encre sombre `#1d1d1f` ; il n'est jamais utilisé comme couleur de texte ni de trait fin. Les traits et icônes prennent `#e8622f` ; le texte prend `#c2410c`. Test : un texte coral sur fond clair qui n'est pas `#c2410c` est une erreur.

**The Kind-Color Rule.** Dans le calendrier, la couleur code la ressource d'un type choisi (classe, salle, enseignant, équipement), pas l'activité : les ressources du type actif sont triées par nom et reçoivent `series-1..6` dans cet ordre fixe ; au-delà de six, `series-other`. On ne cycle jamais la palette et on ne réattribue pas les couleurs entre types.

**The Tinted State Rule.** Un état (succès, erreur, statut, conflit) s'exprime par une capsule ou un fond teinté à 10–14 % portant du texte dans la couleur pleine — jamais par un aplat saturé ni par une barre latérale.

## Typography

**Display Font:** système Apple (`-apple-system`, `BlinkMacSystemFont`) avec Inter variable auto-hébergée en secours, puis Segoe UI, Roboto, Helvetica, Arial.
**Body Font:** identique — une seule famille pour toute l'application.
**Label/Mono Font:** aucune ; les données utilisent `font-variant-numeric: tabular-nums` dans la même famille.

**Character :** neutre, dense, légèrement resserrée. Le tracking négatif croît avec la taille (−0.008em corps, −0.02em titre de page, −0.03em compteurs) ; les graisses se limitent à 400 / 500 / 600 (700 seulement pour la lettre du logo et le badge de conflit). Les chiffres sont tabulaires dès qu'ils sont alignés.

### Hierarchy
- **Display** (600, 2.6rem, line-height 1, −0.03em) : les compteurs du tableau de bord (`.figure__value`), posés directement sur la toile ; 2rem sur mobile. L'unité qui suit (« /100 ») repasse à 1rem / 500 / `ink-2`.
- **Headline** (600, 1.75rem, 1.15, −0.02em) : `h1`, titre de page ; 1.45rem sur mobile. Suivi d'une phrase d'état en `ink-2` 0.95rem.
- **Title** (600, 1.125rem, 1.15, −0.012em) : `h2`, titres de sections et de panneaux. `h3` à 1rem / 600 pour les sous-sections.
- **Body** (400, 15px, 1.47, −0.008em) : le corps. Les contenus denses descendent à 0.9rem (tables, boutons, nav, titres de liste) et 0.88rem (alertes, items de menu).
- **Label** (500, 0.85rem) : libellés de champs, noms dans les jauges, nom d'utilisateur ; en `ink-2` pour les libellés de formulaire. Variante 0.78rem / 500 pour les en-têtes de table et les capsules.
- **Caption** (400, 0.78rem, `ink-2`) : méta, hints, sous-titres de section (0.8rem), erreurs de champ (0.8rem, rouge).
- **Micro** (400, 0.7rem, `ink-3`, tabulaire) : heures du calendrier et de la carte de chaleur, noms de jours (uppercase +0.04em, réservé à l'en-tête du calendrier).

### Named Rules
**The Tabular Data Rule.** Tout nombre susceptible d'être comparé verticalement ou de changer (compteurs, scores, heures, pourcentages, capacités) est en `tabular-nums`.

**The No-Eyebrow Rule.** Pas de sur-titre, kicker ni libellé en capitales au-dessus des titres. La seule exception est native au calendrier : les abréviations de jours en en-tête (0.7rem, uppercase, +0.04em), comme dans Calendrier macOS.

## Layout

Shell en grille à deux colonnes : sidebar de 240px (rail de 68px replié, transition 320 ms ease-out) et zone principale surmontée d'une topbar de 52px collante. Sidebar et topbar sont les seules surfaces en verre dépoli ; la page est une colonne centrée de 1200px max, avec un padding de `1.75rem 1.5rem 4rem` (1.25rem à 768–1023px, `1rem 0.9rem 3rem` sous 768px).

Le rythme spatial est en rem sur une échelle courte : 0.35rem entre libellé et champ, 0.6rem entre boutons, 1rem pour les gaps standard, 1.25–1.5rem pour le padding interne des panneaux et les marges de titres, 1.75rem sous l'en-tête de page et au-dessus des actions de formulaire, 2–3rem entre colonnes de sections. Hauteur de contrôle unique : 34px (28px en `--sm`, 26px dans le segmenté, 22px pour les capsules).

Le tableau de bord n'est pas une grille de cartes : les compteurs sont une rangée `auto-fit minmax(200px)` sur la toile fermée par une hairline ; la carte de chaleur est le seul `.panel` bordé ; les sections suivantes sont des colonnes `auto-fit minmax(300px)` séparées par des hairlines supérieures avec un gap horizontal de 3rem. Les formulaires utilisent `form-grid` `auto-fit minmax(240px)` avec gap `1.1rem 1.25rem`.

Le calendrier est une grille `56px repeat(7, 1fr)` ; avec panneau latéral ouvert, la vue devient `1fr 360px`. Sous 768px, le shell passe en une colonne, la sidebar devient un tiroir fixé (min(290px, 85vw), scrim noir 30 %), les panneaux latéraux deviennent des bottom sheets (85vh max, coins supérieurs `lg`), les actions de formulaire s'empilent en colonne inverse pleine largeur, et les actions de ligne sont toujours visibles (`hover: none`).

Points de rupture observés : 767px (mobile) et 768–1023px (tablette). Un seul seuil structurel.

## Elevation & Depth

Le système est plat par défaut : la profondeur au repos est exprimée par des hairlines (`separator` 8 %) et par le contraste toile / surface (`#f5f5f7` sur `#ffffff`), jamais par une ombre. Le verre dépoli du chrome (`glass` + `saturate(180%) blur(20px)`) suggère la superposition sans ombre. Les ombres n'apparaissent que pour ce qui se détache réellement du plan de la page, en deux tokens seulement.

### Shadow Vocabulary
- **Niveau 0 — Plat** (aucune ombre, `border: 1px solid var(--separator)`) : panneaux, tables, calendrier, champs, auth-card, sidebar et topbar. C'est l'état de 95 % de l'interface.
- **Niveau 1 — Flottant** (`box-shadow: 0 1px 2px rgba(0,0,0,0.04), 0 10px 30px rgba(0,0,0,0.08)`) : ce qui glisse par-dessus la page — tiroir de navigation mobile, bottom sheets des panneaux d'affectation et de résolution. Sombre : `0 1px 2px rgba(0,0,0,0.4), 0 10px 30px rgba(0,0,0,0.5)`.
- **Niveau 2 — Menu** (`box-shadow: 0 1px 2px rgba(0,0,0,0.06), 0 12px 40px rgba(0,0,0,0.14)`) : popovers ancrés — le menu utilisateur (rayon 12px, entrée `menu-in` 180 ms). Sombre : `0 1px 2px rgba(0,0,0,0.5), 0 12px 40px rgba(0,0,0,0.6)`.
- **Focus** (`box-shadow: 0 0 0 3.5px rgba(255,127,80,0.35)`) : anneau unique sur `:focus-visible` et sur les champs au focus (avec bordure `coral-mark`). Sombre : `rgba(255,143,102,0.4)`.
- **Micro-relief** (`0 1px 2px rgba(0,0,0,0.1)`) : uniquement l'onglet actif du contrôle segmenté, à la manière d'iOS.

### Named Rules
**The Three Planes Rule.** Trois plans et pas davantage : plat (hairline), flottant (`shadow-float`), menu (`shadow-menu`). Une ombre ne récompense pas le survol et ne décore pas une carte ; elle indique qu'un élément a quitté le plan de la page.

**The Glass Is Chrome Rule.** Le verre dépoli est réservé à la sidebar et à la topbar. Aucune carte, aucun panneau, aucune modale n'est translucide.

## Shapes

Rayons continus sur trois pas : 7px pour les contrôles (boutons, champs, éléments de nav, alertes, items de menu), 10px pour les surfaces intermédiaires et les panneaux sur mobile, 14px pour les panneaux, tables, calendrier et auth-card. À l'intérieur : 6px pour les blocs d'événement, les tags et les onglets segmentés ; 8px pour le conteneur segmenté et le logo ; 12px pour le popover de menu. Les capsules d'état, les chips de légende, le déclencheur du menu utilisateur et le badge de conflit sont des pilules (999px) ; avatars, pastille « aujourd'hui », numéros d'étapes et puces de légende sont des cercles.

Les contours sont des hairlines de 1px : `separator` pour délimiter, `separator-strong` pour les contrôles saisissables. Les blocs du calendrier n'ont pas de barre latérale colorée — la couleur est un aplat teinté à 16 %, et la sélection ajoute un contour de 1px + un anneau de 1px dans la couleur de série. Le conflit se dessine en hachures à 135° (bandes de 7px teintées / 2px rouge tint) avec bordure rouge. Les jauges sont des pistes de 6px (8px en `--lg`) arrondies à 3px ; les cellules de la carte de chaleur sont des rectangles 22px arrondis à 4px.

## Components

### Buttons
Discrets, denses, à hauteur fixe ; l'aplat coral est réservé à l'action primaire de l'écran.
- **Shape :** contrôle de 34px, coins doux (7px), padding `0 0.95rem`, texte 0.9rem / 500, icône 16px trait 1.7 ; `--sm` 28px / 0.82rem / icône 14px ; `--icon` carré 34px.
- **Primary :** aplat coral avec encre sombre, graisse 600 ; survol `coral-hover`.
- **Ghost :** surface blanche cerclée de `separator-strong` ; survol `surface-hover`.
- **Plain :** transparent, texte `coral-ink` ; survol `coral-tint`. **Danger :** transparent, texte rouge ; survol `red-tint`.
- **Hover / Focus :** transitions 180 ms `cubic-bezier(0.25,0.1,0.25,1)` ; pression `scale(0.985)` en 120 ms ; désactivé à 45 % d'opacité ; focus = anneau coral 3.5px.

### Chips
- **Capsule d'état (`.badge`) :** pilule 22px, fond `surface-hover`, texte `ink-2` 0.76rem / 500 ; `--ok` vert sur vert tint, `--off` `ink-3`.
- **Tag :** 22px, coins 6px, coral tint / `coral-ink`, 0.78rem / 500.
- **Chip de légende (calendrier) :** pilule 26px cerclée `separator`, puce 9px dans la couleur de série ; active = bordure et fond à 12 % dans la couleur ; muette à 45 %.
- **Contrôle segmenté :** conteneur 8px sur `surface-hover` avec 2px de padding ; onglet 26px / 6px / 0.8rem, l'actif passe en surface blanche avec le micro-relief.

### Cards / Containers
- **Corner Style :** 14px (`.panel`, `.table-wrap`, `.calendar`, `.auth-card`) ; 10px sous 768px.
- **Background :** `surface`. **Border :** hairline `separator`. **Shadow Strategy :** niveau 0, aucune ombre.
- **Internal Padding :** 1.5rem (1.1rem mobile) ; auth-card `2.25rem 2rem`, 400px max.
- **Sections sans carte :** sur le dashboard, les sections sous le pli sont séparées par une hairline supérieure et un padding vertical de 1.5rem, sans fond ni bordure.

### Inputs / Fields
- **Style :** 34px, surface blanche, hairline forte, 7px, texte 0.92rem, placeholder `ink-3` ; select avec chevron SVG 12px inline ; textarea 120px min ; checkbox 16px en `accent-color: coral-mark`.
- **Label :** au-dessus, 0.85rem / 500 / `ink-2`, gap 0.35rem ; hint 0.8rem `ink-3`.
- **Focus :** bordure `coral-mark` + anneau focus. **Error :** bordure rouge sur `.ng-invalid.ng-touched`, message 0.8rem rouge.
- **Actions :** `form-actions` alignées à droite, séparées par une hairline, marge 1.75rem.

### Navigation
- **Sidebar :** verre dépoli, 240px, padding `0.85rem 0.6rem` ; logo 28px coral (lettre 700), nom en 600 / −0.01em ; bas de sidebar : user-card + rangée d'icon-buttons séparée par une hairline. Rail 68px : libellés à opacité 0, items centrés.
- **Item :** 34px, 7px, 0.9rem / 500, icône 18px `ink-2` ; survol `surface-hover`, pression `surface-pressed` ; actif = fond `coral-tint`, texte `coral-ink`, icône `coral-mark`.
- **Topbar :** 52px collante en verre, titre 0.95rem / 600 / −0.01em, déclencheur utilisateur en pilule avec avatar 26px.
- **Mobile :** tiroir fixé avec `shadow-float` et scrim, fermeture au scrim ; nom et chevron du menu masqués.

### Icônes
Un seul système : chemins SVG 24×24, `stroke: currentColor`, trait 1.6, extrémités et jointures arrondies, dimensionnés en `1em` (18px dans la nav, 16px dans les boutons, 14px pour les chevrons). Aucun glyphe texte, aucune police d'icônes, aucune icône remplie.

### Compteurs (signature du tableau de bord)
Trois à quatre `.figure` sur la toile : valeur 2.6rem / 600 / −0.03em, libellé 0.9rem / 500 au-dessous, méta 0.78rem `ink-2`. Sans carte ni icône. Le seul compteur coloré est le score (`--accent`, valeur en `coral-ink`). Survol : le libellé passe en `coral-ink`.

### Calendrier (signature)
Surface blanche 14px, grille `56px repeat(7,1fr)`, hairlines `--hair` (= `separator`, 6 % blanc en sombre) pour colonnes et heures ; la colonne du jour est teintée coral à 3 % et son numéro est une pastille 26px coral / encre sombre. Bloc d'événement : coins 6px, padding `4px 7px`, fond `color-mix(var(--c) 16%, surface)`, titre 0.78rem / 600 en `color-mix(var(--c) 70%, ink)`, heure 0.7rem tabulaire, ressources 0.68rem ; survol 24 % ; sélection = bordure + anneau 1px dans `--c`. Trois densités selon la hauteur : normale (titre 2 lignes + ressources), `is-dense` < 64px (titre 1 ligne, sans ressources), `is-compact` < 40px (titre et heure sur une ligne). Chevauchements en lanes, séparés par un filet de 1px de surface (`is-laned`) ; éléments hors filtre à 22 % (`is-dimmed`) ; conflits hachurés.

### Jauges et carte de chaleur
Pistes 6px `surface-hover`, remplissage `coral-mark` (ou `coral-ink` en `is-high`) animé par `scaleX` en 320 ms ease-out ; valeur tabulaire alignée à droite. Carte de chaleur : cellules 22px / 4px, intensité = `color-mix(coral-mark v%, surface-hover)`, survol `scale(1.15)`.

### Mouvement
Un seul moment permanent dans le système : `view-in` (opacité 0 → 1, `translateY(6px)`, `blur(6px)` → 0) en 400 ms `cubic-bezier(0.16,1,0.3,1)` avec 40 ms de délai, appliqué aux enfants directs de `.page` à chaque changement de route. Micro-entrées locales : `menu-in` 180 ms pour le popover, `fade` 180 ms pour le scrim. Transitions d'état : 180 ms (`--t`) avec `cubic-bezier(0.25,0.1,0.25,1)` ; 320 ms (`--t-slow`) pour le repli de la sidebar, le tiroir mobile, les jauges et le changement de thème. `prefers-reduced-motion: reduce` ramène toutes les animations et transitions à 0.001 ms.

## Do's and Don'ts

### Do:
- **Do** délimiter par des hairlines (`separator` 8 %) et le contraste toile / surface ; réserver `separator-strong` (14 %) aux contours de contrôles.
- **Do** utiliser le coral selon son contraste : aplat `#ff7f50` + encre `#1d1d1f`, trait `#e8622f`, texte `#c2410c`.
- **Do** coder la couleur du calendrier par ressource du type actif, dans l'ordre fixe `series-1..6` puis `series-other`.
- **Do** poser les chiffres clés en grande typographie (2.6rem / 600 / −0.03em) directement sur la toile, avec `tabular-nums`.
- **Do** exprimer les états par capsules ou fonds teintés à 10–14 % portant du texte dans la couleur pleine.
- **Do** garder une hauteur de contrôle de 34px et des rayons 7 / 10 / 14px ; pilules uniquement pour les capsules et chips.
- **Do** utiliser les icônes SVG du système (`ICONS`, trait 1.6, `currentColor`), dimensionnées en `1em`.
- **Do** limiter le mouvement à `view-in` au changement de route et à des transitions de 180 ms ; respecter `prefers-reduced-motion`.

### Don't:
- **Don't** écrire du texte ou tracer des traits fins en `#ff7f50` ; ne jamais mettre de blanc sur le coral.
- **Don't** ajouter une ombre au repos à une carte, un panneau ou une table, ni au survol ; les seules ombres sont `shadow-float` (feuilles, tiroir) et `shadow-menu` (popovers).
- **Don't** appliquer le verre dépoli ailleurs que sur la sidebar et la topbar.
- **Don't** construire le tableau de bord en grille de cartes avec icônes, halos ou gradients ; sections séparées par des hairlines.
- **Don't** dessiner de barre latérale colorée sur les blocs du calendrier ; la couleur est un aplat teinté à 16 %.
- **Don't** cycler la palette catégorielle ni réattribuer les couleurs entre types de ressources.
- **Don't** introduire de kicker, sur-titre ou libellé en capitales au-dessus des titres ; l'uppercase est réservé aux abréviations de jours du calendrier.
- **Don't** introduire d'animation permanente, en cascade ou de plus de 400 ms.
