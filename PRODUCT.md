# Product

<!-- impeccable:product-schema 1 -->

## Platform

web

## Users

Primary : le **responsable du planning d'un établissement scolaire** (directeur des études, adjoint, secrétaire pédagogique). Il construit l'emploi du temps avant la rentrée et le corrige toute l'année : un professeur absent, une salle en travaux, une classe dédoublée. Il travaille sur ordinateur, souvent sur deux écrans, sous pression de temps, et ses erreurs sont visibles par tout l'établissement le lundi matin.

Secondaire (confirmé, non prioritaire) : les enseignants et membres de l'équipe en lecture seule (rôle `Member`) qui consultent le planning.

Le produit est conçu générique (ressources / activités / contraintes) pour d'autres secteurs — hôpital, atelier, événementiel — mais aucun de ces verticaux n'est adressé aujourd'hui.

## Product Purpose

Prognize transforme une liste de ressources (salles, personnes, équipements), d'activités à placer (cours, réunions) et de contraintes (disponibilités, capacités, besoins) en un planning hebdomadaire **sans conflit**. Deux modes : placement manuel avec détection de conflits en temps réel, ou génération automatique par solveur de contraintes (Google OR-Tools CP-SAT).

Succès : le responsable obtient un planning valide en minutes au lieu de jours, voit immédiatement ce qui coince, et peut le retoucher à la main sans casser ce qui marche.

## Positioning

Le solveur est **stateless et vérifié** : sa sortie est re-validée par le détecteur de conflits (test automatisé, 0 conflit garanti). Une activité impossible est laissée non placée et signalée, jamais forcée — le planning proposé est toujours cohérent. Le modèle de données est générique par construction (pas de notion d'école dans le backbone), ce qu'un logiciel d'emploi du temps scolaire classique ne peut pas prétendre.

## Operating Context

- Application web, desktop d'abord (le travail de planification se fait sur grand écran), consultable sur mobile.
- Flux : déclarer les ressources → déclarer les activités et leurs besoins → créer un planning → placer à la main ou lancer le solveur → corriger les conflits → publier.
- Un planning couvre une semaine réelle ; les heures sont des heures murales locales de l'établissement.
- Multi-tenant : chaque organisation ne voit que ses données ; le tenant vient du jeton d'authentification.
- Rôles : `Admin` (édite, lance le solveur) et `Member` (lecture).

## Capabilities and Constraints

- Ressources : type libre (`room`, `teacher`, `class`, `equipment`…), capacité optionnelle, attributs JSON libres, fenêtres de disponibilité hebdomadaires.
- Activités : durée (minutes), priorité 1–5, besoins typés (type de ressource, ressource imposée ou capacité minimale), statut actif/inactif.
- Plannings : affectations (activité + créneau + ressources), conflits calculés à la lecture (double réservation, indisponibilité, capacité insuffisante, besoin non couvert, durée incohérente), score et métriques du solveur.
- Le solveur **remplace** les affectations du planning ; il n'y a pas encore de « garder les manuelles et compléter ».
- Pas de récurrence (« 3 fois par semaine » = 3 activités), pas de préférences douces, pas de notifications, pas d'export.
- Terminologie : *ressource*, *activité*, *besoin*, *affectation*, *planning*, *conflit*, *générer*.
- Stack : Angular 22 (standalone, signals, zoneless, SCSS, reactive forms), ASP.NET Core 10, PostgreSQL. Pas de Tailwind, pas de bibliothèque de composants.

## Brand Commitments

- Nom : **Prognize**. Logo : lettre « P » dans une tuile ; pas d'autre asset.
- Direction visuelle **contraignante fixée par l'utilisateur** : langage Apple — neutres (blancs, gris chauds, noir profond), **coral `#FF7F50` comme unique couleur d'action**, typographie et espace comme structure, mouvement discret (150–300 ms, pas d'animation permanente). Les anciennes couleurs teal et sand ne sont plus des engagements.
- Voix : sobre, précise, française, vouvoiement ; pas d'exclamation, pas de jargon solveur dans l'interface (« Générer le planning », pas « Résoudre le CSP »).

## Evidence on Hand

- Le code et les tests : 55 tests xUnit dont un test bout en bout prouvant que la sortie du solveur a zéro conflit.
- Aucun client, témoignage, chiffre d'usage, logo partenaire ou benchmark. **Ne pas en inventer.**
- Données de démonstration possibles uniquement via l'inscription d'une organisation fictive dans l'app.

## Product Principles

1. **Ne jamais mentir sur l'état du planning** : un conflit est toujours visible, une activité non placée toujours nommée.
2. **L'outil recule, le planning avance** : la semaine est l'objet principal ; le chrome, les panneaux et le décor lui cèdent la place.
3. **Corriger doit être aussi facile que générer** : le manuel et l'automatique sont deux gestes sur le même calendrier.
4. **Générique par construction, spécialisé par le vocabulaire** : les types de ressources sont ceux de l'organisation, jamais imposés.
5. **Lisible sous pression** : hiérarchie claire, contrastes vérifiés, pas d'information portée par la couleur seule.

## Accessibility & Inclusion

- Contrastes AA vérifiés (texte ≥ 4,5:1, marques ≥ 3:1) en clair et en sombre.
- `prefers-reduced-motion` respecté : toutes les animations coupées.
- Statuts et conflits toujours accompagnés d'un libellé ou d'un motif, jamais couleur seule.
- Navigation clavier : focus visible, `aria-current`, `aria-expanded` sur les menus.
