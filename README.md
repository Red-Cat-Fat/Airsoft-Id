# Strikeball Social Network

A comprehensive web-based social network platform for airsoft (strikeball) enthusiasts that enables community members to connect, organize games, track their history, and manage team activities.

## 🎯 Features

- **Player Profiles**: Manage airsofter profiles with callsigns, equipment, and game history
- **Team Management**: Create and manage teams with members, logos, and statistics
- **Game Organization**: Schedule and manage airsoft games with registration
- **Field Management**: Maintain field information with maps, rules, and locations
- **Organizer System**: Role-based permissions for game and event management
- **Violation Tracking**: Record and manage rule violations for players and teams
- **Authentication**: Secure JWT-based authentication with role-based access control

## 🛠 Technology Stack

### Backend
- **Framework**: ASP.NET Core 8.0 Web API
- **Language**: C#
- **Architecture**: Clean Architecture with CQRS pattern
- **ORM**: Entity Framework Core
- **Database**: PostgreSQL 15
- **Caching**: Redis
- **Authentication**: ASP.NET Core Identity with JWT
- **API Documentation**: Swagger/OpenAPI

### Frontend
- **Framework**: Angular (Latest LTS)
- **Language**: TypeScript
- **UI Library**: Angular Material / PrimeNG
- **State Management**: NgRx / Akita
- **Maps**: Leaflet with OpenStreetMap

### Mobile
- **Framework**: Ionic Framework with Angular
- **Platforms**: iOS and Android
- **Code Sharing**: Shared components and services with web app

### Infrastructure
- **Containerization**: Docker & Docker Compose
- **Reverse Proxy**: Nginx
- **Version Control**: Git

