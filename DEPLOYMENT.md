# Deployment Guide

Instructions for deploying the Enterprise Project Management System to production environments.

## Pre-Deployment Checklist

- [ ] All tests passing
- [ ] Code reviewed and merged
- [ ] Environment configuration prepared
- [ ] Database backup created
- [ ] SSL certificate installed
- [ ] Database migrations tested
- [ ] Security configurations reviewed

## Backend Deployment

### Deploy to Azure App Service

1. **Create Resource Group and App Service Plan:**
   ```bash
   az group create --name MyResourceGroup --location eastus
   az appservice plan create --name MyAppServicePlan --resource-group MyResourceGroup --sku FREE
   ```

2. **Create Web App:**
   ```bash
   az webapp create --resource-group MyResourceGroup --plan MyAppServicePlan --name MyProjectAPI
   ```

3. **Configure Connection String:**
   ```bash
   az webapp config appsettings set --resource-group MyResourceGroup --name MyProjectAPI \
     --settings ConnectionStrings__DefaultConnection="Server=tcp:enterprise-pm.database.windows.net,1433;Initial Catalog=EnterpriseDb;Persist Security Info=False;User ID=enterpriseadmin;Password=EnterpriseDB@P4ssw0rd2025!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
   ```

4. **Deploy using Git:**
   ```bash
   git remote add azure <AZURE_GIT_DEPLOY_URL>
   git push azure main
   ```

### Deploy to AWS EC2

1. **Launch EC2 Instance with Windows Server**

2. **Connect to instance and install:**
   - .NET 9.0 SDK
   - SQL Server Express or RDS
   - IIS (optional)

3. **Deploy application:**
   ```bash
   git clone <repository-url>
   cd backend
   dotnet publish -c Release -o ./publish
   ```

4. **Configure IIS:**
   - Install ASP.NET Core Runtime
   - Create new website pointing to publish folder
   - Configure application pool

### Deploy to Docker

1. **Create Dockerfile in backend root:**

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet build -c Release -o /app/build

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/build .
EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000
ENTRYPOINT ["dotnet", "backend.dll"]
```

2. **Build Docker image:**
   ```bash
   docker build -t enterprise-project-api:latest .
   ```

3. **Run container:**
   ```bash
   docker run -e ConnectionStrings__DefaultConnection="..." -p 5000:5000 enterprise-project-api:latest
   ```

## Frontend Deployment

### Deploy to Vercel

1. **Connect GitHub repository**
2. **Configure environment variables:**
   ```
   REACT_APP_API_URL=https://enterprise-pm-api.azurewebsites.net
   ```
3. **Vercel automatically builds and deploys on push**

### Deploy to Netlify

1. **Connect GitHub repository**
2. **Configure build settings:**
   - Build command: `npm run build`
   - Publish directory: `build`
3. **Set environment variables:**
   ```
   REACT_APP_API_URL=https://enterprise-pm-api.azurewebsites.net
   ```

### Deploy to GitHub Pages

1. **Update `package.json`:**
   ```json
   "homepage": "https://yourusername.github.io/enterprise-project-management"
   ```

2. **Install gh-pages:**
   ```bash
   npm install --save-dev gh-pages
   ```

3. **Update `package.json` scripts:**
   ```json
   {
     "predeploy": "npm run build",
     "deploy": "gh-pages -d build"
   }
   ```

4. **Deploy:**
   ```bash
   npm run deploy
   ```

### Deploy to Azure Static Web Apps

1. **Create Static Web App in Azure Portal**
2. **Connect GitHub repository**
3. **Configure build settings:**
   - Build preset: Create React App
   - App location: `frontend`
   - Build output location: `build`
4. **Set environment variables for your API URL**

## Database Deployment

### SQL Server Azure

1. **Create SQL Database in Azure**
2. **Configure firewall rules**
3. **Update connection string**
4. **Run migrations:**
   ```bash
   dotnet ef database update
   ```

### SQL Server on EC2

1. **Install SQL Server on EC2**
2. **Restore backup or initialize database**
3. **Configure SQL Server for network access**
4. **Update connection string**

## Environment Configuration

### Production appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning"
    }
  },
  "AllowedHosts": "*.yourdomain.com",
  "ConnectionStrings": {
    "DefaultConnection": "Your-Production-Connection-String"
  },
  "Jwt": {
    "Key": "Your-Secure-Production-Key-Min-32-Characters",
    "ExpireMinutes": 60
  }
}
```

### Production Environment Variables

```bash
# Frontend
REACT_APP_API_URL=https://api.yourdomain.com
NODE_ENV=production

# Backend
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=Your-Connection-String
```

## Security Best Practices

1. **HTTPS Only**
   - Install SSL certificate
   - Redirect HTTP to HTTPS
   - Set HSTS headers

2. **API Security**
   - Implement authentication (JWT)
   - Add rate limiting
   - Validate all inputs
   - Use CORS appropriately

3. **Database Security**
   - Encrypt connection strings
   - Use parameterized queries (EF Core does this)
   - Regular backups
   - Principle of least privilege for DB user

4. **Frontend Security**
   - Keep dependencies updated
   - Remove sensitive data from code
   - Enable Content Security Policy

## Monitoring

### Application Insights (Azure)

```csharp
// In Program.cs
builder.Services.AddApplicationInsightsTelemetry();
```

### Logging

Configure centralized logging:
- Serilog
- Application Insights
- CloudWatch (AWS)
- Stackdriver (GCP)

## CI/CD Pipeline

### GitHub Actions Example

Create `.github/workflows/deploy.yml`:

```yaml
name: Deploy

on:
  push:
    branches: [main]

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v2
      - name: Deploy to Azure
        run: |
          dotnet publish -c Release
          # Deploy commands
```

## Rollback Procedures

1. **Keep previous versions available**
2. **Database migration rollback:**
   ```bash
   dotnet ef database update PreviousMigration
   ```
3. **Keep backup of previous deployment**
4. **Have rollback plan documented**

## Performance Optimization

### Backend
- Enable compression
- Configure caching headers
- Optimize database queries
- Use CDN for static files

### Frontend
- Minification and bundling
- Enable gzip compression
- Lazy load components
- Use CDN for static assets

## Maintenance

- Regular security updates
- Monitor application logs
- Track performance metrics
- Regular database maintenance
- Keep dependencies updated

## Support

For deployment issues, refer to:
- TROUBLESHOOTING.md
- Azure Documentation
- AWS Documentation
- Docker Documentation
