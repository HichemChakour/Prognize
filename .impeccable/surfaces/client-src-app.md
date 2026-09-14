---
version: 1
slug: "client-src-app"
primary_target: "client/src/app"
related_targets: ["client/src/styles.scss"]
---

# Surface brief — application Prognize (toutes les routes authentifiées + auth)

Scope : le shell (sidebar, topbar), le dashboard, les listes et formulaires ressources/activités, les plannings (liste, calendrier, panneaux), login/inscription. Mode : **Operate** (l'auth est aussi Operate : une tâche, pas une vente).

Audience : responsable planning, grand écran, sous pression. Tâche : voir l'état du planning, corriger, générer. États importants : conflit, non placé, chargement, vide. Contrainte pinée par l'utilisateur : langage visuel Apple, coral unique accent, mouvement discret.

## Direction contract

seed: 274f82db (canon — référence pinée par l'utilisateur : Apple. Barre : Calendrier macOS, Réglages Système, apple.com)

THESIS : l'interface disparaît derrière le planning. Refus du dashboard SaaS « cartes + halos + gradients » et de tout chrome coloré : le seul pigment est le coral, et il ne dit qu'une chose — « agis ici ».

OWN-WORLD : gris Apple (#f5f5f7 fond, blanc surface, #1d1d1f encre, #6e6e73 secondaire), séparateurs hairline à 8 % au lieu de bordures, rayons 10–12 px continus, verre dépoli (saturate 180 % + blur 20 px) sur sidebar et topbar uniquement, système SF/Inter avec échelle 1,125 et tracking −0,01 à −0,02 sur les titres, chiffres tabulaires dans les données, capsules teintées pour les états, icônes SVG trait 1,6. Coral #FF7F50 en aplat avec encre sombre ; #c2410c en texte.

STORY : « c'est calme, je vois tout, je sais où cliquer ». Le calendrier ressemble à Calendrier macOS : blocs teintés sans bordure gauche, hairlines, aujourd'hui marqué par une pastille.

FIRST VIEWPORT : dashboard — un titre, une phrase d'état, trois compteurs plus le score du dernier planning en typographie large sans cartes-icônes (le score est le seul chiffre en coral : principe produit n°1, l'état du planning est toujours visible), la carte de chaleur comme premier objet visuel ; sous le pli, des sections séparées par des hairlines, pas de grille de cartes.

SIGNATURE : un seul mouvement — blur-fade 400 ms / 6 px au changement de route, ease-out. Survols 150–200 ms. Rien de permanent, rien de cascadé.

RISK : le coral en aplat exige de l'encre sombre (contraste), ce qui s'éloigne du bouton blanc-sur-bleu d'Apple ; assumé. Le sans-serif système diffère entre macOS et Windows ; Inter chargé en secours pour une rendition proche de SF.

Non résolu : export/impression du planning (hors scope).
