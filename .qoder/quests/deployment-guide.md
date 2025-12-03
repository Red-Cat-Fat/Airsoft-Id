# Deployment Guide - Strikeball Social Network

## Overview

This deployment guide provides comprehensive instructions for deploying the Strikeball Social Network application in various environments. The application consists of a multi-tier architecture with Angular frontend, ASP.NET Core backend, PostgreSQL database, Redis cache, and Nginx reverse proxy.

## Deployment Scope

This guide covers three deployment scenarios:

- **Development Deployment**: Local environment for development and testing
- **Staging Deployment**: Pre-production environment for quality assurance
- **Production Deployment**: Live production environment with full security and optimization

## Architecture Overview

The system architecture consists of the following components deployed as Docker containers:

| Component | Technology | Purpose | Port |
|-----------|-----------|---------|------|
| Frontend | Angular 17 | User interface application | 80/443 |
| Backend API | ASP.NET Core 8.0 | RESTful API service | 5000 |
| Database | PostgreSQL 15 | Primary data storage | 5432 |
| Cache | Redis 7 | Session and data caching | 6379 |
| Reverse Proxy | Nginx Alpine | Load balancing and SSL termination | 80/443 |
| Backup Service | PostgreSQL Client | Automated database backup | N/A |

## System Requirements

### Hardware Requirements

| Environment | CPU | RAM | Storage | Network |
|------------|-----|-----|---------|---------|
| Development | 2 cores | 4 GB | 20 GB | 10 Mbps |
| Staging | 4 cores | 8 GB | 50 GB | 100 Mbps |
| Production | 8+ cores | 16+ GB | 200+ GB SSD | 1 Gbps |

### Software Requirements

The following software must be installed on the deployment target system:

- **Operating System**: Linux (Ubuntu 20.04+ / Debian 11+ / CentOS 8+) or Windows Server 2019+
- **Docker Engine**: Version 20.10 or higher
- **Docker Compose**: Version 2.0 or higher
- **Git**: Version 2.30 or higher (for source code retrieval)
- **OpenSSL**: For SSL certificate generation (production only)
- **Firewall**: UFW (Linux) or Windows Firewall configured appropriately

### Network Requirements

The deployment environment must satisfy these network prerequisites:

- Stable internet connection for Docker image downloads
- Accessible ports for service communication
- DNS configuration for domain name resolution (production)
- SSL certificate authority access (production with HTTPS)

## Pre-Deployment Preparation

### Step 1: Environment Validation

Before beginning deployment, validate that the target environment meets all requirements.

**Server Access Verification**

Ensure administrative access to the target server with sufficient privileges to install software, configure networking, and manage Docker containers.

**Docker Installation Verification**

Verify Docker installation and functionality by checking the daemon status and testing basic container operations. The Docker service must be running and accessible to the deployment user.

**Network Port Availability**

Confirm that the following ports are available and not in use by other services:

- Port 80: HTTP traffic
- Port 443: HTTPS traffic (production)
- Port 5000: Backend API (internal)
- Port 5432: PostgreSQL (internal)
- Port 6379: Redis (internal)

### Step 2: Source Code Acquisition

Obtain the application source code from the version control repository.

**Repository Cloning**

Clone the repository to a dedicated directory on the target server. The directory should have sufficient space and appropriate permissions for the deployment user.

**Branch Selection**

Select the appropriate branch based on the deployment environment:

- Development: develop or feature branches
- Staging: staging or release-candidate branches
- Production: main or master branch with stable release tags

### Step 3: Directory Structure Preparation

Create and organize the necessary directory structure for deployment artifacts.

**Application Directories**

The following directories must be created or verified:

- Application root directory containing all source code
- Configuration directory for environment-specific settings
- Certificate directory for SSL certificates (production)
- Logs directory for application and service logs
- Backup directory for database backups

**Permission Configuration**

Set appropriate file system permissions to ensure the Docker containers can access required directories while maintaining security. The application should run with minimal privileges necessary for operation.

### Step 4: Configuration Management

Prepare environment-specific configuration files before deployment.

**Environment Variables File**

Create a configuration file containing all environment variables required by the application. This file should never be committed to version control and must be secured with appropriate file permissions.

The configuration must include:

- Database credentials with strong, unique passwords
- Redis authentication password
- JWT signing secret key (minimum 32 characters, cryptographically random)
- Port mappings for all services
- Backup retention policies

**Security Configuration**

Generate secure values for all sensitive configuration parameters:

- Database passwords must be at least 16 characters with mixed case, numbers, and symbols
- Redis passwords must be at least 16 characters with mixed case, numbers, and symbols
- JWT secret keys must be at least 32 characters and cryptographically secure

**Configuration Validation**

Verify that all required configuration parameters are present and properly formatted before proceeding with deployment.

## Development Deployment

This section covers deploying the application in a local development environment.

### Step 1: Environment Configuration

Create the environment configuration file by copying the example template and customizing values for development.

**Configuration File Creation**

Copy the example environment file to create the active configuration file. The file should be named exactly as expected by Docker Compose to ensure proper variable substitution.

**Development-Specific Settings**

Configure the following settings appropriate for development:

- Use development-grade passwords (still secure but not production-level)
- Enable detailed logging for debugging
- Configure database on default port 5432
- Configure Redis on default port 6379
- Set API port to 5000
- Use HTTP only (no SSL required)

**Example Development Configuration**

```
Database Configuration:
- Database Name: strikeballdb
- Database User: postgres
- Database Password: minimum 12 characters
- Database Port: 5432

Cache Configuration:
- Redis Password: minimum 12 characters
- Redis Port: 6379

API Configuration:
- API Port: 5000
- JWT Secret: minimum 32 characters
- Environment: Development

Web Server Configuration:
- HTTP Port: 80
- HTTPS Port: 443 (not used in development)

Backup Configuration:
- Retention Period: 7 days
```

### Step 2: Frontend Build Preparation

The Angular frontend must be built before it can be served by Nginx.

**Install Frontend Dependencies**

Navigate to the frontend directory and install all required Node.js packages. This downloads and configures all libraries and tools needed for the Angular application.

**Build Frontend Application**

Execute the Angular build process to compile the TypeScript code, bundle assets, and optimize the application for deployment. The build process should target development configuration with source maps enabled for debugging.

**Build Output Verification**

Confirm that the build process completed successfully and generated the expected output files. The build artifacts should be located in the distribution directory as configured in the Angular build settings.

### Step 3: Container Deployment

Deploy all application services using Docker Compose orchestration.

**Start All Services**

Execute Docker Compose to build images and start all containers in detached mode. This operation will:

- Pull required base images from Docker Hub
- Build the ASP.NET Core API container
- Create the Docker network for inter-container communication
- Initialize persistent volumes for data storage
- Start all service containers
- Execute health checks for each service

**Service Startup Verification**

Monitor the startup process to ensure all services initialize correctly. Each service has health check mechanisms that verify operational readiness.

**Expected Service States**

All services should reach healthy status within the specified timeout periods:

- PostgreSQL: Healthy after accepting connections
- Redis: Healthy after responding to ping commands
- Backend API: Healthy after responding to health endpoint
- Nginx: Healthy after responding to HTTP requests
- Backup Service: Running and scheduled for daily execution

### Step 4: Database Initialization

Initialize the database schema and seed initial data.

**Database Migration Execution**

Execute Entity Framework Core migrations to create the database schema. This operation creates all tables, indexes, constraints, and relationships defined in the domain model.

**Migration Verification**

Confirm that migrations executed successfully by checking the migration history table in the database. All pending migrations should be applied and recorded.

**Initial Data Seeding**

If seed data is configured, verify that initial data was inserted correctly. This may include default roles, system configuration, or sample data for development.

### Step 5: Application Access Verification

