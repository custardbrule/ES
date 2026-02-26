DEV_ENV_FILE := configs/dev/.env
DEV_COMPOSE  := docker-compose -f docker-compose.dev.yml --env-file $(DEV_ENV_FILE)

.PHONY: dev-up dev-down dev-down-v dev-restart dev-logs dev-ps dev-build

dev-up:
	$(DEV_COMPOSE) up -d

dev-down:
	$(DEV_COMPOSE) down

dev-down-v:
	$(DEV_COMPOSE) down -v

dev-restart:
	$(DEV_COMPOSE) restart

dev-logs:
	$(DEV_COMPOSE) logs -f

dev-ps:
	$(DEV_COMPOSE) ps

dev-build:
	$(DEV_COMPOSE) build --no-cache
