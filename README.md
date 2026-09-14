# Prognize

**Planification et optimisation de ressources sous contraintes — SaaS B2B multi-tenant.**

Une organisation déclare ses **ressources** (salles, personnes, équipements…), ses **activités** à
planifier (cours, réunions, interventions…) et leurs **besoins** (une salle de 30 places, ce
professeur précis…). Prognize place les activités dans un planning hebdomadaire — à la main avec
détection de conflits en temps réel, ou automatiquement avec un solveur par contraintes (Google
OR-Tools CP-SAT).

Premier vertical : l'emploi du temps d'un établissement scolaire. Le modèle de données reste
volontairement **générique** (`Resource` / `Activity` / `Constraint`) pour s'appliquer à d'autres
domaines (hôpital, atelier, événementiel).

> Projet d'apprentissage et de portfolio : .NET 10 / EF Core / Angular 22, construit par tranches
> verticales avec des tests à chaque étape.

---

## Fonctionnalités

| Domaine | Ce qui est fait |
|---|---|
| **Multi-tenant** | Une colonne `organization_id` sur chaque table métier, filtrée par **EF Core global query filters** ; le tenant vient du JWT, jamais d'un paramètre client. Estampillage automatique à l'insertion, transfert entre tenants bloqué. Test de garde : toute entité `ITenantOwned` doit avoir un filtre. |
| **Auth** | ASP.NET Core Identity (allégé, 4 tables) + JWT. Inscription = création d'une organisation + son admin. Rôles `Admin` / `Member`. |
| **Ressources** | CRUD, type libre (`room`, `teacher`…), capacité, attributs `jsonb` libres, **disponibilités** hebdomadaires typées (`jsonb` via `OwnsMany().ToJson()`). |
| **Activités** | CRUD, durée, priorité 1-5, **besoins en ressources** typés (type + ressource imposée ou capacité minimale). Les références sont validées contre le tenant. |
| **Plannings** | Calendrier hebdomadaire, création manuelle d'affectations, **détection de conflits** (double réservation, indisponibilité, capacité, besoin non couvert, durée). Couleur par ressource, légende, mise en évidence. |
| **Solveur** | Bibliothèque `Prognize.Solver` (CP-SAT), **stateless** et sans dépendance à la base. Contraintes : heures d'ouverture, besoins, non-chevauchement, disponibilités. Objectif : maximiser les activités placées pondérées par priorité. Sa sortie est re-validée par le détecteur de conflits (0 conflit garanti par test). |
| **UI** | Angular 22 standalone, signals, zoneless, reactive forms. Thème clair / sombre / auto, responsive (sidebar → rail → drawer), dashboard avec carte de chaleur d'occupation et jauges par ressource. |

---

## Stack

| | |
|---|---|
| Backend | .NET 10 · ASP.NET Core Web API · EF Core 10 · PostgreSQL 18 (Npgsql) · Identity · JWT · Swagger |
| Solveur | `Prognize.Solver` · Google.OrTools 9.15 (CP-SAT) |
| Frontend | Angular 22 · TypeScript 6 · RxJS 7 · Vitest |
| Infra dev | Docker Compose (Postgres) · API et Angular lancés nativement |
| Tests | xUnit (unitaires + intégration sur vrai Postgres via `WebApplicationFactory`) · Vitest |

---

## Démarrage rapide

### Prérequis

