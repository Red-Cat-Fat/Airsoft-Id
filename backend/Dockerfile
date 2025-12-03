# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["src/StrikeballApi.WebApi/StrikeballApi.WebApi.csproj", "src/StrikeballApi.WebApi/"]
COPY ["src/StrikeballApi.Infrastructure/StrikeballApi.Infrastructure.csproj", "src/StrikeballApi.Infrastructure/"]
COPY ["src/StrikeballApi.Domain/StrikeballApi.Domain.csproj", "src/StrikeballApi.Domain/"]

RUN dotnet restore "src/StrikeballApi.WebApi/StrikeballApi.WebApi.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/src/StrikeballApi.WebApi"
RUN dotnet build "StrikeballApi.WebApi.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "StrikeballApi.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

EXPOSE 5000

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "StrikeballApi.WebApi.dll"]
