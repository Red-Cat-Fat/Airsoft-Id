# Strikeball Social Network Deployment Script

This directory contains a shell script (`deploy.sh`) for deploying the Strikeball social network application on Ubuntu Linux servers.

## Overview

The deployment script automates the process of setting up the complete Strikeball application stack including:
- ASP.NET Core 8.0 Web API
- Angular 17 frontend
- PostgreSQL 15 database
- Redis 7 cache
- Nginx reverse proxy
- Automated backup service

All services are containerized using Docker and orchestrated with Docker Compose.

## Prerequisites

- Ubuntu 20.04 LTS or newer
- Internet access for package downloads
- Sufficient system resources (minimum 4GB RAM, 2 CPU cores)
- User account with sudo privileges

## Usage

Make the script executable:
```bash
chmod +x deploy.sh
```

### Install Prerequisites
```bash
./deploy.sh --install-prerequisites
```

### Initial Deployment
```bash
./deploy.sh --initial-deploy
```

### Update Existing Deployment
```bash
./deploy.sh --update
```

### Reconfigure Environment
```bash
./deploy.sh --reconfigure
```

### Check Deployment Status
```bash
./deploy.sh --status
```

### Service Management
```bash
./deploy.sh --stop
./deploy.sh --start
./deploy.sh --restart
```

### Database Operations
```bash
./deploy.sh --migrate-db
```

### Monitoring
```bash
./deploy.sh --logs
./deploy.sh --health-check
```

## Environment Variables

The script supports customization through environment variables:

- `PROJECT_DIR`: Directory for application installation (default: /opt/strikeball)
- `DATA_DIR`: Directory for persistent data storage (default: /var/lib/strikeball)
- `DEPLOY_USER`: User account for running services (default: strikeball)
- `DOMAIN_NAME`: Domain name for the application (default: localhost)

Example:
```bash
PROJECT_DIR=/home/myuser/strikeball DATA_DIR=/data/strikeball ./deploy.sh --initial-deploy
```

## Deployment Process

### Initial Deployment Steps

1. Install system prerequisites (Docker, Docker Compose, Node.js, etc.)
2. Create secure environment configuration with randomly generated passwords
3. Clone or update source code repository
4. Build backend API Docker image
5. Build Angular frontend application
6. Initialize Docker Compose stack
7. Run Entity Framework database migrations
8. Configure systemd service for automatic startup
9. Perform health checks to verify successful deployment

### Update Deployment Steps

1. Pull latest source code changes
2. Rebuild backend API Docker image
3. Rebuild Angular frontend application
4. Apply database migrations if needed
5. Restart Docker Compose services
6. Verify updated services are running correctly

## Security Considerations

- Secure passwords are automatically generated for database and Redis
- JWT secret keys are generated with sufficient entropy
- Services run under dedicated non-root users
- File permissions are restricted appropriately
- Secrets are stored in environment variables, not in scripts

## Monitoring and Maintenance

The script includes built-in commands for monitoring:
- Service status checking
- Log viewing
- Health checks
- Backup verification

## Troubleshooting

If you encounter issues:

1. Check the log file at `/var/log/strikeball-deploy.log`
2. Verify Docker is running: `sudo systemctl status docker`
3. Check container status: `docker compose ps`
4. View service logs: `docker compose logs`

Common issues and solutions:

- **Permission denied**: Ensure your user is in the docker group
- **Port conflicts**: Check if required ports (80, 443, 5432, 6379) are in use
- **Insufficient resources**: Ensure adequate CPU, memory, and disk space

## Customization

The deployment can be customized for different environments:
- Development: Default configuration with HTTP only
- Staging: Similar to production with test data
- Production: Custom domains, SSL certificates, optimized resources

## Support

For support and questions, please open an issue in the GitHub repository.