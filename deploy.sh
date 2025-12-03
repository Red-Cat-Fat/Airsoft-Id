#!/bin/bash

# Strikeball Social Network Deployment Script
# Deploys the application on Ubuntu Linux using Docker and Docker Compose

set -e  # Exit on any error

# Color codes for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Script information
SCRIPT_VERSION="1.0.0"
SCRIPT_NAME="Strikeball Deployment Script"

# Default configuration
PROJECT_DIR="/opt/strikeball"
DATA_DIR="/var/lib/strikeball"
DEPLOY_USER="strikeball"
DOMAIN_NAME="localhost"
INSTALL_PREREQUISITES=false
INITIAL_DEPLOY=false
UPDATE_DEPLOY=false
RECONFIGURE=false
SHOW_STATUS=false
STOP_SERVICES=false
START_SERVICES=false
RESTART_SERVICES=false
MIGRATE_DB=false
SHOW_LOGS=false
HEALTH_CHECK=false

# Log file
LOG_FILE="/var/log/strikeball-deploy.log"

# Function to print colored output
print_status() {
    echo -e "${BLUE}[$(date +'%Y-%m-%d %H:%M:%S')]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[$(date +'%Y-%m-%d %H:%M:%S')] SUCCESS:${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[$(date +'%Y-%m-%d %H:%M:%S')] WARNING:${NC} $1"
}

print_error() {
    echo -e "${RED}[$(date +'%Y-%m-%d %H:%M:%S')] ERROR:${NC} $1"
}

log_message() {
    echo "[$(date +'%Y-%m-%d %H:%M:%S')] $1" >> "$LOG_FILE"
}

# Function to check if running as root
check_root() {
    if [[ $EUID -eq 0 ]]; then
        print_error "This script should not be run as root. Please run as a regular user with sudo privileges."
        exit 1
    fi
}

# Function to check Ubuntu version
check_ubuntu_version() {
    print_status "Checking Ubuntu version..."
    
    if ! command -v lsb_release &> /dev/null; then
        print_error "lsb_release not found. This script requires Ubuntu Linux."
        exit 1
    fi
    
    UBUNTU_VERSION=$(lsb_release -rs)
    if [[ $(echo "$UBUNTU_VERSION < 20.04" | bc -l) -eq 1 ]]; then
        print_error "Ubuntu version $UBUNTU_VERSION is not supported. Minimum required version is 20.04."
        exit 1
    fi
    
    print_success "Ubuntu $UBUNTU_VERSION detected"
}

# Function to install prerequisites
install_prerequisites() {
    print_status "Installing prerequisites..."
    
    # Update package lists
    sudo apt update
    
    # Install required packages
    sudo apt install -y \
        apt-transport-https \
        ca-certificates \
        curl \
        gnupg \
        lsb-release \
        wget \
        git \
        nodejs \
        npm
    
    # Install Docker
    if ! command -v docker &> /dev/null; then
        print_status "Installing Docker..."
        
        # Add Docker's official GPG key
        curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo gpg --dearmor -o /usr/share/keyrings/docker-archive-keyring.gpg
        
        # Add Docker repository
        echo \
          "deb [arch=$(dpkg --print-architecture) signed-by=/usr/share/keyrings/docker-archive-keyring.gpg] https://download.docker.com/linux/ubuntu \
          $(lsb_release -cs) stable" | sudo tee /etc/apt/sources.list.d/docker.list > /dev/null
        
        # Update package index and install Docker
        sudo apt update
        sudo apt install -y docker-ce docker-ce-cli containerd.io
        
        # Add user to docker group
        sudo usermod -aG docker "$USER"
        print_warning "You may need to log out and back in for Docker permissions to take effect"
    else
        print_success "Docker is already installed"
    fi
    
    # Install Docker Compose (plugin version)
    if ! docker compose version &> /dev/null; then
        print_status "Installing Docker Compose plugin..."
        sudo apt install -y docker-compose-plugin
    else
        print_success "Docker Compose is already installed"
    fi
    
    # Install Node.js and npm if not present
    if ! command -v node &> /dev/null || ! command -v npm &> /dev/null; then
        print_status "Installing Node.js and npm..."
        curl -fsSL https://deb.nodesource.com/setup_18.x | sudo -E bash -
        sudo apt install -y nodejs
    fi
    
    # Install Angular CLI globally
    if ! command -v ng &> /dev/null; then
        print_status "Installing Angular CLI..."
        sudo npm install -g @angular/cli
    fi
    
    print_success "Prerequisites installation completed"
}

# Function to generate secure passwords
generate_password() {
    openssl rand -base64 32 | tr -d "=+/" | cut -c1-20
}

