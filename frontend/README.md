# Strikeball Social Network - Frontend

Angular-based web application for the Strikeball Social Network platform.

## Prerequisites

- Node.js 18+ 
- npm or yarn
- Angular CLI 17+

## Installation

1. **Install Node.js** (if not already installed)
   - Download from https://nodejs.org/
   - Verify: `node --version` and `npm --version`

2. **Install Angular CLI globally**:
   ```bash
   npm install -g @angular/cli
   ```

3. **Install project dependencies**:
   ```bash
   cd frontend
   npm install
   ```

## Development server

Run `ng serve` for a dev server. Navigate to `http://localhost:4200/`. The application will automatically reload if you change any of the source files.

```bash
npm start
# or
ng serve
```

## Build

Run `ng build` to build the project. The build artifacts will be stored in the `dist/` directory.

```bash
npm run build
# or
ng build --configuration production
```

## Project Structure

```
frontend/
├── src/
│   ├── app/
│   │   ├── core/              # Core services, guards, interceptors
│   │   ├── shared/            # Shared components, pipes, directives
│   │   ├── features/          # Feature modules
│   │   │   ├── auth/         # Authentication feature
│   │   │   ├── games/        # Games management
│   │   │   ├── teams/        # Teams management
│   │   │   ├── fields/       # Fields management
│   │   │   └── profile/      # User profile
│   │   ├── app.component.*   # Root component
│   │   ├── app.routes.ts     # Application routes
│   │   └── app.config.ts     # Application configuration
│   ├── assets/               # Static assets
│   ├── environments/         # Environment configurations
│   └── styles.scss          # Global styles
├── angular.json             # Angular CLI configuration
├── package.json             # Dependencies
└── tsconfig.json           # TypeScript configuration
```

## Features to be Implemented

- [ ] Authentication (login, register, logout)
- [ ] Player profile management
- [ ] Team management
- [ ] Game browsing and registration
- [ ] Field information with maps
- [ ] Violation tracking
- [ ] Dashboard with statistics

## Tech Stack

- **Framework**: Angular 17 (standalone components)
- **UI Library**: Angular Material
- **State Management**: Signals (Angular 17+)
- **HTTP Client**: Angular HttpClient with interceptors
- **Routing**: Angular Router with lazy loading
- **Maps**: Leaflet + OpenStreetMap
- **Styling**: SCSS

## Available Scripts

- `npm start` - Start development server
- `npm run build` - Build for production
- `npm test` - Run unit tests
- `npm run watch` - Build in watch mode
- `ng generate component [name]` - Generate a new component
- `ng generate service [name]` - Generate a new service

## Environment Configuration

The application uses environment files for configuration:

- `environment.development.ts` - Development environment (http://localhost:5000/api)
- `environment.ts` - Production environment (/api)

## API Integration

The frontend connects to the ASP.NET Core backend API at:
- Development: `http://localhost:5000/api`
- Production: `/api` (proxied through Nginx)

## Next Steps

1. Install dependencies: `npm install`
2. Implement authentication service and components
3. Create feature modules (games, teams, fields, profile)
4. Integrate Angular Material components
5. Add Leaflet maps for field locations
6. Implement guards for protected routes
7. Add form validation and error handling

## Notes

- This project uses Angular 17's standalone components (no NgModule)
- HTTP interceptors handle JWT token authentication
- Lazy loading is used for feature modules to optimize bundle size
- All dependencies listed in package.json need to be installed with `npm install`