## 📋 Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 18+](https://nodejs.org/)
- [Angular CLI](https://angular.io/cli)
- [Docker](https://www.docker.com/get-started) and [Docker Compose](https://docs.docker.com/compose/install/)
- [PostgreSQL 15](https://www.postgresql.org/download/) (for local development without Docker)

## 🚀 Quick Start

### 1. Clone the Repository

```bash
git clone https://github.com/yourusername/strikeball-social-network.git
cd strikeball-social-network
```

### 2. Configure Environment Variables

Copy the example environment file and update with your values:

```bash
cp .env.example .env
```

Edit `.env` and set secure passwords and secrets:

```env
POSTGRES_PASSWORD=YourSecurePassword
REDIS_PASSWORD=YourSecureRedisPassword
JWT_SECRET_KEY=YourSuperSecretJWTKey
```

### 3. Run with Docker Compose

The easiest way to get started is using Docker Compose, which will start all services:

```bash
docker-compose up -d
```

This will start:
- PostgreSQL database (port 5432)
- Redis cache (port 6379)
- ASP.NET Core API (port 5000)
- Nginx reverse proxy (ports 80, 443)
- Automated backup service

### 4. Run Database Migrations

After the containers are running, apply database migrations:

```bash
docker-compose exec api dotnet ef database update
```

### 5. Access the Application

- **API**: http://localhost:5000
- **Swagger Documentation**: http://localhost:5000/swagger
- **Frontend** (when built): http://localhost:80

## 🔧 Development Setup

### Backend Development (Without Docker)

1. **Install .NET 8.0 SDK**

2. **Set up PostgreSQL** and create a database:
   ```sql
   CREATE DATABASE strikeballdb;
   ```

3. **Update connection string** in `backend/src/StrikeballApi.WebApi/appsettings.Development.json`

4. **Run migrations**:
   ```bash
   cd backend/src/StrikeballApi.WebApi
   dotnet ef database update
   ```

5. **Run the API**:
   ```bash
   dotnet run
   ```

The API will be available at `https://localhost:5001` or `http://localhost:5000`

### Frontend Development

1. **Install Node.js and Angular CLI**:
   ```bash
   npm install -g @angular/cli
   ```

2. **Navigate to frontend directory** (to be created):
   ```bash
   cd frontend
   ```

3. **Install dependencies**:
   ```bash
   npm install
   ```

4. **Run the development server**:
   ```bash
   ng serve
   ```

The Angular app will be available at `http://localhost:4200`

## 📁 Project Structure

```
strikeball-social-network/
├── backend/
│   ├── src/
│   │   ├── StrikeballApi.Domain/         # Domain entities and enums
│   │   ├── StrikeballApi.Application/    # Business logic (to be added)
│   │   ├── StrikeballApi.Infrastructure/ # Data access and external services
│   │   └── StrikeballApi.WebApi/         # API controllers and configuration
│   ├── tests/                            # Unit and integration tests
│   └── Dockerfile                        # Docker configuration for API
├── frontend/                             # Angular web application (to be added)
├── mobile/                               # Ionic mobile app (to be added)
├── nginx/                                # Nginx configuration
│   ├── conf.d/
│   │   └── default.conf                  # Server configuration
│   └── certs/                            # SSL certificates
├── scripts/                              # Utility scripts
├── docker-compose.yml                    # Docker Compose configuration
├── .env.example                          # Environment variables template
├── .gitignore                            # Git ignore rules
└── README.md                             # This file
```

## 🗄 Database Schema

The application uses the following main entities:

- **Airsofters**: Player profiles with personal information and equipment
- **Teams**: Team information with members and statistics
- **Fields**: Game field locations with maps and rules
- **Games**: Scheduled and completed games
- **Organizers**: Users with elevated permissions
- **Violations**: Rule violation records
- **GameParticipants**: Many-to-many relationship between games and players
- **OrganizerFieldAuthorizations**: Organizer permissions for specific fields
- **FieldBannedTeams**: Teams banned from specific fields

## 🔐 Authentication & Authorization

The application uses JWT-based authentication with the following roles:

- **Guest**: Unauthenticated users (view public content)
- **Airsofter**: Registered players
- **Team Leader**: Team administrators
- **Organizer**: Event managers
- **Field Owner**: Field administrators
- **Administrator**: Platform administrators

## 🐳 Docker Commands

### Start all services
```bash
docker-compose up -d
```

### Stop all services
```bash
docker-compose down
```

### View logs
```bash
docker-compose logs -f [service-name]
# Example: docker-compose logs -f api
```

### Rebuild and restart a service
```bash
docker-compose up -d --build api
```

### Access a running container
```bash
docker-compose exec api bash
```

### Database backup (manual)
```bash
docker-compose exec postgres pg_dump -U postgres strikeballdb > backup.sql
```

### Restore database
```bash
docker-compose exec -T postgres psql -U postgres strikeballdb < backup.sql
```

## 📊 API Endpoints

### Authentication
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - User login
- `POST /api/auth/refresh-token` - Refresh access token
- `POST /api/auth/logout` - User logout

### Airsofters
- `GET /api/airsofters` - Get all airsofters
- `GET /api/airsofters/{id}` - Get airsofter by ID
- `POST /api/airsofters` - Create new airsofter
- `PUT /api/airsofters/{id}` - Update airsofter
- `DELETE /api/airsofters/{id}` - Delete airsofter

### Teams
- `GET /api/teams` - Get all teams
- `GET /api/teams/{id}` - Get team by ID
- `POST /api/teams` - Create new team
- `PUT /api/teams/{id}` - Update team
- `DELETE /api/teams/{id}` - Delete team

### Games
- `GET /api/games` - Get all games
- `GET /api/games/{id}` - Get game by ID
- `POST /api/games` - Create new game
- `PUT /api/games/{id}` - Update game
- `DELETE /api/games/{id}` - Delete game
- `POST /api/games/{id}/register` - Register for game
- `DELETE /api/games/{id}/unregister` - Unregister from game

### Fields
- `GET /api/fields` - Get all fields
- `GET /api/fields/{id}` - Get field by ID
- `POST /api/fields` - Create new field
- `PUT /api/fields/{id}` - Update field
- `DELETE /api/fields/{id}` - Delete field

For complete API documentation, visit `/swagger` when the API is running.

## 🧪 Testing

### Run Backend Tests
```bash
cd backend
dotnet test
```

### Run Frontend Tests
```bash
cd frontend
ng test
```

## 🚢 Deployment

### Production Deployment Steps

1. **Update environment variables** in `.env` with production values
2. **Configure SSL certificates** in `nginx/certs/`
3. **Update Nginx configuration** for your domain
4. **Build and start services**:
   ```bash
   docker-compose -f docker-compose.yml up -d
   ```
5. **Run database migrations**:
   ```bash
   docker-compose exec api dotnet ef database update
   ```
6. **Configure firewall** to allow ports 80 and 443

### Monitoring

Access logs:
```bash
docker-compose logs -f
```

Check service health:
```bash
curl http://localhost/health
```

## 📝 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 👥 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## 📧 Support

For support and questions, please open an issue in the GitHub repository.

## 🗺 Roadmap

- [x] Project structure and architecture
- [x] Domain entities and database schema
- [x] Docker Compose configuration
- [ ] Authentication API endpoints
- [ ] CRUD operations for all entities
- [ ] Angular frontend application
- [ ] Ionic mobile application
- [ ] Map integration with Leaflet
- [ ] Real-time notifications
- [ ] Payment integration
- [ ] Advanced analytics and statistics

## 🙏 Acknowledgments

- Built with ASP.NET Core and Angular
- PostgreSQL for robust data storage
- Redis for high-performance caching
- Docker for containerization
- Nginx for reverse proxy and load balancing