# Function to create .env file
create_env_file() {
    print_status "Creating environment configuration file..."
    
    # Create project directory if it doesn't exist
    mkdir -p "$PROJECT_DIR"
    
    # Check if .env file already exists
    if [[ -f "$PROJECT_DIR/.env" ]]; then
        print_warning "Environment file already exists. Backing up existing file..."
        cp "$PROJECT_DIR/.env" "$PROJECT_DIR/.env.backup.$(date +%s)"
    fi
    
    # Generate secure passwords if they don't exist
    POSTGRES_PASSWORD=$(generate_password)
    REDIS_PASSWORD=$(generate_password)
    JWT_SECRET_KEY=$(openssl rand -base64 48 | tr -d "=+/" | cut -c1-48)
    
    # Create .env file
    cat > "$PROJECT_DIR/.env" << EOF
# Database Configuration
POSTGRES_DB=strikeballdb
POSTGRES_USER=postgres
POSTGRES_PASSWORD=$POSTGRES_PASSWORD
POSTGRES_PORT=5432

# Redis Configuration
REDIS_PASSWORD=$REDIS_PASSWORD
REDIS_PORT=6379

# API Configuration
API_PORT=5000
JWT_SECRET_KEY=$JWT_SECRET_KEY

# Nginx Configuration
HTTP_PORT=80
HTTPS_PORT=443

# Backup Configuration
BACKUP_RETENTION_DAYS=30
EOF
    
    print_success "Environment configuration file created at $PROJECT_DIR/.env"
}

# Function to clone or update source code
manage_source_code() {
    print_status "Managing source code..."
    
    if [[ -d "$PROJECT_DIR/.git" ]]; then
        print_status "Updating existing repository..."
        cd "$PROJECT_DIR"
        git pull origin main
    else
        print_status "Cloning repository..."
        git clone https://github.com/yourusername/strikeball-social-network.git "$PROJECT_DIR"
        cd "$PROJECT_DIR"
    fi
    
    print_success "Source code management completed"
}

# Function to build backend API
build_backend() {
    print_status "Building backend API..."
    
    cd "$PROJECT_DIR/backend"
    
    # Build Docker image for backend API
    docker build -t strikeball-api .
    
    print_success "Backend API built successfully"
}