Verify that the application is accessible and functioning correctly.

**API Health Check**

Test the backend API health endpoint to confirm the service is responding. The health check should return a successful status indicating all dependencies are available.

**API Documentation Access**

Access the Swagger documentation interface to explore available API endpoints. The documentation should display all controllers and operations with request/response schemas.

**Frontend Access**

Navigate to the frontend application URL to verify the Angular application loads correctly. The application should display the login or landing page.

**Service Log Review**

Examine logs from all services to identify any warnings or errors that occurred during startup. Address any issues before proceeding with development activities.

## Staging Deployment

This section covers deploying the application to a staging environment that mirrors production configuration.

### Step 1: Server Preparation

Prepare the staging server with appropriate security and configuration.

**Server Provisioning**

Provision a dedicated server or virtual machine that meets the staging hardware requirements. The server should be isolated from development and production environments.

**Firewall Configuration**

Configure the server firewall to restrict access to only necessary ports:

- Allow port 22 for SSH administrative access (restrict to known IP addresses)
- Allow port 80 for HTTP traffic
- Allow port 443 for HTTPS traffic
- Block all other incoming traffic
- Allow all outgoing traffic

**Security Hardening**

Apply security best practices to the staging server:

- Disable root login via SSH
- Configure SSH key-based authentication only
- Set up automatic security updates
- Configure system logging and monitoring
- Implement intrusion detection if required

### Step 2: SSL Certificate Configuration

Configure SSL certificates for HTTPS access in staging environment.

**Certificate Acquisition**

Obtain SSL certificates appropriate for staging:

- Use Let's Encrypt for free automated certificates
- Use a wildcard certificate for subdomain flexibility
- Use self-signed certificates only if external access is not required

**Certificate Installation**

Place certificate files in the designated certificate directory with appropriate naming:

- Public certificate: cert.crt or fullchain.pem
- Private key: cert.key or privkey.pem
- Certificate chain: chain.pem (if separate from fullchain)

**Certificate Permission Configuration**

Set restrictive permissions on certificate files to prevent unauthorized access. Private keys should be readable only by the Nginx process.

**Nginx SSL Configuration**

Update the Nginx configuration to enable HTTPS:

- Enable the HTTPS server block
- Configure certificate file paths
- Specify strong SSL protocols (TLS 1.2 and 1.3 only)
- Configure secure cipher suites
- Enable HTTP to HTTPS redirect
- Configure HSTS header for enhanced security

### Step 3: Domain Configuration

Configure DNS and domain settings for the staging environment.

**DNS Record Creation**

Create DNS records pointing to the staging server:

- A record for the primary domain or subdomain
- AAAA record if IPv6 is supported
- CNAME records for any additional subdomains

**DNS Propagation Verification**

Verify that DNS records have propagated and resolve correctly to the staging server IP address.

**Domain-Specific Configuration**

Update application configuration files with the staging domain:

- Update Nginx server_name directive
- Configure CORS allowed origins in backend API
- Update frontend API endpoint URLs
- Configure any external service webhooks or callbacks

### Step 4: Environment-Specific Configuration

Create and configure staging-specific settings.

**Environment Variables Configuration**

Create staging environment file with production-grade security:

- Generate cryptographically strong passwords for all services
- Use unique passwords different from development and production
- Configure production-like JWT settings with appropriate expiration
- Set appropriate resource limits and timeouts
- Configure monitoring and logging endpoints if available

**Application Settings Override**

Configure staging-specific application settings:

- Set API base URLs to staging domain
- Configure external service endpoints to staging or sandbox versions
- Enable detailed logging for troubleshooting
- Configure email service with staging-specific settings
- Set up staging-specific payment gateway credentials if applicable

### Step 5: Deployment Execution

Deploy the application to staging environment.

**Build Production Frontend**

Build the Angular frontend with production optimization:

- Enable ahead-of-time compilation
- Enable production mode optimizations
- Minimize and uglify JavaScript bundles
- Optimize assets and images
- Generate source maps for debugging (optional in staging)

**Deploy Containers**

Execute Docker Compose deployment with production configuration:

- Use production-grade base images
- Enable resource limits on containers
- Configure restart policies for high availability
- Set up container logging with appropriate retention
- Enable health monitoring for all services

**Database Initialization**

Initialize the staging database:

- Execute all database migrations
- Verify schema matches latest domain model
- Seed staging-appropriate test data
- Configure database backup schedule

### Step 6: Post-Deployment Verification

Verify staging deployment is functional and secure.

**Functional Testing**

Execute comprehensive functional tests:

- Verify all API endpoints respond correctly
- Test authentication and authorization flows
- Verify frontend loads and navigates properly
- Test critical user workflows end-to-end
- Verify external service integrations

**Security Testing**

Conduct security verification:

- Verify HTTPS is enforced for all connections
- Test that HTTP requests redirect to HTTPS
- Verify security headers are present in responses
- Test authentication token expiration and refresh
- Verify rate limiting and input validation

**Performance Baseline**

Establish performance baselines for staging:

- Measure API response times
- Measure page load times
- Monitor resource utilization
- Document expected performance metrics

## Quick Start: VPS Deployment Commands

This section provides a complete command sequence for deploying the application on a fresh VPS server. Follow these commands in order after connecting to your server via SSH.

### Prerequisites Verification

Before starting, verify that Docker and Git are installed and running:

```
Verify Docker installation:
docker --version
docker compose version

Verify Docker daemon is running:
sudo systemctl status docker

Verify Git installation:
git --version
```

If Docker is not running, start it:

```
sudo systemctl start docker
sudo systemctl enable docker
```

### Step-by-Step Deployment Commands

**Step 1: Create Application Directory**

Create a dedicated directory for the application and navigate to it:

```
mkdir -p /opt/strikeball
cd /opt/strikeball
```

**Step 2: Clone Repository**

Clone the application repository from your Git hosting service:

```
git clone https://github.com/yourusername/Airsoft-Id.git .
```

Alternatively, if using a specific branch:

```
git clone -b main https://github.com/yourusername/Airsoft-Id.git .
```

**Step 3: Create Environment Configuration**

Copy the example environment file and open it for editing:

```
cp .env.example .env
nano .env
```

Update the following values in the .env file (use strong, unique passwords):

```
POSTGRES_PASSWORD=YourSecurePostgresPassword123!
REDIS_PASSWORD=YourSecureRedisPassword456!
JWT_SECRET_KEY=YourSuperSecretJWTKeyMustBeAtLeast64CharactersLongForProductionSecurity!
```

Save and exit the editor (Ctrl+X, then Y, then Enter in nano).

**Step 4: Generate Strong Passwords**

Optionally, use these commands to generate secure random passwords:

```
Generate PostgreSQL password:
openssl rand -base64 32

Generate Redis password:
openssl rand -base64 32

Generate JWT secret key:
openssl rand -base64 64
```

Copy these generated values into your .env file.

**Step 5: Install Node.js and npm**

Install Node.js required for building the frontend:

```
For Ubuntu/Debian:
curl -fsSL https://deb.nodesource.com/setup_18.x | sudo -E bash -
sudo apt-get install -y nodejs

For CentOS/RHEL:
curl -fsSL https://rpm.nodesource.com/setup_18.x | sudo bash -
sudo yum install -y nodejs

Verify installation:
node --version
npm --version
```

**Step 6: Build Frontend Application**

Navigate to frontend directory, install dependencies, and build:

```
cd frontend
npm install
npm run build
cd ..
```

The build process will create optimized production files in the `frontend/dist` directory.

**Step 7: Create Required Directories**

Create directories for SSL certificates and backups:

```
mkdir -p nginx/certs
mkdir -p backups
```

**Step 8: Configure Firewall**

