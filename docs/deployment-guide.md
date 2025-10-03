# Deployment Guide

This guide covers deploying LiftTracker to various environments including Azure, Docker, and traditional hosting.

## Table of Contents

- [Prerequisites](#prerequisites)
- [Azure Deployment](#azure-deployment)
- [Docker Deployment](#docker-deployment)
- [Traditional Hosting](#traditional-hosting)
- [Environment Configuration](#environment-configuration)
- [Database Setup](#database-setup)
- [Monitoring & Logging](#monitoring--logging)
- [Troubleshooting](#troubleshooting)

## Prerequisites

### Required Tools
- **.NET 8 SDK** for building the application
- **Azure CLI** for Azure deployments
- **Docker** for containerized deployments
- **Terraform** for infrastructure as code (optional)

### Required Services
- **SQL Server** database (Azure SQL Database recommended for production)
- **Google OAuth 2.0** credentials for authentication
- **SSL Certificate** for HTTPS (Azure provides free certificates)

## Azure Deployment

### Option 1: Azure App Service (Recommended)

#### 1. Prepare Infrastructure

Using Azure CLI:
```bash
# Create resource group
az group create --name rg-lifttracker-prod --location "East US"

# Create App Service Plan
az appservice plan create \
  --name plan-lifttracker-prod \
  --resource-group rg-lifttracker-prod \
  --sku B1 \
  --is-linux

# Create Web App for API
az webapp create \
  --name lifttracker-api-prod \
  --resource-group rg-lifttracker-prod \
  --plan plan-lifttracker-prod \
  --runtime "DOTNETCORE:8.0"

# Create Static Web App for Client
az staticwebapp create \
  --name lifttracker-client-prod \
  --resource-group rg-lifttracker-prod \
  --source https://github.com/your-username/lift-tracker \
  --branch main \
  --app-location "src/LiftTracker.Client" \
  --api-location "src/LiftTracker.API" \
  --output-location "wwwroot"
```

#### 2. Database Setup

Create Azure SQL Database:
```bash
# Create SQL Server
az sql server create \
  --name lifttracker-sql-prod \
  --resource-group rg-lifttracker-prod \
  --location "East US" \
  --admin-user sqladmin \
  --admin-password "YourSecurePassword123!"

# Create Database
az sql db create \
  --name LiftTrackerDb \
  --server lifttracker-sql-prod \
  --resource-group rg-lifttracker-prod \
  --service-objective Basic

# Configure firewall (allow Azure services)
az sql server firewall-rule create \
  --name AllowAzureServices \
  --server lifttracker-sql-prod \
  --resource-group rg-lifttracker-prod \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0
```

#### 3. Configure Application Settings

```bash
# Set connection string
az webapp config connection-string set \
  --name lifttracker-api-prod \
  --resource-group rg-lifttracker-prod \
  --connection-string-type SQLAzure \
  --settings DefaultConnection="Server=tcp:lifttracker-sql-prod.database.windows.net,1433;Database=LiftTrackerDb;User ID=sqladmin;Password=YourSecurePassword123!;Encrypt=true;Connection Timeout=30;"

# Set app settings
az webapp config appsettings set \
  --name lifttracker-api-prod \
  --resource-group rg-lifttracker-prod \
  --settings \
    ASPNETCORE_ENVIRONMENT=Production \
    Authentication__Google__ClientId="your-google-client-id" \
    Authentication__Google__ClientSecret="your-google-client-secret" \
    Logging__LogLevel__Default=Information \
    Caching__DefaultExpiration=00:30:00
```

#### 4. Deploy Application

Using GitHub Actions (recommended):

Create `.github/workflows/deploy.yml`:
```yaml
name: Deploy to Azure

on:
  push:
    branches: [ main ]

jobs:
  deploy:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '8.0.x'
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Build
      run: dotnet build --no-restore --configuration Release
    
    - name: Test
      run: dotnet test --no-build --configuration Release
    
    - name: Publish API
      run: dotnet publish src/LiftTracker.API -c Release -o api-publish
    
    - name: Deploy to Azure App Service
      uses: azure/webapps-deploy@v2
      with:
        app-name: lifttracker-api-prod
        publish-profile: ${{ secrets.AZURE_WEBAPP_PUBLISH_PROFILE }}
        package: api-publish
```

### Option 2: Azure Container Instances

#### 1. Create Docker Images

API Dockerfile (`src/LiftTracker.API/Dockerfile`):
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/LiftTracker.API/LiftTracker.API.csproj", "src/LiftTracker.API/"]
COPY ["src/LiftTracker.Application/LiftTracker.Application.csproj", "src/LiftTracker.Application/"]
COPY ["src/LiftTracker.Domain/LiftTracker.Domain.csproj", "src/LiftTracker.Domain/"]
COPY ["src/LiftTracker.Infrastructure/LiftTracker.Infrastructure.csproj", "src/LiftTracker.Infrastructure/"]
RUN dotnet restore "src/LiftTracker.API/LiftTracker.API.csproj"
COPY . .
WORKDIR "/src/src/LiftTracker.API"
RUN dotnet build "LiftTracker.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LiftTracker.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LiftTracker.API.dll"]
```

#### 2. Deploy Container

```bash
# Build and push to Azure Container Registry
az acr create --name lifttrackeracr --resource-group rg-lifttracker-prod --sku Basic --admin-enabled true

# Get ACR credentials
az acr credential show --name lifttrackeracr

# Build and push
docker build -t lifttrackeracr.azurecr.io/lifttracker-api:latest .
docker push lifttrackeracr.azurecr.io/lifttracker-api:latest

# Create container instance
az container create \
  --name lifttracker-api \
  --resource-group rg-lifttracker-prod \
  --image lifttrackeracr.azurecr.io/lifttracker-api:latest \
  --registry-login-server lifttrackeracr.azurecr.io \
  --registry-username lifttrackeracr \
  --registry-password "your-acr-password" \
  --dns-name-label lifttracker-api \
  --ports 80 443 \
  --environment-variables \
    ASPNETCORE_ENVIRONMENT=Production \
    ConnectionStrings__DefaultConnection="your-connection-string"
```

## Docker Deployment

### Docker Compose Setup

Create `docker-compose.yml`:
```yaml
version: '3.8'

services:
  api:
    build:
      context: .
      dockerfile: src/LiftTracker.API/Dockerfile
    ports:
      - "7001:80"
      - "7002:443"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Server=db;Database=LiftTrackerDb;User Id=sa;Password=YourPassword123!;TrustServerCertificate=true;
      - Authentication__Google__ClientId=your-google-client-id
      - Authentication__Google__ClientSecret=your-google-client-secret
    depends_on:
      - db
    volumes:
      - ./https:/https:ro

  client:
    build:
      context: .
      dockerfile: src/LiftTracker.Client/Dockerfile
    ports:
      - "5001:80"
      - "5002:443"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
    volumes:
      - ./https:/https:ro

  db:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourPassword123!
      - MSSQL_PID=Express
    ports:
      - "1433:1433"
    volumes:
      - sqlserver_data:/var/opt/mssql

volumes:
  sqlserver_data:
```

Client Dockerfile (`src/LiftTracker.Client/Dockerfile`):
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/LiftTracker.Client/LiftTracker.Client.csproj", "src/LiftTracker.Client/"]
RUN dotnet restore "src/LiftTracker.Client/LiftTracker.Client.csproj"
COPY . .
WORKDIR "/src/src/LiftTracker.Client"
RUN dotnet build "LiftTracker.Client.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LiftTracker.Client.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LiftTracker.Client.dll"]
```

### Run with Docker Compose

```bash
# Build and start services
docker-compose up -d

# Check status
docker-compose ps

# View logs
docker-compose logs -f api
docker-compose logs -f client

# Stop services
docker-compose down
```

## Traditional Hosting

### IIS Deployment

#### 1. Prerequisites

- **Windows Server** with IIS installed
- **.NET 8 Hosting Bundle** installed
- **SQL Server** accessible from the server

#### 2. Prepare Application

```bash
# Publish the API
dotnet publish src/LiftTracker.API -c Release -o publish/api

# Publish the Client
dotnet publish src/LiftTracker.Client -c Release -o publish/client
```

#### 3. IIS Configuration

1. **Create IIS Sites**:
   - API Site: Port 7001, Physical Path: `C:\inetpub\lifttracker\api`
   - Client Site: Port 5001, Physical Path: `C:\inetpub\lifttracker\client`

2. **Configure Application Pool**:
   - Set .NET CLR Version to "No Managed Code"
   - Set Identity to appropriate service account

3. **Copy Files**:
   ```cmd
   xcopy publish\api C:\inetpub\lifttracker\api /E /I
   xcopy publish\client C:\inetpub\lifttracker\client /E /I
   ```

#### 4. Web.config for API

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <location path="." inheritInChildApplications="false">
    <system.webServer>
      <handlers>
        <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
      </handlers>
      <aspNetCore processPath="dotnet" 
                  arguments=".\LiftTracker.API.dll" 
                  stdoutLogEnabled="false" 
                  stdoutLogFile=".\logs\stdout" 
                  hostingModel="inprocess" />
    </system.webServer>
  </location>
</configuration>
```

### Linux Deployment (Ubuntu/CentOS)

#### 1. Install Prerequisites

Ubuntu:
```bash
# Install .NET 8
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt-get update
sudo apt-get install -y aspnetcore-runtime-8.0

# Install Nginx
sudo apt-get install nginx
```

#### 2. Create Service Files

API Service (`/etc/systemd/system/lifttracker-api.service`):
```ini
[Unit]
Description=LiftTracker API
After=network.target

[Service]
Type=notify
ExecStart=/usr/bin/dotnet /var/www/lifttracker/api/LiftTracker.API.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=lifttracker-api
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment="ConnectionStrings__DefaultConnection=your-connection-string"
WorkingDirectory=/var/www/lifttracker/api

[Install]
WantedBy=multi-user.target
```

#### 3. Nginx Configuration

`/etc/nginx/sites-available/lifttracker`:
```nginx
server {
    listen 80;
    server_name your-domain.com;
    return 301 https://$server_name$request_uri;
}

server {
    listen 443 ssl http2;
    server_name your-domain.com;

    ssl_certificate /path/to/your/certificate.crt;
    ssl_certificate_key /path/to/your/private.key;

    # Client (Blazor WebAssembly)
    location / {
        root /var/www/lifttracker/client/wwwroot;
        try_files $uri $uri/ /index.html;
        
        # Enable compression
        gzip on;
        gzip_types text/plain text/css application/json application/javascript text/xml application/xml application/xml+rss text/javascript;
    }

    # API
    location /api/ {
        proxy_pass http://localhost:5000/api/;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
    }
}
```

#### 4. Start Services

```bash
# Enable and start API service
sudo systemctl enable lifttracker-api
sudo systemctl start lifttracker-api

# Enable Nginx
sudo ln -s /etc/nginx/sites-available/lifttracker /etc/nginx/sites-enabled/
sudo systemctl reload nginx
```

## Environment Configuration

### Production Settings

#### appsettings.Production.json
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    },
    "Serilog": {
      "MinimumLevel": "Information",
      "WriteTo": [
        {
          "Name": "File",
          "Args": {
            "path": "/var/log/lifttracker/api-.log",
            "rollingInterval": "Day",
            "retainedFileCountLimit": 30,
            "formatter": "Serilog.Formatting.Json.JsonFormatter"
          }
        }
      ]
    }
  },
  "AllowedHosts": "your-domain.com",
  "Authentication": {
    "Google": {
      "ClientId": "${GOOGLE_CLIENT_ID}",
      "ClientSecret": "${GOOGLE_CLIENT_SECRET}"
    }
  },
  "Caching": {
    "DefaultExpiration": "00:30:00",
    "UserCacheExpiration": "01:00:00",
    "ProgressCacheExpiration": "00:15:00"
  },
  "Performance": {
    "SlowRequestThreshold": 1000,
    "EnableDetailedLogging": false
  }
}
```

### Environment Variables

Essential environment variables for production:

```bash
# Required
export ASPNETCORE_ENVIRONMENT=Production
export ConnectionStrings__DefaultConnection="your-sql-connection-string"
export Authentication__Google__ClientId="your-google-client-id"
export Authentication__Google__ClientSecret="your-google-client-secret"

# Optional
export Logging__LogLevel__Default=Information
export Caching__DefaultExpiration=00:30:00
export Performance__SlowRequestThreshold=1000
```

## Database Setup

### Azure SQL Database

#### 1. Connection String Format
```
Server=tcp:your-server.database.windows.net,1433;Database=LiftTrackerDb;User ID=your-username;Password=your-password;Encrypt=true;TrustServerCertificate=false;Connection Timeout=30;
```

#### 2. Run Migrations
```bash
# Set connection string
export ConnectionStrings__DefaultConnection="your-azure-sql-connection-string"

# Run migrations
dotnet ef database update --project src/LiftTracker.Infrastructure --startup-project src/LiftTracker.API
```

### SQL Server On-Premises

#### 1. Create Database
```sql
CREATE DATABASE LiftTrackerDb;
GO

-- Create application user
CREATE LOGIN lifttracker_user WITH PASSWORD = 'YourSecurePassword123!';
GO

USE LiftTrackerDb;
GO

CREATE USER lifttracker_user FOR LOGIN lifttracker_user;
GO

-- Grant permissions
ALTER ROLE db_datareader ADD MEMBER lifttracker_user;
ALTER ROLE db_datawriter ADD MEMBER lifttracker_user;
ALTER ROLE db_ddladmin ADD MEMBER lifttracker_user;
GO
```

#### 2. Connection String
```
Server=your-server;Database=LiftTrackerDb;User Id=lifttracker_user;Password=YourSecurePassword123!;TrustServerCertificate=true;
```

## Monitoring & Logging

### Application Insights (Azure)

#### 1. Setup Application Insights
```bash
# Create Application Insights resource
az monitor app-insights component create \
  --app lifttracker-insights \
  --location "East US" \
  --resource-group rg-lifttracker-prod \
  --application-type web
```

#### 2. Configure in Application
```json
{
  "ApplicationInsights": {
    "ConnectionString": "InstrumentationKey=your-instrumentation-key;IngestionEndpoint=https://eastus-8.in.applicationinsights.azure.com/;LiveEndpoint=https://eastus.livediagnostics.monitor.azure.com/"
  }
}
```

### Serilog Configuration

Structured logging setup in `Program.cs`:
```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "LiftTracker.API")
    .WriteTo.Console()
    .WriteTo.File("logs/app-.log", 
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        formatter: new JsonFormatter())
    .WriteTo.ApplicationInsights(
        serviceProvider.GetRequiredService<TelemetryConfiguration>(),
        TelemetryConverter.Traces)
    .CreateLogger();
```

### Health Checks

Monitor application health at `/health`:
- Database connectivity
- External service availability
- Memory usage
- Disk space

## Troubleshooting

### Common Deployment Issues

#### Database Connection Failures
```bash
# Test connection string
dotnet ef database list --project src/LiftTracker.Infrastructure --startup-project src/LiftTracker.API

# Check firewall rules
az sql server firewall-rule list --server your-server --resource-group your-rg
```

#### Authentication Issues
- Verify Google OAuth redirect URLs include your production domain
- Check client ID and secret are correctly set
- Ensure HTTPS is properly configured

#### Performance Issues
- Monitor metrics at `/api/performance/metrics`
- Check database query performance
- Review cache hit rates
- Monitor memory usage

#### SSL Certificate Issues
```bash
# Test certificate
openssl s_client -connect your-domain.com:443 -servername your-domain.com

# Verify chain
curl -I https://your-domain.com
```

### Log Analysis

#### Common Log Patterns

**Slow Requests:**
```
2025-10-02 10:15:30 [Warning] Slow request detected: GET /api/progress/user/123 took 1250ms
```

**Authentication Failures:**
```
2025-10-02 10:16:45 [Error] Authentication failed for user: invalid_token
```

**Database Errors:**
```
2025-10-02 10:17:20 [Error] Database operation failed: timeout expired
```

### Performance Monitoring

Monitor these key metrics:
- **Response Time**: API endpoints < 500ms average
- **Database Performance**: Query execution < 100ms average
- **Cache Hit Rate**: > 80% for frequently accessed data
- **Error Rate**: < 1% of total requests
- **Memory Usage**: < 80% of available memory

## Security Considerations

### Production Checklist

- [ ] HTTPS enforced for all traffic
- [ ] Security headers configured (HSTS, CSP, etc.)
- [ ] Database credentials stored securely
- [ ] Google OAuth redirect URLs restricted to production domain
- [ ] Sensitive data encrypted at rest
- [ ] Regular security updates applied
- [ ] Access logging enabled
- [ ] Rate limiting configured
- [ ] Input validation enabled at all layers
- [ ] Error messages don't expose sensitive information

### Backup Strategy

#### Database Backups
```bash
# Azure SQL Database automated backups are enabled by default
# Manual backup export
az sql db export \
  --admin-password "YourPassword123!" \
  --admin-user sqladmin \
  --storage-key "your-storage-key" \
  --storage-key-type StorageAccessKey \
  --storage-uri "https://yourstorageaccount.blob.core.windows.net/backups/backup.bacpac" \
  --name LiftTrackerDb \
  --resource-group rg-lifttracker-prod \
  --server lifttracker-sql-prod
```

#### Application Backups
- Source code in version control (Git)
- Configuration in Azure Key Vault or environment variables
- Static assets in Azure Storage or CDN
- Regular testing of restoration procedures

---

*This deployment guide is continuously updated. Check the latest version in the repository.*
