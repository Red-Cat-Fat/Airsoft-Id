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

PROJECT_DIR="$(cd "$(dirname "$0")/.." && pwd)"
FRONTEND_DIR="$PROJECT_DIR/frontend"

echo ""
echo -e "${CYAN}============================================${NC}"
echo -e "${CYAN}   Strikeball Social Network — Stopper     ${NC}"
echo -e "${CYAN}============================================${NC}"
echo ""

if lsof -ti tcp:4200 &>/dev/null 2>&1; then
  warn "Останавливаем frontend dev server (port 4200)..."
  kill "$(lsof -ti tcp:4200)" 2>/dev/null || true
  log "Frontend dev server остановлен."
else
  log "Frontend dev server не запущен."
fi

if [ -f "$PROJECT_DIR/docker-compose.yml" ]; then
  log "Останавливаем Docker-контейнеры..."
  docker compose -f "$PROJECT_DIR/docker-compose.yml" down
  log "Контейнеры остановлены."
else
  warn "docker-compose.yml не найден."
fi

echo ""
log "============================================"
log "  Проект полностью остановлен.             "
log "============================================"
echo ""
