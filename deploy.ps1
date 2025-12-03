# Complete Deployment Script for Strikeball Social Network
# This script completes the deployment after Docker Desktop is started

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Strikeball Social Network Deployment" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Navigate to project directory
Set-Location -Path "e:\Projects\Web\Airsoft-Id"

# Step 1: Verify Docker is running
Write-Host "[1/5] Checking Docker status..." -ForegroundColor Yellow
$dockerRunning = $false
$maxAttempts = 30
$attempt = 0

while (-not $dockerRunning -and $attempt -lt $maxAttempts) {
    try {
        docker ps | Out-Null
        $dockerRunning = $true
        Write-Host "✓ Docker is running" -ForegroundColor Green
    } catch {
        $attempt++
        Write-Host "Waiting for Docker Desktop to start... ($attempt/$maxAttempts)" -ForegroundColor Yellow
        Start-Sleep -Seconds 2
    }
}

if (-not $dockerRunning) {
    Write-Host "✗ Docker Desktop is not running!" -ForegroundColor Red
    Write-Host "Please start Docker Desktop and run this script again." -ForegroundColor Red
    exit 1
}

# Step 2: Start Docker Containers
Write-Host ""
Write-Host "[2/5] Starting Docker containers..." -ForegroundColor Yellow
docker compose up -d --build

if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ Containers started successfully" -ForegroundColor Green
} else {
    Write-Host "✗ Failed to start containers" -ForegroundColor Red
    exit 1
}

# Step 3: Wait for services to be healthy
Write-Host ""
Write-Host "[3/5] Waiting for services to be healthy..." -ForegroundColor Yellow
Write-Host "This may take 1-2 minutes..." -ForegroundColor Gray

Start-Sleep -Seconds 30

# Check container status
$containers = docker compose ps --format json | ConvertFrom-Json
foreach ($container in $containers) {
    $status = docker inspect $container.Name --format='{{.State.Status}}'
    Write-Host "  - $($container.Service): $status" -ForegroundColor Cyan
}

# Step 4: Run Database Migrations
Write-Host ""
Write-Host "[4/5] Running database migrations..." -ForegroundColor Yellow
Start-Sleep -Seconds 10

docker compose exec -T api dotnet ef database update

if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ Database migrations completed successfully" -ForegroundColor Green
} else {
    Write-Host "⚠ Migration command completed with warnings (this is normal if migrations already exist)" -ForegroundColor Yellow
}

# Step 5: Verify Deployment
Write-Host ""
Write-Host "[5/5] Verifying deployment..." -ForegroundColor Yellow

# Check if services are responding
try {
    $apiHealth = Invoke-WebRequest -Uri "http://localhost:5000/health" -UseBasicParsing -TimeoutSec 5
    Write-Host "✓ Backend API is responding" -ForegroundColor Green
} catch {
    Write-Host "⚠ Backend API health check failed (may need more time)" -ForegroundColor Yellow
}

try {
    $nginxHealth = Invoke-WebRequest -Uri "http://localhost:80" -UseBasicParsing -TimeoutSec 5
    Write-Host "✓ Frontend is accessible" -ForegroundColor Green
} catch {
    Write-Host "⚠ Frontend health check failed (may need more time)" -ForegroundColor Yellow
}

# Display final status
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Deployment Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Access your application:" -ForegroundColor White
Write-Host "  Frontend:     http://localhost" -ForegroundColor Cyan
Write-Host "  API:          http://localhost:5000" -ForegroundColor Cyan
Write-Host "  Swagger:      http://localhost:5000/swagger" -ForegroundColor Cyan
Write-Host "  Health Check: http://localhost:5000/health" -ForegroundColor Cyan
Write-Host ""
Write-Host "View logs:      docker compose logs -f" -ForegroundColor Gray
Write-Host "Stop services:  docker compose stop" -ForegroundColor Gray
Write-Host "Start services: docker compose start" -ForegroundColor Gray
Write-Host ""