# Function to build frontend
build_frontend() {
    print_status "Building frontend application..."
    
    cd "$PROJECT_DIR/frontend"
    
    # Install dependencies
    npm install
    
    # Build production-ready Angular bundle
    npm run build
    
    # Copy build artifacts to Nginx directory
    mkdir -p "$PROJECT_DIR/nginx/dist"
    cp -r dist/* "$PROJECT_DIR/nginx/dist/"
    
    print_success "Frontend application built successfully"
}

# Function to initialize Docker Compose stack
initialize_docker_stack() {
    print_status "Initializing Docker Compose stack..."
    
    cd "$PROJECT_DIR"
    
    # Bring up services in detached mode
    docker compose up -d
    
    # Wait for services to start
    print_status "Waiting for services to initialize..."
    sleep 30
    
    print_success "Docker Compose stack initialized"
}

# Function to run database migrations
run_database_migrations() {
    print_status "Running database migrations..."
    
    cd "$PROJECT_DIR"
    
    # Wait for database to be ready
    print_status "Waiting for database to be ready..."
    sleep 10
    
    # Run Entity Framework migrations
    docker compose exec api dotnet ef database update
    
    print_success "Database migrations completed"
}

# Function to configure systemd service
configure_systemd_service() {
    print_status "Configuring systemd service..."
    
    # Create systemd service file
    sudo tee /etc/systemd/system/strikeball.service > /dev/null << EOF
[Unit]
Description=Strikeball Social Network
After=docker.service
Requires=docker.service

[Service]
Type=oneshot
RemainAfterExit=yes
WorkingDirectory=$PROJECT_DIR
ExecStart=/usr/bin/docker compose up -d
ExecStop=/usr/bin/docker compose down
TimeoutStartSec=0

[Install]
WantedBy=multi-user.target
EOF
    
    # Reload systemd and enable service
    sudo systemctl daemon-reload
    sudo systemctl enable strikeball.service
    
    print_success "Systemd service configured"
}

# Function to perform health checks
perform_health_checks() {
    print_status "Performing health checks..."
    
    # Check if containers are running
    if docker compose ps | grep -q "Up"; then
        print_success "Containers are running"
    else
        print_error "Some containers are not running"
        return 1
    fi
    
    # Check API health
    if curl -f http://localhost:5000/health > /dev/null 2>&1; then
        print_success "API health check passed"
    else
        print_warning "API health check failed"
    fi
    
    # Check if frontend directory exists
    if [[ -d "$PROJECT_DIR/frontend/dist" ]] && [[ -n "$(ls -A "$PROJECT_DIR/frontend/dist")" ]]; then
        print_success "Frontend build artifacts exist"
    else
        print_warning "Frontend build artifacts missing"
    fi
    
    print_success "Health checks completed"
}

# Function to show deployment status
show_status() {
    print_status "Deployment status:"
    
    cd "$PROJECT_DIR"
    
    # Show running containers
    print_status "Running containers:"
    docker compose ps
    
    # Show service logs
    print_status "Recent service logs:"
    docker compose logs --tail=20
}

# Function to stop services
stop_services() {
    print_status "Stopping services..."
    
    cd "$PROJECT_DIR"
    docker compose stop
    
    print_success "Services stopped"
}

# Function to start services
start_services() {
    print_status "Starting services..."
    
    cd "$PROJECT_DIR"
    docker compose start
    
    print_success "Services started"
}

# Function to restart services
restart_services() {
    print_status "Restarting services..."
    
    cd "$PROJECT_DIR"
    docker compose restart
    
    print_success "Services restarted"
}

# Function to show logs
show_logs() {
    print_status "Showing service logs..."
    
    cd "$PROJECT_DIR"
    docker compose logs -f
}

# Function to display help
show_help() {
    echo "$SCRIPT_NAME v$SCRIPT_VERSION"
    echo ""
    echo "Usage: $0 [OPTIONS]"
    echo ""
    echo "Options:"
    echo "  --install-prerequisites    Install all required system packages"
    echo "  --initial-deploy          Perform full initial deployment"
    echo "  --update                  Deploy code updates only"
    echo "  --reconfigure             Update configuration without code changes"
    echo "  --status                  Check deployment status"
    echo "  --stop                    Stop all services"
    echo "  --start                   Start all services"
    echo "  --restart                 Restart all services"
    echo "  --migrate-db              Run database migrations only"
    echo "  --logs                    Show service logs"
    echo "  --health-check            Perform health checks"
    echo "  --help                    Display this help message"
    echo ""
    echo "Environment Variables:"
    echo "  PROJECT_DIR               Directory for application installation (default: /opt/strikeball)"
    echo "  DATA_DIR                  Directory for persistent data storage (default: /var/lib/strikeball)"
    echo "  DEPLOY_USER               User account for running services (default: strikeball)"
    echo "  DOMAIN_NAME               Domain name for the application (default: localhost)"
}

# Function to parse command line arguments
parse_arguments() {
    while [[ $# -gt 0 ]]; do
        case $1 in
            --install-prerequisites)
                INSTALL_PREREQUISITES=true
                shift
                ;;
            --initial-deploy)
                INITIAL_DEPLOY=true
                shift
                ;;
            --update)
                UPDATE_DEPLOY=true
                shift
                ;;
            --reconfigure)
                RECONFIGURE=true
                shift
                ;;
            --status)
                SHOW_STATUS=true
                shift
                ;;
            --stop)
                STOP_SERVICES=true
                shift
                ;;
            --start)
                START_SERVICES=true
                shift
                ;;
            --restart)
                RESTART_SERVICES=true
                shift
                ;;
            --migrate-db)
                MIGRATE_DB=true
                shift
                ;;
            --logs)
                SHOW_LOGS=true
                shift
                ;;
            --health-check)
                HEALTH_CHECK=true
                shift
                ;;
            --help)
                show_help
                exit 0
                ;;
            *)
                print_error "Unknown option: $1"
                show_help
                exit 1
                ;;
        esac
    done
}

# Main function
main() {
    print_status "Starting $SCRIPT_NAME v$SCRIPT_VERSION"
    
    # Parse command line arguments
    parse_arguments "$@"
    
    # Check if any action is specified
    if [[ "$INSTALL_PREREQUISITES" == false && 
          "$INITIAL_DEPLOY" == false && 
          "$UPDATE_DEPLOY" == false && 
          "$RECONFIGURE" == false && 
          "$SHOW_STATUS" == false && 
          "$STOP_SERVICES" == false && 
          "$START_SERVICES" == false && 
          "$RESTART_SERVICES" == false && 
          "$MIGRATE_DB" == false && 
          "$SHOW_LOGS" == false && 
          "$HEALTH_CHECK" == false ]]; then
        print_error "No action specified. Use --help for usage information."
        exit 1
    fi
    
    # Check root privileges
    check_root
    
    # Check Ubuntu version
    check_ubuntu_version
    
    # Execute requested actions
    if [[ "$INSTALL_PREREQUISITES" == true ]]; then
        install_prerequisites
    fi
    
    if [[ "$INITIAL_DEPLOY" == true ]]; then
        create_env_file
        manage_source_code
        build_backend
        build_frontend
        initialize_docker_stack
        run_database_migrations
        configure_systemd_service
        perform_health_checks
    fi
    
    if [[ "$UPDATE_DEPLOY" == true ]]; then
        manage_source_code
        build_backend
        build_frontend
        run_database_migrations
        restart_services
        perform_health_checks
    fi
    
    if [[ "$RECONFIGURE" == true ]]; then
        create_env_file
        restart_services
        perform_health_checks
    fi
    
    if [[ "$SHOW_STATUS" == true ]]; then
        show_status
    fi
    
    if [[ "$STOP_SERVICES" == true ]]; then
        stop_services
    fi
    
    if [[ "$START_SERVICES" == true ]]; then
        start_services
    fi
    
    if [[ "$RESTART_SERVICES" == true ]]; then
        restart_services
    fi
    
    if [[ "$MIGRATE_DB" == true ]]; then
        run_database_migrations
    fi
    
    if [[ "$SHOW_LOGS" == true ]]; then
        show_logs
    fi
    
    if [[ "$HEALTH_CHECK" == true ]]; then
        perform_health_checks
    fi
    
    print_success "$SCRIPT_NAME execution completed"
}

# Run main function with all arguments
main "$@"