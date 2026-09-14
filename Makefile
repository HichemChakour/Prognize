# ---------------------------------------------------------------------------
# Prognize — commandes de développement
#   make help        liste les cibles
#   make dev         Postgres + API + Angular (tout en un, Ctrl+C arrête tout)
# ---------------------------------------------------------------------------

SHELL := /bin/bash
.DEFAULT_GOAL := help

API_PROJECT := src/Prognize.Api
SOLUTION    := Prognize.slnx
CLIENT_DIR  := client
DB_CONTAINER := prognize-db

# nvm n'est pas chargé dans un shell non interactif : on le source si présent.
NVM := export NVM_DIR="$$HOME/.nvm"; [ -s "$$NVM_DIR/nvm.sh" ] && . "$$NVM_DIR/nvm.sh" && nvm use default >/dev/null;
# dotnet installé via dotnet-install.sh : ajouter au PATH si besoin.
DOTNET := export DOTNET_ROOT="$$HOME/.dotnet"; export PATH="$$DOTNET_ROOT:$$DOTNET_ROOT/tools:$$PATH";

.PHONY: help setup dev db db-stop db-logs db-shell db-reset api api-watch client build test test-api test-client \
        migrate migration lint format clean

help: ## Affiche cette aide
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) | awk 'BEGIN {FS = ":.*?## "}; {printf "  \033[36m%-14s\033[0m %s\n", $$1, $$2}'

# ---------------------------------------------------------------- setup
setup: ## Première installation : .env, packages, base migrée
	@[ -f .env ] || (cp .env.example .env && echo "→ .env créé, pense à changer POSTGRES_PASSWORD")
	$(DOTNET) dotnet restore $(SOLUTION)
	$(NVM) cd $(CLIENT_DIR) && npm install
	$(MAKE) db
	$(MAKE) migrate
	@echo "✓ Prêt. Lance : make dev"

# ---------------------------------------------------------------- docker / db
db: ## Démarre PostgreSQL (docker compose) et attend qu'il soit prêt
	docker compose up -d
	@for i in $$(seq 1 30); do \
	  s=$$(docker inspect --format '{{.State.Health.Status}}' $(DB_CONTAINER) 2>/dev/null); \
	  [ "$$s" = "healthy" ] && echo "✓ Postgres prêt" && exit 0; sleep 1; \
	done; echo "✗ Postgres ne répond pas (docker logs $(DB_CONTAINER))"; exit 1

db-stop: ## Arrête PostgreSQL (les données sont conservées)
	docker compose down

db-logs: ## Logs PostgreSQL
	docker compose logs -f db

db-shell: ## Ouvre psql dans le conteneur
	docker exec -it $(DB_CONTAINER) psql -U prognize -d prognize

db-reset: ## Supprime le volume Postgres et recrée la base (PERTE DE DONNÉES)
	docker compose down -v
	$(MAKE) db
	$(MAKE) migrate

# ---------------------------------------------------------------- backend
api: db ## Lance l'API .NET (http://localhost:5089, Swagger sur /swagger)
	$(DOTNET) dotnet run --project $(API_PROJECT) --launch-profile http

api-watch: db ## Lance l'API avec rechargement à chaud
	$(DOTNET) dotnet watch --project $(API_PROJECT) run --launch-profile http

migrate: ## Applique les migrations EF Core
	$(DOTNET) dotnet ef database update --project $(API_PROJECT)

migration: ## Crée une migration : make migration name=AddSomething
	@[ -n "$(name)" ] || (echo "usage: make migration name=NomDeLaMigration"; exit 1)
	$(DOTNET) dotnet ef migrations add $(name) --project $(API_PROJECT) --output-dir Data/Migrations

# ---------------------------------------------------------------- frontend
client: ## Lance Angular (http://localhost:4200)
	$(NVM) cd $(CLIENT_DIR) && ng serve

# ---------------------------------------------------------------- tout en un
dev: db ## Postgres + API + Angular en parallèle (Ctrl+C arrête tout)
	@trap 'kill 0' INT TERM EXIT; \
	( $(DOTNET) dotnet watch --project $(API_PROJECT) run --launch-profile http 2>&1 | sed 's/^/[api]    /' ) & \
	( $(NVM) cd $(CLIENT_DIR) && ng serve 2>&1 | sed 's/^/[client] /' ) & \
	wait

# ---------------------------------------------------------------- qualité
build: ## Build backend + frontend (prod)
	$(DOTNET) dotnet build $(SOLUTION) -c Release
	$(NVM) cd $(CLIENT_DIR) && ng build

test: test-api test-client ## Tous les tests

test-api: db ## Tests xUnit (nécessite Postgres)
	$(DOTNET) dotnet test $(SOLUTION)

test-client: ## Tests Angular (vitest)
	$(NVM) cd $(CLIENT_DIR) && ng test --watch=false

format: ## Formate le code (dotnet format + prettier)
	$(DOTNET) dotnet format $(SOLUTION)
	$(NVM) cd $(CLIENT_DIR) && npx prettier --write "src/**/*.{ts,html,scss}"

clean: ## Supprime bin/obj/dist/node_modules
	find . -type d \( -name bin -o -name obj \) -not -path "*/node_modules/*" -exec rm -rf {} + 2>/dev/null || true
	rm -rf $(CLIENT_DIR)/dist $(CLIENT_DIR)/.angular
