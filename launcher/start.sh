#!/usr/bin/env bash
set -euo pipefail

RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
NC='\033[0m'

log()  { echo -e "${GREEN}[✓]${NC} $1"; }
warn() { echo -e "${YELLOW}[!]${NC} $1"; }
err()  { echo -e "${RED}[✗]${NC} $1"; }
info() { echo -e "${CYAN}[i]${NC} $1"; }

PROJECT_DIR="$(cd "$(dirname "$0")/.." && pwd)"
ENV_FILE="$PROJECT_DIR/.env"
ENV_EXAMPLE="$PROJECT_DIR/.env.example"

echo ""
info "============================================"
info "   Strikeball Social Network — Launcher    "
info "============================================"
echo ""

if ! command -v docker &>/dev/null; then
  err "Docker не установлен. Установи Docker Desktop: https://docker.com"
  exit 1
fi

if ! docker info &>/dev/null; then
  err "Docker daemon не запущен. Запусти Docker Desktop."
  exit 1
fi

if [ ! -f "$ENV_FILE" ]; then
  warn ".env файл не найден. Создаём новый."

  if [ -f "$ENV_EXAMPLE" ]; then
    cp "$ENV_EXAMPLE" "$ENV_FILE"
    log "Скопирован .env.example -> .env"
  fi

  random_password() { openssl rand -base64 32 2>/dev/null || python3 -c "import secrets; print(secrets.token_urlsafe(32))" 2>/dev/null || echo "CHANGE_ME_STRONG_PASSWORD"; }

  ask() {
    local prompt="$1" default="$2" var_name="$3"
    read -rp "$(echo -e "${CYAN}?${NC} $prompt [${default}]: ")" input
    eval "$var_name='${input:-$default}'"
  }

  ask "HTTP port" "80" HTTP_PORT
  ask "HTTPS port" "443" HTTPS_PORT
  ask "API port" "5000" API_PORT
  ask "PostgreSQL port" "5432" POSTGRES_PORT
  ask "Redis port" "6379" REDIS_PORT
  ask "Database name" "strikeballdb" POSTGRES_DB
  ask "Database user" "postgres" POSTGRES_USER

  POSTGRES_PASSWORD=$(random_password)
  REDIS_PASSWORD=$(random_password)
  JWT_SECRET_KEY=$(random_password)
  BACKUP_RETENTION_DAYS=30

  info "Сгенерированы безопасные пароли для PostgreSQL, Redis и JWT."

  cat > "$ENV_FILE" <<EOF
HTTP_PORT=$HTTP_PORT
HTTPS_PORT=$HTTPS_PORT
API_PORT=$API_PORT
POSTGRES_PORT=$POSTGRES_PORT
REDIS_PORT=$REDIS_PORT
POSTGRES_DB=$POSTGRES_DB
POSTGRES_USER=$POSTGRES_USER
POSTGRES_PASSWORD=$POSTGRES_PASSWORD
REDIS_PASSWORD=$REDIS_PASSWORD
JWT_SECRET_KEY=$JWT_SECRET_KEY
BACKUP_RETENTION_DAYS=$BACKUP_RETENTION_DAYS
EOF

  log ".env создан с указанными параметрами."
else
  log ".env найден, загружаем конфигурацию."
fi

source <(grep -v '^\s*#' "$ENV_FILE" | grep -v '^\s*$')

log "Запуск Docker-контейнеров..."
docker compose -f "$PROJECT_DIR/docker-compose.yml" up --build -d

info "Ожидание готовности сервисов..."
WAIT_SECONDS=60
for i in $(seq 1 $WAIT_SECONDS); do
  STATUS=$(docker inspect --format='{{.State.Health.Status}}' strikeball-postgres 2>/dev/null || echo "starting")
  if [ "$STATUS" = "healthy" ]; then
    log "PostgreSQL готов."
    break
  fi
  if [ "$i" -eq "$WAIT_SECONDS" ]; then
    warn "PostgreSQL не стал healthy за ${WAIT_SECONDS}с, пробуем миграцию вслепую."
  fi
  sleep 1
done

info "База данных создаётся автоматически при первом запросе к API (EnsureCreatedAsync)."

echo ""
log "============================================"
log "  Проект запущен!"
log "============================================"
echo ""
info "  Backend API:  http://localhost:${API_PORT:-5000}/swagger"
info "  Frontend:     http://localhost:${HTTP_PORT:-80}"
info "  PostgreSQL:   localhost:${POSTGRES_PORT:-5432}"
info "  Redis:        localhost:${REDIS_PORT:-6379}"
echo ""
info "Для запуска frontend dev server с hot-reload:"
info "  cd frontend && npm run start"
echo ""
info "Для остановки: ./launcher/stop.sh"
echo ""