Configure the firewall to allow necessary traffic:

```
For Ubuntu/Debian (UFW):
sudo ufw allow 22/tcp
sudo ufw allow 80/tcp
sudo ufw allow 443/tcp
sudo ufw enable
sudo ufw status

For CentOS/RHEL (firewalld):
sudo firewall-cmd --permanent --add-service=ssh
sudo firewall-cmd --permanent --add-service=http
sudo firewall-cmd --permanent --add-service=https
sudo firewall-cmd --reload
sudo firewall-cmd --list-all
```

**Step 9: Start Docker Containers**

Build and start all application containers:

```
docker compose up -d --build
```

This command will:
- Build the backend API Docker image
- Pull PostgreSQL, Redis, and Nginx images
- Create Docker network and volumes
- Start all containers in detached mode

**Step 10: Verify Container Status**

Check that all containers are running:

```
docker compose ps
```

You should see all services with status "Up" and "healthy".

**Step 11: View Container Logs**

Monitor the startup logs to ensure no errors:

```
View all logs:
docker compose logs

View specific service logs:
docker compose logs api
docker compose logs postgres
docker compose logs redis
docker compose logs nginx

Follow logs in real-time:
docker compose logs -f
```

**Step 12: Wait for Services to Initialize**

Wait for all health checks to pass (typically 30-60 seconds):

```
watch -n 5 'docker compose ps'
```

Press Ctrl+C when all services show "healthy" status.

**Step 13: Run Database Migrations**

Initialize the database schema:

```
docker compose exec api dotnet ef database update
```

If migrations execute successfully, you'll see confirmation messages.

**Step 14: Verify Application Access**

Test that the application is accessible:

```
Test API health endpoint:
curl http://localhost:5000/health

Test Nginx proxy:
curl http://localhost/health

Test from external network (replace YOUR_SERVER_IP):
curl http://YOUR_SERVER_IP/health
```

**Step 15: Access Application**

The application is now deployed and accessible:

- Frontend: http://YOUR_SERVER_IP
- API: http://YOUR_SERVER_IP/api
- Swagger Documentation: http://YOUR_SERVER_IP/swagger

### Post-Deployment Verification

Perform these verification steps:

**Check Container Health:**

```
docker compose ps
docker inspect strikeball-api --format='{{.State.Health.Status}}'
docker inspect strikeball-postgres --format='{{.State.Health.Status}}'
docker inspect strikeball-redis --format='{{.State.Health.Status}}'
```

**Check Resource Usage:**

```
docker stats --no-stream
```

**Check Database Connection:**

```
docker compose exec postgres psql -U postgres -d strikeballdb -c "SELECT version();"
```

**Check Redis Connection:**

```
docker compose exec redis redis-cli -a YourRedisPassword ping
```

**View Application Logs:**

```
docker compose logs api --tail=100
```

### Optional: SSL Certificate Setup

To enable HTTPS with Let's Encrypt:

**Install Certbot:**

```
For Ubuntu/Debian:
sudo apt-get update
sudo apt-get install -y certbot

For CentOS/RHEL:
sudo yum install -y certbot
```

**Stop Nginx temporarily:**

```
docker compose stop nginx
```

**Obtain SSL Certificate:**

```
Replace yourdomain.com with your actual domain:
sudo certbot certonly --standalone -d yourdomain.com -d www.yourdomain.com
```

**Copy Certificates to Nginx Directory:**

```
sudo cp /etc/letsencrypt/live/yourdomain.com/fullchain.pem nginx/certs/cert.crt
sudo cp /etc/letsencrypt/live/yourdomain.com/privkey.pem nginx/certs/cert.key
sudo chmod 644 nginx/certs/cert.crt
sudo chmod 600 nginx/certs/cert.key
```

**Update Nginx Configuration:**

```
nano nginx/conf.d/default.conf
```

Uncomment the HTTPS server block and update server_name with your domain.

**Restart Nginx:**

```
docker compose start nginx
```

**Set Up Auto-Renewal:**

```
Test renewal:
sudo certbot renew --dry-run

Add to crontab for automatic renewal:
sudo crontab -e

Add this line:
0 3 * * * certbot renew --quiet --deploy-hook "cp /etc/letsencrypt/live/yourdomain.com/fullchain.pem /opt/strikeball/nginx/certs/cert.crt && cp /etc/letsencrypt/live/yourdomain.com/privkey.pem /opt/strikeball/nginx/certs/cert.key && docker compose -f /opt/strikeball/docker-compose.yml restart nginx"
```

### Useful Management Commands

**Stop Application:**

```
cd /opt/strikeball
docker compose stop
```

**Start Application:**

```
cd /opt/strikeball
docker compose start
```

**Restart Application:**

```
cd /opt/strikeball
docker compose restart
```

**View Real-Time Logs:**

```
cd /opt/strikeball
docker compose logs -f api
```

**Update Application:**

```
cd /opt/strikeball
git pull
cd frontend
npm install
npm run build
cd ..
docker compose up -d --build
docker compose exec api dotnet ef database update
```

**Create Manual Database Backup:**

```
cd /opt/strikeball
docker compose exec postgres pg_dump -U postgres strikeballdb | gzip > backups/manual-backup-$(date +%Y%m%d-%H%M%S).sql.gz
```

**View Disk Usage:**

```
docker system df
docker volume ls
du -sh /var/lib/docker/volumes/
```

**Clean Up Unused Resources:**

```
Remove stopped containers:
docker container prune -f

Remove unused images:
docker image prune -a -f

Remove unused volumes (WARNING: only if you're sure):
docker volume prune -f
```

### Troubleshooting Common Issues

**If API container fails to start:**

```
Check logs:
docker compose logs api

Check if database is ready:
docker compose exec postgres pg_isready -U postgres

Restart API:
docker compose restart api
```

**If database connection fails:**

```
Verify environment variables:
docker compose exec api printenv | grep POSTGRES

Test database connection:
docker compose exec postgres psql -U postgres -c "\l"

Check network connectivity:
docker network inspect strikeball-network
```

**If frontend doesn't load:**

```
Verify build files exist:
ls -la frontend/dist/

Check Nginx configuration:
docker compose exec nginx nginx -t

Review Nginx logs:
docker compose logs nginx
```

**If port conflicts occur:**

```
Check what's using the port:
sudo netstat -tulpn | grep :80
sudo netstat -tulpn | grep :443
sudo netstat -tulpn | grep :5000

Kill conflicting process or change ports in .env file
```

### Security Hardening Commands

**Restrict Docker Socket Permissions:**

```
sudo chmod 660 /var/run/docker.sock
sudo usermod -aG docker $USER
```

**Set Up Automatic Security Updates:**

```
For Ubuntu/Debian:
sudo apt-get install -y unattended-upgrades
sudo dpkg-reconfigure -plow unattended-upgrades

For CentOS/RHEL:
sudo yum install -y yum-cron
sudo systemctl enable yum-cron
sudo systemctl start yum-cron
```

**Configure Fail2ban for SSH Protection:**

```
sudo apt-get install -y fail2ban
sudo systemctl enable fail2ban
sudo systemctl start fail2ban
sudo fail2ban-client status sshd
```

**Disable Root Login:**

```
sudo nano /etc/ssh/sshd_config

Set: PermitRootLogin no

sudo systemctl restart sshd
```

### Monitoring Setup

**Install and Configure Monitoring Tools:**

```
Install htop for resource monitoring:
sudo apt-get install -y htop

Install ncdu for disk usage analysis:
sudo apt-get install -y ncdu

Check system resources:
htop
free -h
df -h
```

**Set Up Log Rotation:**

```
Create logrotate configuration:
sudo nano /etc/logrotate.d/docker-containers

Add:
/var/lib/docker/containers/*/*.log {
  rotate 7
  daily
  compress
  missingok
  delaycompress
  copytruncate
}
```