- [.NET 10 SDK](https://dot.net) + `dotnet tool install --global dotnet-ef`
- Node 22 LTS + `npm install -g @angular/cli`
- Docker (avec Compose v2)
- `make` (facultatif mais recommandé)

### Première installation

```bash
make setup
```

Crée `.env` depuis `.env.example`, restaure les packages, démarre Postgres et applique les migrations.

### Lancer l'application

```bash
make dev
```

Démarre Postgres, l'API (avec rechargement à chaud) et Angular dans le même terminal. `Ctrl+C`
arrête tout.

| Service | URL |
|---|---|
| Application | http://localhost:4200 |
| API | http://localhost:5089 |
| Swagger | http://localhost:5089/swagger |
| PostgreSQL | `localhost:5432` (`prognize` / mot de passe du `.env`) |

Première utilisation : **Créer une organisation** depuis l'écran de connexion, puis suivre les trois
étapes du tableau de bord (ressources → activités → planning).

### Sans `make`

```bash
docker compose up -d
dotnet run --project src/Prognize.Api --launch-profile http
cd client && ng serve
```

---

## Commandes utiles

```
make help          liste des cibles
make dev           tout lancer
make api           API seule          make client        Angular seul
make test          tous les tests     make test-api      xUnit (Postgres requis)
make migrate       applique les migrations
make migration name=AddSomething      crée une migration
make db-shell      psql dans le conteneur
make db-reset      recrée la base (perte de données)
make format        dotnet format + prettier
```

---

## Architecture

```
Prognize/
├── src/
│   ├── Prognize.Api/            monolithe ASP.NET Core
│   │   ├── Domain/              entités EF (Organization, AppUser, Resource, Activity, Schedule…)
│   │   ├── Data/                AppDbContext (query filters, estampillage), configurations, migrations
│   │   ├── Common/Tenancy/      ITenantContext (lit le claim org_id du JWT)
│   │   └── Features/            une fonctionnalité = un dossier (controller + DTOs + logique)
│   │       ├── Auth/
│   │       ├── Resources/
│   │       ├── Activities/
│   │       └── Schedules/       ConflictDetector, SolverBridge, controllers
│   └── Prognize.Solver/         CP-SAT — ne référence QUE Google.OrTools
├── tests/Prognize.Tests/        xUnit
├── client/                      Angular
│   └── src/app/
│       ├── core/                services HTTP, auth, thème, viewport
│       ├── features/            pages (dashboard, resources, activities, schedules)
│       └── shared/              layout (Shell), directives
├── docker-compose.yml           PostgreSQL
└── Makefile
```

### Choix structurants

- **Monolithe bien rangé, pas de Clean Architecture.** Deux projets backend seulement : l'API et le
  solveur. Le solveur est séparé physiquement pour que le compilateur garantisse qu'il ne touche
  jamais à la base.
- **Le multi-tenant est invisible pour les controllers.** Aucun endpoint ne reçoit ni ne renseigne
  d'`OrganizationId` : le `DbContext` filtre en lecture et estampille en écriture. Les seules
  exceptions explicites (`IgnoreQueryFilters`) sont le login et l'inscription.
- **JSON typé plutôt que tables de jointure** pour les besoins et les disponibilités
  (`OwnsMany().ToJson()`) : lisibles en SQL, typés en C#, sans `JOIN`.
- **Heures de planning en heure murale** (`timestamp without time zone`), instants d'audit en
  `DateTimeOffset`. Un cours « lundi 8h » ne dépend pas du fuseau du serveur.
- **Conflits calculés à la lecture**, jamais stockés — toujours exacts.
- **Le solveur ne bloque pas** : une activité impossible est laissée non placée (variable `placed`
  maximisée pondérée par la priorité) au lieu de rendre le problème infaisable.

---

## API

Toutes les routes sont sous `/api`, authentifiées par `Authorization: Bearer <jwt>` sauf
`auth/register` et `auth/login`. Les écritures exigent le rôle `Admin`.

| Ressource | Routes |
|---|---|
| Auth | `POST auth/register` · `POST auth/login` · `GET auth/me` |
| Ressources | `GET/POST resources` · `GET/PUT/DELETE resources/{id}` · `?kind=` |
| Activités | `GET/POST activities` · `GET/PUT/DELETE activities/{id}` |
| Plannings | `GET/POST schedules` · `GET/PUT/DELETE schedules/{id}` · **`POST schedules/{id}/solve`** |
| Affectations | `POST schedules/{id}/assignments` · `PUT/DELETE schedules/{id}/assignments/{aid}` |

Les erreurs suivent [RFC 7807](https://www.rfc-editor.org/rfc/rfc7807) (`ProblemDetails`).
Documentation interactive sur `/swagger` en développement.

---

## Tests

```bash
make test
```

- **`MultiTenancyTests`** — isolation entre deux organisations, sur un vrai Postgres (base jetable).
- **`TenantModelGuardTests`** — chaque entité `ITenantOwned` a un query filter (échec de build si oubli).
- **`ConflictDetectorTests`** — 12 cas, sans base.
- **`CpSatSchedulerTests`** — le solveur respecte disponibilités, capacités, priorités, non-chevauchement.
- **`*ApiTests`** — intégration bout en bout via `WebApplicationFactory` ; dont
  `Le_solveur_place_tout_sans_aucun_conflit` : la sortie du solveur est vérifiée par le détecteur.

Les tests d'intégration créent une base `prognize_test_<guid>` par classe de test et la suppriment.

---

## Configuration

| Clé | Où | Note |
|---|---|---|
| `POSTGRES_*` | `.env` | lus par `docker-compose.yml` |
| `ConnectionStrings:Default` | `appsettings.Development.json` | doit correspondre au `.env` |
| `Jwt:Key` | `appsettings.Development.json` | **dev uniquement** — en production, variable d'environnement `Jwt__Key` |
| `Cors:AllowedOrigins` | `appsettings.Development.json` | `http://localhost:4200` |
| `apiUrl` | `client/src/environments/` | `http://localhost:5089/api` en dev |

---

## Limites connues / pistes

- Le solveur **remplace** les affectations du planning (pas de « garde les manuelles et complète »).
- Pas de récurrence (« 3 fois par semaine » = 3 activités).
- Pas de préférences douces (« plutôt le matin »).
- Le calcul est synchrone (limite de temps configurable) — un job en arrière-plan serait la version
  production.
- Le token JWT est en `localStorage` ; un cookie `httpOnly` serait plus sûr.

---

## Licence

Projet personnel — tous droits réservés.
