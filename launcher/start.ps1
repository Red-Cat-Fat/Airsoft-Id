$Host.UI.RawUI.WindowTitle = "Strikeball Social Network — Launcher"

function Write-Log   { Write-Host "[✓] $args" -ForegroundColor Green }
function Write-Warn  { Write-Host "[!] $args" -ForegroundColor Yellow }
function Write-Err   { Write-Host "[✗] $args" -ForegroundColor Red }
function Write-Info  { Write-Host "[i] $args" -ForegroundColor Cyan }

Write-Host ""
Write-Info "============================================"
Write-Info "   Strikeball Social Network — Launcher    "
Write-Info "============================================"
Write-Host ""

$ProjectDir = Split-Path -Path (Split-Path -Path $PSScriptRoot -Parent) -Parent
$EnvFile = Join-Path $ProjectDir ".env"
$EnvExample = Join-Path $ProjectDir ".env.example"

# --- Check Docker ---
if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
    Write-Err "Docker не установлен. Установи Docker Desktop: https://docker.com"
    exit 1
}

try {
    $null = docker info 2>&1 | Out-Null
} catch {
    Write-Err "Docker daemon не запущен. Запусти Docker Desktop."
    exit 1
}

# --- .env setup ---
if (-not (Test-Path $EnvFile)) {
    Write-Warn ".env файл не найден. Создаём новый."

    if (Test-Path $EnvExample) {
        Copy-Item $EnvExample $EnvFile
        Write-Log "Скопирован .env.example -> .env"
    }

    function Random-Password {
        try {
            $bytes = [byte[]]::new(32)
            [Security.Cryptography.RandomNumberGenerator]::Fill($bytes)
            return [Convert]::ToBase64String($bytes)
        } catch {
            return "CHANGE_ME_STRONG_PASSWORD"
        }
    }

    function Ask-Value {
        param([string]$Prompt, [string]$Default)
        $input = Read-Host "$Prompt [$Default]"
        if ([string]::IsNullOrWhiteSpace($input)) { return $Default }
        return $input
    }

    $HTTP_PORT  = Ask-Value "HTTP port" "80"
    $HTTPS_PORT = Ask-Value "HTTPS port" "443"
    $API_PORT   = Ask-Value "API port" "5000"
    $POSTGRES_PORT = Ask-Value "PostgreSQL port" "5432"
    $REDIS_PORT    = Ask-Value "Redis port" "6379"
    $POSTGRES_DB   = Ask-Value "Database name" "strikeballdb"
    $POSTGRES_USER = Ask-Value "Database user" "postgres"

    $POSTGRES_PASSWORD = Random-Password
    $REDIS_PASSWORD    = Random-Password
    $JWT_SECRET_KEY    = Random-Password
    $BACKUP_RETENTION_DAYS = 30

    Write-Info "Сгенерированы безопасные пароли для PostgreSQL, Redis и JWT."

    @"
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
"@ | Set-Content -Path $EnvFile

    Write-Log ".env создан с указанными параметрами."
} else {
    Write-Log ".env найден, загружаем конфигурацию."
}

# --- Docker Compose Up ---
Write-Log "Запуск Docker-контейнеров..."
docker compose -f "$ProjectDir/docker-compose.yml" up --build -d

Write-Info "Ожидание готовности PostgreSQL..."
$Timeout = 60
$Timer = 0
do {
    $Status = docker inspect --format='{{.State.Health.Status}}' strikeball-postgres 2>$null
    if ($Status -eq "healthy") {
        Write-Log "PostgreSQL готов."
        break
    }
    Start-Sleep -Seconds 1
    $Timer++
} while ($Timer -lt $Timeout)
if ($Timer -eq $Timeout) {
    Write-Warn "PostgreSQL не стал healthy за ${Timeout}с, пробуем миграцию вслепую."
}

# --- Database ---
Write-Info "База данных создаётся автоматически при первом запросе к API (EnsureCreatedAsync)."

Write-Host ""
Write-Log "============================================"
Write-Log "  Проект запущен!"
Write-Log "============================================"
Write-Host ""
Write-Info "  Backend API:  http://localhost:${API_PORT:-5000}/swagger"
Write-Info "  Frontend:     http://localhost:${HTTP_PORT:-80}"
Write-Info "  PostgreSQL:   localhost:${POSTGRES_PORT:-5432}"
Write-Info "  Redis:        localhost:${REDIS_PORT:-6379}"
Write-Host ""
Write-Info "Для запуска frontend dev server с hot-reload:"
Write-Info "  cd frontend && npm run start"
Write-Host ""
Write-Info "Для остановки: .\launcher\stop.ps1"
Write-Host ""