### Complete Deployment Script

For automated deployment, you can create a script with all commands:

```
Create deployment script:
nano deploy.sh
```

Script content:

```
#!/bin/bash
set -e

echo "Starting Strikeball deployment..."

# Clone repository
echo "Cloning repository..."
git clone https://github.com/yourusername/Airsoft-Id.git /opt/strikeball
cd /opt/strikeball

# Create environment file
echo "Creating environment configuration..."
cp .env.example .env
echo "Please edit .env file with secure passwords"
nano .env

# Build frontend
echo "Building frontend..."
cd frontend
npm install
npm run build
cd ..

# Create directories
echo "Creating required directories..."
mkdir -p nginx/certs backups

# Start containers
echo "Starting Docker containers..."
docker compose up -d --build

# Wait for services
echo "Waiting for services to initialize..."
sleep 30

# Run migrations
echo "Running database migrations..."
docker compose exec -T api dotnet ef database update

# Verify deployment
echo "Verifying deployment..."
docker compose ps

echo "Deployment complete!"
echo "Access your application at: http://$(curl -s ifconfig.me)"
```

Make script executable and run:

```
chmod +x deploy.sh
sudo ./deploy.sh
```

## Production Deployment

This section covers deploying the application to production environment with full security and reliability measures.

### Step 1: Production Infrastructure Preparation

Prepare production infrastructure with enterprise-grade configuration.

**Server Provisioning**

Provision production servers according to requirements:

- Use dedicated physical servers or high-tier cloud instances
- Configure multiple servers for high availability if required
- Set up load balancing across multiple API instances if required
- Configure geographic redundancy if multi-region deployment is needed

**Network Infrastructure**

Configure production-grade networking:

- Set up Virtual Private Cloud or private network
- Configure network segmentation to isolate services
- Set up DDoS protection and WAF if available
- Configure CDN for static asset delivery
- Set up monitoring and alerting for network issues

**Storage Configuration**

Configure production storage solutions:

- Use SSD or NVMe storage for database volumes
- Configure RAID for data redundancy if on physical servers
- Set up network-attached storage for backups
- Configure storage encryption at rest
- Set up monitoring for storage capacity and performance

**Backup Infrastructure**

Establish comprehensive backup strategy:

- Configure automated daily database backups
- Set up off-site backup storage
- Implement backup encryption
- Define backup retention policies
- Document and test backup restoration procedures

### Step 2: Security Hardening

Implement production-grade security measures.

**Operating System Hardening**

Secure the operating system:

- Apply all security patches and updates
- Disable unnecessary services and ports
- Configure SELinux or AppArmor if available
- Set up system audit logging
- Configure automated security update installation
- Implement host-based intrusion detection

**Container Security**

Enhance Docker container security:

- Use minimal base images to reduce attack surface
- Scan images for vulnerabilities before deployment
- Run containers with non-root users where possible
- Implement container resource limits to prevent resource exhaustion
- Enable Docker content trust for image verification
- Configure Docker daemon with security best practices

**Network Security**

Implement network security controls:

- Configure firewall with strict rules allowing only necessary traffic
- Implement network intrusion detection system
- Set up VPN for administrative access
- Configure fail2ban or equivalent for brute force protection
- Enable DDoS protection mechanisms
- Implement rate limiting at network level

**Application Security**

Enhance application-level security:

- Configure strong JWT settings with short expiration times
- Implement API rate limiting and throttling
- Enable CORS with restrictive allowed origins
- Configure Content Security Policy headers
- Implement input validation and sanitization
- Enable SQL injection protection mechanisms
- Configure XSS protection headers

### Step 3: SSL Certificate Configuration

Configure production SSL certificates with enhanced security.

**Certificate Acquisition**

Obtain production SSL certificates:

- Use extended validation certificates for enhanced trust
- Obtain certificates from reputable certificate authority
- Configure automatic certificate renewal for Let's Encrypt
- Maintain backup certificates for failover scenarios

**Certificate Installation**

Install certificates with production best practices:

- Store certificates in secure location with restricted access
- Configure certificate files with minimal permissions
- Set up certificate monitoring and expiration alerts
- Document certificate renewal procedures

**SSL Configuration Optimization**

Configure Nginx with optimal SSL settings:

- Enable TLS 1.3 with TLS 1.2 fallback only
- Configure strong cipher suites prioritizing forward secrecy
- Enable OCSP stapling for certificate validation
- Configure HSTS with long max-age and includeSubDomains
- Implement certificate pinning if appropriate
- Disable SSL compression to prevent CRIME attacks
- Configure DHE parameters with 4096-bit keys

### Step 4: Production Configuration

Create production-specific configuration with maximum security.

**Environment Variables**

Configure production environment with secure values:

- Generate cryptographically random 32+ character passwords
- Use unique strong passwords for each service
- Generate 64+ character JWT secret key
- Configure production database name and credentials
- Set production domain and CORS origins
- Configure production API endpoints
- Set conservative resource limits and timeouts

**Secrets Management**

Implement secure secrets management:

- Store sensitive configuration in secure vault if available
- Never commit production secrets to version control
- Encrypt environment configuration file at rest
- Restrict access to configuration files
- Implement secret rotation procedures
- Document secret management procedures

**Application Configuration**

Configure production application settings:

- Set production logging levels
- Configure log aggregation and monitoring
- Set up error reporting and alerting
- Configure email service for notifications
- Set up payment gateway with production credentials
- Configure any third-party service integrations

### Step 5: Database Configuration

Configure production database with high availability and performance.

**Database Optimization**

Optimize PostgreSQL for production workload:

- Configure appropriate connection pool size
- Set memory allocation parameters based on available RAM
- Configure query performance logging
- Set up query plan optimization
- Configure appropriate checkpoint and WAL settings
- Enable query caching where beneficial

**Database Security**

Enhance database security:

- Use strong unique password for database user
- Restrict database network access to API containers only
- Enable database audit logging
- Configure row-level security policies if needed
- Implement database encryption at rest
- Set up database connection encryption with SSL

**Database Backup Configuration**

Configure comprehensive database backup:

- Schedule automated daily backups during low-traffic periods
- Configure backup retention matching business requirements
- Implement backup verification procedures
- Set up backup encryption with separate encryption keys
- Configure off-site backup replication
- Document restoration procedures with recovery time objectives

### Step 6: Monitoring and Logging

Implement comprehensive monitoring and logging infrastructure.

**Application Monitoring**

Set up application performance monitoring:

- Configure health check endpoints for all services
- Implement uptime monitoring with external service
- Monitor API response times and error rates
- Track user authentication and authorization events
- Monitor frontend performance metrics
- Set up real-time alerting for critical issues

**Infrastructure Monitoring**

Monitor infrastructure health and performance:

- Track CPU, memory, and disk utilization for all servers
- Monitor Docker container health and resource usage
- Track network bandwidth and latency
- Monitor database connection pool utilization
- Track Redis cache hit rates
- Set up disk space monitoring with alerts

**Log Management**

Implement centralized log management:

- Configure log aggregation from all services
- Set up log retention policies
- Implement log analysis and search capabilities
- Configure alerts for error patterns
- Enable audit logging for security events
- Document log review procedures

### Step 7: Production Deployment Execution

Execute production deployment with controlled rollout.

**Pre-Deployment Checklist**

Verify all prerequisites before deployment:

- All configuration files reviewed and validated
- SSL certificates installed and tested
- DNS records configured and propagated
- Monitoring and alerting configured
- Backup and restoration procedures documented and tested
- Rollback plan documented and understood
- Maintenance window scheduled and communicated

**Frontend Build**

Build production frontend with full optimization:

