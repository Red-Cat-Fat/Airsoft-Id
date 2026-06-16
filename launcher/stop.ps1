$Host.UI.RawUI.WindowTitle = "Strikeball Social Network — Stopper"

function Write-Log   { Write-Host "[✓] $args" -ForegroundColor Green }
function Write-Warn  { Write-Host "[!] $args" -ForegroundColor Yellow }
function Write-Err   { Write-Host "[✗] $args" -ForegroundColor Red }

Write-Host ""
Write-Host "[i] ============================================" -ForegroundColor Cyan
Write-Host "[i]    Strikeball Social Network — Stopper      " -ForegroundColor Cyan
Write-Host "[i] ============================================" -ForegroundColor Cyan
Write-Host ""

$ProjectDir = Split-Path -Path (Split-Path -Path $PSScriptRoot -Parent) -Parent
$FrontendDir = Join-Path $ProjectDir "frontend"

# --- Stop frontend dev server ---
$npmProcess = Get-Process -Name "node" -ErrorAction SilentlyContinue `
    | Where-Object { $_.CommandLine -match "ng serve|angular" }
if ($npmProcess) {
    Write-Warn "Останавливаем frontend dev server..."
    $npmProcess | Stop-Process -Force
    Write-Log "Frontend dev server остановлен."
} else {
    Write-Log "Frontend dev server не запущен."
}

# --- Docker Compose Down ---
$composeFile = Join-Path $ProjectDir "docker-compose.yml"
if (Test-Path $composeFile) {
    Write-Log "Останавливаем Docker-контейнеры..."
    docker compose -f $composeFile down
    Write-Log "Контейнеры остановлены."
} else {
    Write-Warn "docker-compose.yml не найден"
}

Write-Host ""
Write-Log "============================================"
Write-Log "  Проект полностью остановлен.             "
Write-Log "============================================"
Write-Host ""
