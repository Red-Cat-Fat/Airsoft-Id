# Automated Deployment Script for Strikeball Social Network

## Overview

This document outlines the design of an automated deployment script for the Strikeball social network application on an Ubuntu Linux server. The deployment solution will handle the complete stack including the .NET backend API, Angular frontend, PostgreSQL database, Redis cache, and Nginx reverse proxy using Docker containers orchestrated with Docker Compose.

## System Architecture Context

The Strikeball social network consists of:
- Backend: .NET 8.0 Web API (StrikeballApi)
- Frontend: Angular 17 application
- Database: PostgreSQL 15
- Cache: Redis 7
- Reverse Proxy: Nginx for serving frontend and routing API requests
- Backup Service: Automated database backup service

## Deployment Target Environment

- Operating System: Ubuntu Linux (20.04 LTS or newer)
- Containerization: Docker and Docker Compose
- Service Management: systemd for auto-start and monitoring
- Security: Non-root user for running services

## Supported Deployment Scenarios

### Development Deployment
- Single server deployment
- Default configuration values
- HTTP access only (no SSL)
- Local development focus

### Production Deployment
- Custom domain configuration
- SSL/HTTPS support with certificate management
- Custom passwords and secrets
- Resource optimization for production workloads
- Firewall and security hardening

### Staging/QA Deployment
- Similar to production but with test data
- Pre-release testing environment
- May use self-signed certificates

## Deployment Components

### 1. Prerequisites Installation Module

The script will automatically install required system dependencies:
- Docker Engine
- Docker Compose plugin (newer Docker versions)
- Git (for cloning/fetching updates)
- Node.js and npm (for Angular frontend build)
- Essential utilities (curl, wget, gnupg, etc.)

### 2. Application Build Module

#### Backend (.NET Web API)
- Restore NuGet packages
- Build .NET projects in Release configuration
- Create Docker image for the API using the existing Dockerfile

#### Frontend (Angular)
- Install Node.js and npm (if not present)
- Install Angular CLI
- Install frontend dependencies via npm
- Build production-ready Angular bundle
- Place build artifacts in the correct directory for Nginx to serve

Note: Unlike typical deployments, the frontend is not containerized separately but served by the Nginx container from static files.

### 3. Configuration Management

The deployment script will handle environment-specific configurations:
- Generate or update `.env` file with secure passwords and configuration values
- Configure database connection strings
- Set JWT secret keys and other security tokens
- Configure Nginx reverse proxy configuration
- Set up SSL certificates (if provided)

### 4. Database Initialization

- Run Entity Framework migrations to set up database schema
- Handle database password setup securely
- Verify database connectivity before proceeding

### 5. Docker Orchestration

The deployment will use the existing `docker-compose.yml`:
- Services for backend API, frontend (via Nginx), database, cache, and backup service
- Set appropriate restart policies
- Configure volume mappings for persistent data
- Set up networking between containers
- Configure resource limits for each service

### 6. Service Management

- Configure systemd service for Docker Compose stack
- Enable automatic startup on boot
- Implement health checks for critical services
- Setup log rotation for container logs

## Services to be Deployed

### 1. PostgreSQL Database (strikeball-postgres)
- Version: PostgreSQL 15-alpine
- Purpose: Primary data storage for all application data
- Configuration: UTF-8 encoding with en_US locale
- Health Check: pg_isready utility
- Persistence: Dedicated Docker volume for data storage

### 2. Redis Cache (strikeball-redis)
- Version: Redis 7-alpine
- Purpose: Caching layer for improved performance
- Configuration: 1GB memory limit with allkeys-lru eviction policy
- Security: Password protection
- Persistence: Dedicated Docker volume for data storage

### 3. ASP.NET Core API (strikeball-api)
- Framework: .NET 8.0
- Purpose: Backend REST API serving all business logic
- Ports: Internal port 5000
- Health Check: HTTP GET to /health endpoint
- Dependencies: Waits for healthy database and cache services
- Resource Limits: 2GB memory limit, 1GB reservation

### 4. Nginx Reverse Proxy (strikeball-nginx)
- Version: Nginx alpine
- Purpose: Serve Angular frontend and proxy API requests
- Ports: 80 (HTTP) and 443 (HTTPS)
- Configuration: Static file serving for frontend, proxy for API
- Volumes: Mounts for configuration, certificates, and frontend files
- Health Check: HTTP GET to /health endpoint
- Resource Limits: 512MB memory limit, 256MB reservation

### 5. Database Backup Service (strikeball-backup)
- Based on: PostgreSQL 15-alpine
- Purpose: Automated daily database backups
- Retention: Configurable backup retention (default 30 days)
- Storage: Dedicated Docker volume for backup storage
- Schedule: Daily backups with automatic cleanup

## Deployment Workflow

### Initial Deployment Steps

1. Check system requirements and install missing dependencies
2. Clone/update application source code
3. Build Docker images for backend API
4. Build Angular frontend application
5. Configure environment variables and secrets in `.env` file
6. Initialize Docker Compose stack
7. Run Entity Framework migrations
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

### Secrets Management
- Database passwords will be generated randomly or provided securely
- Redis passwords will be generated for secure cache access
- JWT signing keys will be generated for production use (minimum 32 characters)
- HTTPS certificates will be handled via Let's Encrypt or provided certificates
- No secrets will be stored in plain text in scripts or configuration files
- Environment variables will be used for runtime secret injection

### User Permissions
- Services will run under dedicated non-root users
- File permissions will be restricted appropriately
- Docker socket access will be limited
- SSH access will be preserved for administrative tasks