- Execute production build with all optimizations enabled
- Verify build completed without errors or warnings
- Validate bundle sizes are within acceptable limits
- Test built application locally before deployment
- Create backup of previous frontend build for rollback

**Database Migration**

Execute database migrations with safety measures:

- Create full database backup before migration
- Test migrations on copy of production database first
- Execute migrations during maintenance window
- Monitor migration progress and execution time
- Verify data integrity after migration
- Document migration completion

**Container Deployment**

Deploy application containers with controlled rollout:

- Pull and verify all Docker images
- Start database and cache services first
- Wait for database and cache health checks to pass
- Start backend API service
- Wait for API health checks to pass
- Deploy frontend by copying build artifacts to Nginx volume
- Start Nginx service
- Verify all services reach healthy state

**Smoke Testing**

Execute smoke tests immediately after deployment:

- Verify frontend application loads successfully
- Test user authentication flow
- Verify API endpoints respond correctly
- Test critical user workflows
- Verify external integrations function
- Check application logs for errors

### Step 8: Post-Deployment Verification

Conduct comprehensive verification of production deployment.

**Functional Verification**

Verify all application functionality:

- Test all user-facing features
- Verify data persistence and retrieval
- Test file upload and download
- Verify email notifications
- Test payment processing if applicable
- Verify map functionality and external integrations

**Performance Verification**

Verify production performance meets requirements:

- Measure API response times under normal load
- Measure frontend page load times
- Verify database query performance
- Check cache hit rates and effectiveness
- Monitor resource utilization under load
- Compare metrics against baseline expectations

**Security Verification**

Verify security measures are functioning:

- Verify HTTPS enforcement across all endpoints
- Test authentication and authorization flows
- Verify security headers present in all responses
- Test rate limiting functionality
- Verify CORS configuration restricts unauthorized origins
- Conduct vulnerability scan if tools available

**Monitoring Verification**

Verify monitoring systems are functioning:

- Confirm all health checks are passing
- Verify monitoring dashboards show current data
- Test alerting by simulating error conditions
- Verify logs are being collected and aggregated
- Confirm backup job executed successfully
- Review all system logs for warnings or errors

## Deployment Troubleshooting

This section provides guidance for resolving common deployment issues.

### Container Startup Issues

**Symptom**: Containers fail to start or immediately exit

**Diagnostic Approach**

Review container logs to identify the root cause. Docker provides detailed logs showing the output and errors from container processes.

**Common Causes and Resolutions**

Port Conflicts:
- Verify that configured ports are not in use by other processes
- Check for multiple container instances attempting to bind same ports
- Modify port mappings in environment configuration if needed

Configuration Errors:
- Review environment variables for typos or missing values
- Verify configuration file syntax and format
- Check that all required configuration parameters are present

Permission Issues:
- Verify Docker has permission to access mounted volumes
- Check file permissions on configuration files
- Ensure directories exist with appropriate ownership

Resource Constraints:
- Verify server has sufficient memory and CPU for all containers
- Check for out-of-memory errors in container logs
- Adjust resource limits in Docker Compose configuration

### Database Connection Issues

**Symptom**: API cannot connect to PostgreSQL database

**Diagnostic Approach**

Verify database service health and network connectivity. Check database logs and API logs for connection error details.

**Common Causes and Resolutions**

Database Not Ready:
- Wait for database health check to pass before starting API
- Increase health check timeout if database takes longer to initialize
- Verify depends_on conditions are properly configured

Connection String Errors:
- Verify database hostname matches service name in Docker network
- Check database port, username, and password are correct
- Verify database name exists or will be created automatically

Network Issues:
- Confirm both services are on same Docker network
- Verify Docker network created successfully
- Check for network isolation or firewall rules blocking connections

### Migration Failures

**Symptom**: Database migrations fail to execute

**Diagnostic Approach**

Review migration error messages and database state. Check for schema conflicts or permission issues.

**Common Causes and Resolutions**

Schema Conflicts:
- Check if manual database changes conflict with migrations
- Verify no database objects exist with names matching migration objects
- Review migration history table for incomplete migrations

Permission Issues:
- Verify database user has sufficient privileges for schema modifications
- Grant CREATE, ALTER, DROP permissions if needed
- Check ownership of database and schema objects

Migration Code Errors:
- Review migration code for syntax errors
- Test migrations against development database first
- Check for breaking changes in entity relationships

### SSL Certificate Issues

**Symptom**: HTTPS connections fail or show certificate errors

**Diagnostic Approach**

Verify certificate files exist, are properly formatted, and match the domain.

**Common Causes and Resolutions**

Certificate File Issues:
- Verify certificate files exist at configured paths
- Check file permissions allow Nginx to read certificates
- Validate certificate format and encoding

Certificate Validation Errors:
- Verify certificate chain is complete
- Check certificate is valid for the requested domain
- Ensure certificate is not expired
- Verify certificate is trusted by client browsers

Configuration Errors:
- Check Nginx SSL configuration syntax
- Verify certificate and key file paths are correct
- Ensure SSL protocols and ciphers are properly configured

### Performance Issues

**Symptom**: Application responds slowly or times out

**Diagnostic Approach**

Monitor resource utilization and identify bottlenecks. Review application logs for slow queries or operations.

**Common Causes and Resolutions**

Database Performance:
- Check for missing indexes on frequently queried columns
- Review slow query logs
- Verify database connection pool is properly sized
- Consider adding caching for frequently accessed data

Resource Constraints:
- Monitor CPU, memory, and disk utilization
- Increase container resource limits if needed
- Scale horizontally by adding more API instances
- Upgrade server hardware if consistently resource-constrained

Network Latency:
- Verify network bandwidth is sufficient
- Check for network congestion or packet loss
- Consider CDN for static asset delivery
- Optimize API response payload sizes

Cache Configuration:
- Verify Redis is functioning and accessible
- Check cache hit rates and tune caching strategy
- Increase cache memory allocation if needed
- Review cache expiration policies

## Maintenance Procedures

This section covers ongoing maintenance tasks for deployed application.

### Database Backup and Restoration

**Automated Backup Process**

The backup service container automatically performs daily database backups. Backups are stored as compressed SQL dumps in the backup volume with timestamp-based naming.

**Manual Backup Execution**

To perform manual backup outside scheduled interval:

- Execute pg_dump command through PostgreSQL container
- Specify database name, username, and output file path
- Compress backup file to save storage space
- Store backup in secure location with appropriate retention

**Backup Verification**

Regularly verify backup integrity:

- Check that backup files are created on schedule
- Verify backup file size is reasonable for database size
- Periodically test restoration to non-production environment
- Document restoration time to ensure meets recovery objectives

**Restoration Procedure**

To restore database from backup:

- Stop API container to prevent connection attempts during restoration
- Create backup of current database state before restoration
- Drop and recreate target database
- Execute restore command using psql through PostgreSQL container
- Verify data integrity after restoration
- Restart API container and verify application functionality

### Security Updates

**Operating System Updates**

Regularly apply security updates to host operating system:

- Configure automatic security update installation if supported
- Monitor security advisories for critical vulnerabilities
- Schedule maintenance windows for major updates requiring restart
- Test updates in staging environment before production

**Container Image Updates**

Keep Docker images updated with security patches:

- Regularly pull updated base images
- Rebuild application containers with updated dependencies
- Review changelogs for breaking changes
- Test updated containers in staging before production
- Update Docker Compose configuration with new image versions

**Application Dependency Updates**

Update application framework and library dependencies:

- Monitor security advisories for .NET, Angular, and Node.js
- Update dependencies in staging environment first
- Execute full test suite after updates
- Review breaking changes and update code if necessary
- Deploy updated application through standard deployment process

### Log Management

**Log Collection**

Application and infrastructure logs are collected from:

