# Installation Guide - Strikeball Social Network

This guide will help you set up the development environment for the Strikeball Social Network project.

## Step 1: Install .NET 8.0 SDK

### Windows

1. **Download .NET 8.0 SDK**
   - Visit: https://dotnet.microsoft.com/download/dotnet/8.0
   - Download the "SDK x64" installer for Windows
   - File will be named something like: `dotnet-sdk-8.0.xxx-win-x64.exe`

2. **Run the Installer**
   - Double-click the downloaded `.exe` file
   - Follow the installation wizard
   - Accept the license terms
   - Click "Install"

3. **Verify Installation**
   Open a new PowerShell or Command Prompt window and run:
   ```bash
   dotnet --version
   ```
   You should see output like: `8.0.xxx`

### Linux (Ubuntu/Debian)

```bash
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

sudo apt-get update
sudo apt-get install -y dotnet-sdk-8.0

# Verify installation
dotnet --version
```

### macOS

```bash
# Using Homebrew
brew install --cask dotnet-sdk

# Verify installation
dotnet --version
```

## Step 2: Install Node.js and npm

### Windows

1. **Download Node.js LTS**
   - Visit: https://nodejs.org/
   - Download the LTS version (18.x or higher)
   - Run the installer and follow the wizard

2. **Verify Installation**
   ```bash
   node --version
   npm --version
   ```

### Linux (Ubuntu/Debian)

```bash
curl -fsSL https://deb.nodesource.com/setup_18.x | sudo -E bash -
sudo apt-get install -y nodejs

# Verify installation
node --version
npm --version
```

### macOS

```bash
brew install node

# Verify installation
node --version
npm --version
```

## Step 3: Install Angular CLI

```bash
npm install -g @angular/cli

# Verify installation
ng version
```

## Step 4: Install Docker Desktop

### Windows

1. **Download Docker Desktop**
   - Visit: https://www.docker.com/products/docker-desktop
   - Download Docker Desktop for Windows
   - Requires Windows 10/11 Pro, Enterprise, or Education with Hyper-V enabled

2. **Install Docker Desktop**
   - Run the installer
   - Follow the installation wizard
   - Restart your computer when prompted

3. **Verify Installation**
   ```bash
   docker --version
   docker-compose --version
   ```

### Linux

```bash
# Install Docker
curl -fsSL https://get.docker.com -o get-docker.sh
sudo sh get-docker.sh

# Add your user to docker group
sudo usermod -aG docker $USER

# Install Docker Compose
sudo curl -L "https://github.com/docker/compose/releases/latest/download/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
sudo chmod +x /usr/local/bin/docker-compose

# Log out and log back in for group changes to take effect

# Verify installation
docker --version
docker-compose --version
```

### macOS

```bash
# Download and install Docker Desktop from:
# https://www.docker.com/products/docker-desktop

# Or using Homebrew
brew install --cask docker

# Verify installation
docker --version
docker-compose --version
```

## Step 5: Install Git

### Windows

1. Download from: https://git-scm.com/download/win
2. Run the installer with default settings

### Linux

```bash
sudo apt-get update
sudo apt-get install git
```

### macOS

```bash
brew install git
```

## Step 6: Install PostgreSQL (Optional - for local development without Docker)

### Windows

1. Download from: https://www.postgresql.org/download/windows/
2. Run the installer
3. Remember the password you set for the `postgres` user
4. Default port is 5432

### Linux

```bash
sudo apt-get update
sudo apt-get install postgresql postgresql-contrib

# Start PostgreSQL service
sudo systemctl start postgresql
sudo systemctl enable postgresql
```

### macOS

```bash
brew install postgresql@15
brew services start postgresql@15
```

## Step 7: Install Recommended IDEs

### For Backend (.NET Development)

**Visual Studio 2022 (Windows)**
- Download from: https://visualstudio.microsoft.com/downloads/
- Install the "ASP.NET and web development" workload

**Visual Studio Code (Cross-platform)**
- Download from: https://code.visualstudio.com/
- Install extensions:
  - C# (Microsoft)
  - C# Dev Kit (Microsoft)
  - Docker (Microsoft)

**JetBrains Rider (Cross-platform, Paid)**
- Download from: https://www.jetbrains.com/rider/

### For Frontend (Angular Development)

**Visual Studio Code**
- Install extensions:
  - Angular Language Service
  - Angular Snippets
  - ESLint
  - Prettier

**WebStorm (Paid)**
- Download from: https://www.jetbrains.com/webstorm/

## Step 8: Clone and Setup Project

```bash
# Clone the repository (if you haven't already)
cd e:\Projects\Web\Airsoft-Id

# Copy environment file
copy .env.example .env

# Edit .env and set your passwords
# (Use notepad, VS Code, or any text editor)
```

## Step 9: Build and Run

### Option A: Using Docker (Recommended)

```bash
# Start all services
docker-compose up -d

# Check if services are running
docker-compose ps

# View logs
docker-compose logs -f
```

### Option B: Local Development

```bash
# Start PostgreSQL and Redis (make sure they're running)

# Run the backend API
cd backend/src/StrikeballApi.WebApi
dotnet restore
dotnet ef database update
dotnet run

# In a new terminal, run the frontend (when available)
cd frontend
npm install
ng serve
```

## Troubleshooting

### .NET SDK not found after installation

- **Windows**: Restart your terminal or computer
- **Linux/macOS**: Make sure `~/.dotnet` is in your PATH

### Docker permission denied (Linux)

```bash
sudo usermod -aG docker $USER
# Log out and log back in
```

### PostgreSQL connection refused

- Check if PostgreSQL is running:
  ```bash
  # Windows
  Get-Service postgresql*
  
  # Linux
  sudo systemctl status postgresql
  
  # macOS
  brew services list
  ```

### Port already in use

- Check what's using the port:
  ```bash
  # Windows
  netstat -ano | findstr :5000
  
  # Linux/macOS
  lsof -i :5000
  ```

## Next Steps

Once you have everything installed:

1. Read the [README.md](README.md) for project overview
2. Check the [API Documentation](http://localhost:5000/swagger) when API is running
3. Review the design document in `.qoder/quests/strikeball-social-network.md`
4. Start developing!

## Need Help?

- Open an issue on GitHub
- Check the project documentation
- Review the design document