### Network Security
- Firewall configuration recommendations
- Container network isolation
- Port exposure minimization
- Secure communication between services

### Data Protection
- Database encryption at rest (PostgreSQL)
- SSL/TLS encryption in transit
- Regular automated backups with retention policies
- GDPR-compliant data handling procedures

## Monitoring and Maintenance Features

### Logging
- Centralized logging via Docker's logging drivers
- Log rotation to prevent disk space exhaustion
- Error reporting mechanisms
- Structured logging for easier analysis
- Log level configuration per service

### Health Checks
- API endpoint availability verification
- Database connectivity testing
- Frontend application accessibility confirmation
- Container health status monitoring
- Automated restart of unhealthy services

### Backup Capabilities
- Automated daily database backups
- Configurable backup retention policies
- Backup integrity verification
- Manual backup triggers
- Backup restoration procedures

### Maintenance Operations
- Zero-downtime deployment strategies
- Service scaling capabilities
- Performance monitoring and alerting
- Resource utilization tracking
- Automated cleanup of temporary files

## Script Interface Design

### Command Line Arguments
- `--install-prerequisites`: Install all required system packages
- `--initial-deploy`: Perform full initial deployment
- `--update`: Deploy code updates only
- `--reconfigure`: Update configuration without code changes
- `--status`: Check deployment status
- `--stop`: Stop all services
- `--start`: Start all services
- `--restart`: Restart all services
- `--migrate-db`: Run database migrations only
- `--logs`: Show service logs
- `--health-check`: Perform health checks

### Environment Variables
The script will support customization through environment variables:
- `DEPLOY_USER`: User account for running services
- `INSTALL_DIR`: Directory for application installation
- `DATA_DIR`: Directory for persistent data storage
- `DOMAIN_NAME`: Domain name for the application

## Error Handling and Recovery

### Failure Scenarios
- Insufficient system resources (CPU, memory, disk space)
- Network connectivity issues during dependency installation
- Database migration failures
- Docker daemon problems
- Configuration file errors
- Service health check failures
- Certificate validation errors
- Permission denied errors

### Recovery Procedures
- Rollback capabilities for failed updates
- Automatic retry mechanisms for transient failures
- Detailed error logging for troubleshooting
- Manual intervention points for critical failures
- Graceful degradation strategies
- Service isolation and containment
- Automated alerts for critical failures

### Diagnostic Features
- Comprehensive logging of all operations
- Pre-flight environment checks
- Dependency validation
- Configuration file validation
- Service status reporting
- Resource utilization monitoring

## Post-Deployment Verification

The script will perform automated verification checks:
- API endpoints respond correctly with expected status codes
- Frontend loads without errors and assets are accessible
- User registration/login functionality works
- Database connections are functional
- Redis cache is accessible
- SSL/TLS certificates are valid (if applicable)
- Container health checks are passing
- Backup service is operational
- Nginx reverse proxy is routing correctly

### Verification Tests
- HTTP status code validation for critical endpoints
- Response time measurements
- Database query validation
- Authentication flow testing
- Static asset loading verification
- Health check endpoint validation

## Customization Points

### Branding
- Application name and logo
- Color schemes
- Custom domain configuration
- Email templates and notifications
- Legal pages and policies

### Scaling Options
- Single-server deployment (default)
- Multi-container setup for load balancing
- External database configuration
- CDN integration for static assets
- Horizontal scaling of API instances
- Database read replicas

### Feature Toggles
- Optional service components
- Development vs. production features
- Beta feature flags
- A/B testing capabilities

## Dependencies

### Runtime Dependencies
- Ubuntu 20.04+ system (or compatible Debian-based distribution)
- Internet access for package downloads
- Sufficient disk space for Docker images and data (minimum 10GB recommended)
- Available RAM (minimum 4GB recommended, 8GB+ for production)
- CPU resources (minimum 2 cores recommended)
- Firewall access for required ports (80, 443, 5432, 6379)

### Development Dependencies
- Shell scripting environment (bash)
- Standard Unix utilities (curl, wget, grep, sed, awk)
- Docker CLI tools
- Git for source code management
- Text editors for configuration files

### Optional Dependencies
- External SMTP server for email notifications
- CDN service for static asset delivery
- DNS management tools for domain configuration
- SSL certificate management tools

## Future Extensibility

### Potential Enhancements
- Integration with CI/CD pipelines
- Kubernetes deployment option
- Multi-environment support (staging/production)
- Automated SSL certificate management
- Backup and disaster recovery automation
- Monitoring and alerting integration (Prometheus, Grafana)
- Log aggregation and analysis (ELK stack)
- Auto-scaling based on load metrics
- Geographic distribution and CDN integration
- Microservice architecture evolution
- Advanced security scanning and compliance checking

## Implementation Considerations

### Performance Optimization
- Container resource limits and reservations
- Database connection pooling
- Caching strategies with Redis
- CDN integration for static assets
- Load balancing configurations

### Compliance and Standards
- GDPR data protection requirements
- OWASP security guidelines
- ISO 27001 compliance measures
- PCI DSS considerations (if payment processing is added)

## Conclusion

This deployment script design provides a comprehensive solution for automating the deployment of the Strikeball social network application on Ubuntu Linux servers. By leveraging Docker and Docker Compose, the solution ensures consistency across different environments while maintaining flexibility for various deployment scenarios. The modular approach allows for easy maintenance and future enhancements, making it suitable for both development and production use cases.