- Docker container stdout and stderr
- Application log files within containers
- System logs from host operating system
- Nginx access and error logs
- Database query logs if enabled

**Log Retention**

Implement log retention policies:

- Configure automatic log rotation to prevent disk space issues
- Archive older logs to long-term storage if required
- Delete logs older than retention period
- Adjust retention based on storage capacity and compliance requirements

**Log Analysis**

Regularly review logs for:

- Application errors and exceptions
- Security events and authentication failures
- Performance issues and slow queries
- Unusual access patterns
- Resource utilization trends

### Monitoring and Alerting

**Health Check Monitoring**

Continuously monitor application health:

- Verify all container health checks passing
- Monitor API endpoint availability
- Track database connection availability
- Monitor cache service availability
- Alert on health check failures

**Performance Monitoring**

Track performance metrics:

- Monitor API response times
- Track database query performance
- Monitor cache hit rates
- Track resource utilization trends
- Alert on performance degradation

**Capacity Planning**

Use monitoring data for capacity planning:

- Track growth trends for storage, CPU, and memory
- Predict when scaling will be required
- Plan infrastructure upgrades proactively
- Optimize resource allocation based on usage patterns

### Scaling Considerations

**Vertical Scaling**

Increase resources for existing containers:

- Upgrade server CPU and memory capacity
- Increase container resource limits
- Adjust database memory allocation
- Increase cache memory allocation

**Horizontal Scaling**

Add additional instances for increased capacity:

- Deploy multiple API container instances
- Configure load balancing across API instances
- Implement session persistence or stateless authentication
- Set up database read replicas for read-heavy workloads
- Consider database sharding for extremely large datasets

## Security Best Practices

### Authentication and Authorization

**JWT Configuration**

Configure JWT tokens with security best practices:

- Use cryptographically strong secret key minimum 64 characters
- Set short access token expiration (15-60 minutes)
- Implement refresh token rotation
- Store refresh tokens securely with HttpOnly cookies
- Implement token revocation mechanism for logout
- Use different secrets for different environments

**Password Policies**

Enforce strong password requirements:

- Require minimum 12 characters for user passwords
- Require mix of uppercase, lowercase, numbers, and symbols
- Implement password complexity validation
- Prevent use of common passwords
- Implement account lockout after failed attempts
- Require password change after first login

**Role-Based Access Control**

Implement proper authorization:

- Define clear role hierarchy and permissions
- Verify user roles on every protected endpoint
- Implement principle of least privilege
- Audit role assignments and permissions regularly
- Log authorization failures for security monitoring

### Data Protection

**Encryption at Rest**

Protect stored data:

- Enable database encryption for sensitive data
- Encrypt backup files before storage
- Use encrypted volumes for sensitive file storage
- Protect encryption keys with proper key management

**Encryption in Transit**

Protect data during transmission:

- Enforce HTTPS for all client-server communication
- Use TLS for all inter-service communication
- Configure database connections with SSL
- Verify certificate validation is enabled

**Sensitive Data Handling**

Properly handle sensitive information:

- Never log passwords or tokens in application logs
- Mask sensitive data in API responses
- Implement proper data sanitization
- Follow privacy regulations and compliance requirements

### Network Security

**Firewall Configuration**

Maintain restrictive firewall rules:

- Default deny all incoming traffic
- Allow only necessary ports for application services
- Restrict administrative access to known IP addresses
- Block suspicious traffic patterns automatically
- Regularly review and audit firewall rules

**Rate Limiting**

Protect against abuse and attacks:

- Implement rate limiting on authentication endpoints
- Rate limit API endpoints based on user or IP
- Implement progressive delays for repeated failures
- Block IP addresses with suspicious patterns
- Configure rate limits appropriate for legitimate usage

**DDoS Protection**

Protect against denial of service attacks:

- Use CDN with DDoS protection for public endpoints
- Implement connection limits at network level
- Configure request size limits
- Implement request throttling
- Use cloud provider DDoS protection services if available

## Rollback Procedures

### Identifying Need for Rollback

Determine when rollback is necessary:

- Critical functionality is completely broken
- Security vulnerability introduced in new deployment
- Severe performance degradation impacting users
- Database corruption or data loss risk identified
- High error rates indicating systemic issues

### Pre-Rollback Preparation

Before executing rollback:

- Document the issues requiring rollback
- Notify stakeholders of rollback decision
- Prepare communication for users if necessary
- Identify which components need rollback
- Verify previous version artifacts are available

### Application Rollback

**Frontend Rollback**

Restore previous frontend version:

- Locate backup of previous frontend build
- Stop Nginx container
- Replace frontend files in Nginx volume with previous version
- Restart Nginx container
- Verify frontend loads and displays correctly

**Backend Rollback**

Restore previous backend version:

- Locate previous Docker image version or source code
- Stop API container
- Deploy previous container image
- Verify API health check passes
- Test critical API endpoints

**Database Rollback**

Handle database schema rollback:

- Assess if database migrations need reversal
- Create backup of current database state
- Execute rollback migrations if available
- Manually revert schema changes if necessary
- Verify data integrity after rollback
- Test application against reverted schema

### Post-Rollback Verification

After rollback completion:

- Execute smoke tests to verify functionality
- Monitor error rates and logs
- Verify performance returns to normal levels
- Communicate rollback completion to stakeholders
- Document rollback execution for post-mortem analysis

### Root Cause Analysis

After successful rollback:

- Analyze logs and metrics to identify root cause
- Document what went wrong and why
- Identify what could prevent similar issues
- Update testing procedures to catch similar issues
- Plan corrective actions before next deployment attempt

## Disaster Recovery

### Backup Strategy

**Backup Components**

Maintain backups of all critical components:

- Database: Daily automated backups with 30-day retention
- Application configuration: Version controlled and securely stored
- SSL certificates: Securely backed up with documentation
- Docker images: Tagged and stored in image registry
- Frontend builds: Archived with version identifiers

**Backup Storage**

Store backups securely:

- Use geographically separate storage location
- Implement encryption for all backups
- Test backup accessibility regularly
- Document backup locations and access procedures
- Maintain backup retention policies

**Backup Testing**

Regularly test backup validity:

- Perform quarterly restoration tests
- Verify restored data integrity
- Document restoration time
- Update recovery procedures based on test results
- Train team on restoration procedures

### Recovery Procedures

**Complete System Recovery**

Restore entire system from scratch:

- Provision new infrastructure matching production specifications
- Install required software and dependencies
- Configure networking and security
- Restore database from latest backup
- Deploy application containers
- Restore configuration files
- Verify functionality before directing traffic

**Partial Recovery**

Recover specific components:

- Identify failed component
- Deploy replacement instance
- Restore data if necessary
- Update networking and load balancing
- Verify integration with other components

**Data Recovery**

Recover lost or corrupted data:

- Identify scope and timing of data loss
- Locate appropriate backup containing required data
- Restore backup to temporary environment
- Extract required data
- Validate data integrity
- Import data to production database
- Verify application functionality

### Recovery Time Objectives

Define recovery time objectives for different scenarios:

- Complete infrastructure loss: 4-8 hours
- Database corruption: 1-2 hours
- Single service failure: 15-30 minutes
- Configuration error: 5-15 minutes

### Business Continuity

**High Availability Configuration**

For critical production deployments:

- Deploy multiple API instances with load balancing
- Configure database replication with automatic failover
- Use managed database services with built-in HA
- Implement health checks with automatic recovery
- Configure geographic redundancy if required

**Failover Procedures**

Document and test failover procedures:

- Define criteria for initiating failover
- Document steps to redirect traffic
- Test failover in staging environment
- Train team on failover execution
- Define communication procedures during failover

## Compliance and Audit

### Logging Requirements

Maintain audit logs for:

- User authentication and authorization events
- Administrative actions and configuration changes
- Data access and modification
- Security events and failed access attempts
- System errors and exceptions

### Log Retention

Configure log retention meeting compliance requirements:

- Security logs: minimum 1 year retention
- Application logs: 90 days retention
- Access logs: 90 days retention
- Audit logs: per regulatory requirements
- Store archived logs in secure long-term storage

### Access Control Auditing

Regularly audit access controls:

- Review user accounts and permissions
- Verify role assignments are appropriate
- Audit administrative access
- Review API key and token usage
- Document access control reviews

### Security Auditing

Conduct regular security audits:

- Review security configurations quarterly
- Conduct vulnerability assessments
- Review security logs for suspicious activity
- Test incident response procedures
- Update security measures based on findings

## Conclusion

This deployment guide provides comprehensive instructions for deploying the Strikeball Social Network application across development, staging, and production environments. Following these procedures ensures secure, reliable, and maintainable deployments that meet operational requirements while maintaining high availability and performance standards.
- Confirm backup job executed successfully
- Review all system logs for warnings or errors

## Deployment Troubleshooting

This section provides guidance for resolving common deployment issues.

### Container Startup Issues

**Symptom**: Containers fail to start or immediately exit

**Diagnostic Approach**

Review container logs to identify the root cause. Docker provides detailed logs showing the output and errors from container processes.

**Common Causes and Resolutions**

Port Conflicts:
- Verify that configured ports are not in use by other processes
- Check for multiple container instances attempting to bind same ports
- Modify port mappings in environment configuration if needed

Configuration Errors:
- Review environment variables for typos or missing values
- Verify configuration file syntax and format
- Check that all required configuration parameters are present

Permission Issues:
- Verify Docker has permission to access mounted volumes
- Check file permissions on configuration files
- Ensure directories exist with appropriate ownership

Resource Constraints:
- Verify server has sufficient memory and CPU for all containers
- Check for out-of-memory errors in container logs
- Adjust resource limits in Docker Compose configuration

### Database Connection Issues

**Symptom**: API cannot connect to PostgreSQL database

**Diagnostic Approach**

Verify database service health and network connectivity. Check database logs and API logs for connection error details.

**Common Causes and Resolutions**

Database Not Ready:
- Wait for database health check to pass before starting API
- Increase health check timeout if database takes longer to initialize
- Verify depends_on conditions are properly configured

Connection String Errors:
- Verify database hostname matches service name in Docker network
- Check database port, username, and password are correct
- Verify database name exists or will be created automatically

Network Issues:
- Confirm both services are on same Docker network
- Verify Docker network created successfully
- Check for network isolation or firewall rules blocking connections

### Migration Failures

**Symptom**: Database migrations fail to execute

**Diagnostic Approach**

Review migration error messages and database state. Check for schema conflicts or permission issues.

**Common Causes and Resolutions**

Schema Conflicts:
- Check if manual database changes conflict with migrations
- Verify no database objects exist with names matching migration objects
- Review migration history table for incomplete migrations

Permission Issues:
- Verify database user has sufficient privileges for schema modifications
- Grant CREATE, ALTER, DROP permissions if needed
- Check ownership of database and schema objects

Migration Code Errors:
- Review migration code for syntax errors
- Test migrations against development database first
- Check for breaking changes in entity relationships

### SSL Certificate Issues

**Symptom**: HTTPS connections fail or show certificate errors

**Diagnostic Approach**

Verify certificate files exist, are properly formatted, and match the domain.

**Common Causes and Resolutions**

Certificate File Issues:
- Verify certificate files exist at configured paths
- Check file permissions allow Nginx to read certificates
- Validate certificate format and encoding

Certificate Validation Errors:
- Verify certificate chain is complete
- Check certificate is valid for the requested domain
- Ensure certificate is not expired
- Verify certificate is trusted by client browsers

Configuration Errors:
- Check Nginx SSL configuration syntax
- Verify certificate and key file paths are correct
- Ensure SSL protocols and ciphers are properly configured

### Performance Issues

**Symptom**: Application responds slowly or times out

**Diagnostic Approach**

Monitor resource utilization and identify bottlenecks. Review application logs for slow queries or operations.

**Common Causes and Resolutions**

Database Performance:
- Check for missing indexes on frequently queried columns
- Review slow query logs
- Verify database connection pool is properly sized
- Consider adding caching for frequently accessed data

Resource Constraints:
- Monitor CPU, memory, and disk utilization
- Increase container resource limits if needed
- Scale horizontally by adding more API instances
- Upgrade server hardware if consistently resource-constrained

Network Latency:
- Verify network bandwidth is sufficient
- Check for network congestion or packet loss
- Consider CDN for static asset delivery
- Optimize API response payload sizes

Cache Configuration:
- Verify Redis is functioning and accessible
- Check cache hit rates and tune caching strategy
- Increase cache memory allocation if needed
- Review cache expiration policies

## Maintenance Procedures

This section covers ongoing maintenance tasks for deployed application.

### Database Backup and Restoration

**Automated Backup Process**

The backup service container automatically performs daily database backups. Backups are stored as compressed SQL dumps in the backup volume with timestamp-based naming.

**Manual Backup Execution**

To perform manual backup outside scheduled interval:

- Execute pg_dump command through PostgreSQL container
- Specify database name, username, and output file path
- Compress backup file to save storage space
- Store backup in secure location with appropriate retention

**Backup Verification**

Regularly verify backup integrity:

- Check that backup files are created on schedule
- Verify backup file size is reasonable for database size
- Periodically test restoration to non-production environment
- Document restoration time to ensure meets recovery objectives

**Restoration Procedure**

To restore database from backup:

- Stop API container to prevent connection attempts during restoration
- Create backup of current database state before restoration
- Drop and recreate target database
- Execute restore command using psql through PostgreSQL container
- Verify data integrity after restoration
- Restart API container and verify application functionality

### Security Updates

**Operating System Updates**

Regularly apply security updates to host operating system:

- Configure automatic security update installation if supported
- Monitor security advisories for critical vulnerabilities
- Schedule maintenance windows for major updates requiring restart
- Test updates in staging environment before production

**Container Image Updates**

Keep Docker images updated with security patches:

- Regularly pull updated base images
- Rebuild application containers with updated dependencies
- Review changelogs for breaking changes
- Test updated containers in staging before production
- Update Docker Compose configuration with new image versions

**Application Dependency Updates**

Update application framework and library dependencies:

- Monitor security advisories for .NET, Angular, and Node.js
- Update dependencies in staging environment first
- Execute full test suite after updates
- Review breaking changes and update code if necessary
- Deploy updated application through standard deployment process

### Log Management

**Log Collection**

Application and infrastructure logs are collected from:

- Docker container stdout and stderr
- Application log files within containers
- System logs from host operating system
- Nginx access and error logs
- Database query logs if enabled

**Log Retention**

Implement log retention policies:

- Configure automatic log rotation to prevent disk space issues
- Archive older logs to long-term storage if required
- Delete logs older than retention period
- Adjust retention based on storage capacity and compliance requirements

**Log Analysis**

Regularly review logs for:

- Application errors and exceptions
- Security events and authentication failures
- Performance issues and slow queries
- Unusual access patterns
- Resource utilization trends

### Monitoring and Alerting

**Health Check Monitoring**

Continuously monitor application health:

- Verify all container health checks passing
- Monitor API endpoint availability
- Track database connection availability
- Monitor cache service availability
- Alert on health check failures

**Performance Monitoring**

Track performance metrics:

- Monitor API response times
- Track database query performance
- Monitor cache hit rates
- Track resource utilization trends
- Alert on performance degradation

**Capacity Planning**

Use monitoring data for capacity planning:

- Track growth trends for storage, CPU, and memory
- Predict when scaling will be required
- Plan infrastructure upgrades proactively
- Optimize resource allocation based on usage patterns

### Scaling Considerations

**Vertical Scaling**

Increase resources for existing containers:

- Upgrade server CPU and memory capacity
- Increase container resource limits
- Adjust database memory allocation
- Increase cache memory allocation

**Horizontal Scaling**

Add additional instances for increased capacity:

- Deploy multiple API container instances
- Configure load balancing across API instances
- Implement session persistence or stateless authentication
- Set up database read replicas for read-heavy workloads
- Consider database sharding for extremely large datasets

## Security Best Practices

### Authentication and Authorization

**JWT Configuration**

Configure JWT tokens with security best practices:

- Use cryptographically strong secret key minimum 64 characters
- Set short access token expiration (15-60 minutes)
- Implement refresh token rotation
- Store refresh tokens securely with HttpOnly cookies
- Implement token revocation mechanism for logout
- Use different secrets for different environments

**Password Policies**

Enforce strong password requirements:

- Require minimum 12 characters for user passwords
- Require mix of uppercase, lowercase, numbers, and symbols
- Implement password complexity validation
- Prevent use of common passwords
- Implement account lockout after failed attempts
- Require password change after first login

**Role-Based Access Control**

Implement proper authorization:

- Define clear role hierarchy and permissions
- Verify user roles on every protected endpoint
- Implement principle of least privilege
- Audit role assignments and permissions regularly
- Log authorization failures for security monitoring

### Data Protection

**Encryption at Rest**

Protect stored data:

- Enable database encryption for sensitive data
- Encrypt backup files before storage
- Use encrypted volumes for sensitive file storage
- Protect encryption keys with proper key management

**Encryption in Transit**

Protect data during transmission:

- Enforce HTTPS for all client-server communication
- Use TLS for all inter-service communication
- Configure database connections with SSL
- Verify certificate validation is enabled

**Sensitive Data Handling**

Properly handle sensitive information:

- Never log passwords or tokens in application logs
- Mask sensitive data in API responses
- Implement proper data sanitization
- Follow privacy regulations and compliance requirements

### Network Security

**Firewall Configuration**

Maintain restrictive firewall rules:

- Default deny all incoming traffic
- Allow only necessary ports for application services
- Restrict administrative access to known IP addresses
- Block suspicious traffic patterns automatically
- Regularly review and audit firewall rules

**Rate Limiting**

Protect against abuse and attacks:

- Implement rate limiting on authentication endpoints
- Rate limit API endpoints based on user or IP
- Implement progressive delays for repeated failures
- Block IP addresses with suspicious patterns
- Configure rate limits appropriate for legitimate usage

**DDoS Protection**

Protect against denial of service attacks:

- Use CDN with DDoS protection for public endpoints
- Implement connection limits at network level
- Configure request size limits
- Implement request throttling
- Use cloud provider DDoS protection services if available

## Rollback Procedures

### Identifying Need for Rollback

Determine when rollback is necessary:

- Critical functionality is completely broken
- Security vulnerability introduced in new deployment
- Severe performance degradation impacting users
- Database corruption or data loss risk identified
- High error rates indicating systemic issues

### Pre-Rollback Preparation

Before executing rollback:

- Document the issues requiring rollback
- Notify stakeholders of rollback decision
- Prepare communication for users if necessary
- Identify which components need rollback
- Verify previous version artifacts are available

### Application Rollback

**Frontend Rollback**

Restore previous frontend version:

- Locate backup of previous frontend build
- Stop Nginx container
- Replace frontend files in Nginx volume with previous version
- Restart Nginx container
- Verify frontend loads and displays correctly

**Backend Rollback**

Restore previous backend version:

- Locate previous Docker image version or source code
- Stop API container
- Deploy previous container image
- Verify API health check passes
- Test critical API endpoints

**Database Rollback**

Handle database schema rollback:

- Assess if database migrations need reversal
- Create backup of current database state
- Execute rollback migrations if available
- Manually revert schema changes if necessary
- Verify data integrity after rollback
- Test application against reverted schema

### Post-Rollback Verification

After rollback completion:

- Execute smoke tests to verify functionality
- Monitor error rates and logs
- Verify performance returns to normal levels
- Communicate rollback completion to stakeholders
- Document rollback execution for post-mortem analysis

### Root Cause Analysis

After successful rollback:

- Analyze logs and metrics to identify root cause
- Document what went wrong and why
- Identify what could prevent similar issues
- Update testing procedures to catch similar issues
- Plan corrective actions before next deployment attempt

## Disaster Recovery

### Backup Strategy

**Backup Components**

Maintain backups of all critical components:

- Database: Daily automated backups with 30-day retention
- Application configuration: Version controlled and securely stored
- SSL certificates: Securely backed up with documentation
- Docker images: Tagged and stored in image registry
- Frontend builds: Archived with version identifiers

**Backup Storage**

Store backups securely:

- Use geographically separate storage location
- Implement encryption for all backups
- Test backup accessibility regularly
- Document backup locations and access procedures
- Maintain backup retention policies

**Backup Testing**

Regularly test backup validity:

- Perform quarterly restoration tests
- Verify restored data integrity
- Document restoration time
- Update recovery procedures based on test results
- Train team on restoration procedures

### Recovery Procedures

**Complete System Recovery**

Restore entire system from scratch:

- Provision new infrastructure matching production specifications
- Install required software and dependencies
- Configure networking and security
- Restore database from latest backup
- Deploy application containers
- Restore configuration files
- Verify functionality before directing traffic

**Partial Recovery**

Recover specific components:

- Identify failed component
- Deploy replacement instance
- Restore data if necessary
- Update networking and load balancing
- Verify integration with other components

**Data Recovery**

Recover lost or corrupted data:

- Identify scope and timing of data loss
- Locate appropriate backup containing required data
- Restore backup to temporary environment
- Extract required data
- Validate data integrity
- Import data to production database
- Verify application functionality

### Recovery Time Objectives

Define recovery time objectives for different scenarios:

- Complete infrastructure loss: 4-8 hours
- Database corruption: 1-2 hours
- Single service failure: 15-30 minutes
- Configuration error: 5-15 minutes

### Business Continuity

**High Availability Configuration**

For critical production deployments:

- Deploy multiple API instances with load balancing
- Configure database replication with automatic failover
- Use managed database services with built-in HA
- Implement health checks with automatic recovery
- Configure geographic redundancy if required

**Failover Procedures**

Document and test failover procedures:

- Define criteria for initiating failover
- Document steps to redirect traffic
- Test failover in staging environment
- Train team on failover execution
- Define communication procedures during failover

## Compliance and Audit

### Logging Requirements

Maintain audit logs for:

- User authentication and authorization events
- Administrative actions and configuration changes
- Data access and modification
- Security events and failed access attempts
- System errors and exceptions

### Log Retention

Configure log retention meeting compliance requirements:

- Security logs: minimum 1 year retention
- Application logs: 90 days retention
- Access logs: 90 days retention
- Audit logs: per regulatory requirements
- Store archived logs in secure long-term storage

### Access Control Auditing

Regularly audit access controls:

- Review user accounts and permissions
- Verify role assignments are appropriate
- Audit administrative access
- Review API key and token usage
- Document access control reviews

### Security Auditing

Conduct regular security audits:

- Review security configurations quarterly
- Conduct vulnerability assessments
- Review security logs for suspicious activity
- Test incident response procedures
- Update security measures based on findings

## Conclusion

This deployment guide provides comprehensive instructions for deploying the Strikeball Social Network application across development, staging, and production environments. Following these procedures ensures secure, reliable, and maintainable deployments that meet operational requirements while maintaining high availability and performance standards.
