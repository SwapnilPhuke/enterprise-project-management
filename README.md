# Enterprise Project Management System

[![CI](https://github.com/SwapnilPhuke/enterprise-project-management/actions/workflows/ci.yml/badge.svg)](https://github.com/SwapnilPhuke/enterprise-project-management/actions/workflows/ci.yml)
[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![React](https://img.shields.io/badge/React-18.2-61DAFB?logo=react)](https://reactjs.org/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen.svg)](CONTRIBUTING.md)

A full-stack, production-ready web application for managing enterprise projects — built with **.NET 9** and **React 18**.

## ✨ Features

| Feature | Details |
|---|---|
| **JWT Authentication** | Register / login, BCrypt-hashed passwords; short-lived access tokens (15 min) |
| **Refresh Token Rotation** | 7-day refresh tokens stored server-side; every refresh issues a new pair |
| **Role-based Access Control** | `User` / `Admin` roles; `AdminOnly` policy guards `/api/v1/admin/*` endpoints |
| **Project CRUD** | Full create / read / update / delete scoped to the authenticated user |
| **Pagination & Filtering** | Server-side paging, keyword search, status filter, and sort on all project lists |
| **Stats & Dashboard** | Aggregated project stats cached in-memory; React dashboard with metric cards |
| **File Attachments** | Upload, download, and delete files per project (PDF, DOCX, XLSX, images, ZIP) |
| **API Versioning** | All routes versioned under `/api/v1` via `Asp.Versioning.Mvc` |
| **Rate Limiting** | 100 requests / minute per IP via `System.Threading.RateLimiting` |
| **Structured Logging** | Serilog with console + rolling-file sinks; separate log file per day |
| **AutoMapper** | Zero-boilerplate DTO mapping via `MappingProfile` |
| **FluentValidation** | Declarative request validation with descriptive error messages |
| **API Documentation** | Swagger UI with Bearer token support (Development only) |
| **Global Error Handling** | Consistent JSON error responses; never leaks internals in production |
| **Docker Ready** | Single `docker-compose up` spins up API + frontend + SQL Server |
| **CI/CD** | GitHub Actions pipeline — builds and tests both services on every push |

## 🛠️ Tech Stack

### Backend
| | |
|---|---|
| Framework | .NET 9 / ASP.NET Core Web API |
| ORM | Entity Framework Core 9 |
| Database | SQL Server (SQL Express / Azure SQL) |
| Auth | JWT Bearer (15 min) + Refresh Token Rotation (7 days) + BCrypt.Net |
| Mapping | AutoMapper 12 |
| Validation | FluentValidation 11 |
| Logging | Serilog (console + rolling file) |
| Versioning | Asp.Versioning.Mvc 8 — all routes under `/api/v1` |
| Rate Limiting | System.Threading.RateLimiting — 100 req/min/IP |
| Caching | `IMemoryCache` — stats cached 5 min, invalidated on writes |
| File Storage | Local `wwwroot/uploads`; extension + 10 MB size validation |
| API Docs | Swashbuckle / Swagger (dev only) |
| Architecture | Layered — Controller → Service → EF Core |

### Frontend
| | |
|---|---|
| Library | React 18 + Hooks |
| Routing | React Router DOM v6 with protected routes |
| State | Context API (`AuthProvider`) + `useState` / `useEffect` |
| HTTP | Axios — 401 interceptor auto-refreshes tokens |
| Tests | React Testing Library + Jest |
| Build | Create React App |

## 📋 Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (or Docker)
- [Node.js 20+](https://nodejs.org/)
- [Docker & Docker Compose](https://docs.docker.com/get-docker/) *(optional, for Docker setup)*

## 🚀 Quick Start

### Option A — Docker (recommended)

```bash
# 1. Clone the repository
git clone https://github.com/SwapnilPhuke/enterprise-project-management.git
cd enterprise-project-management

# 2. Set required environment variables
cp .env.example .env          # edit DB_PASSWORD and JWT_SECRET_KEY

# 3. Start all services
docker-compose up --build
```

| Service | URL |
|---|---|
| Frontend | http://localhost:3000 |
| API | http://localhost:8080 |
| Swagger | http://localhost:8080/swagger *(dev mode only)* |

---

### Option B — Manual Setup

#### Backend

```bash
cd backend

# Restore packages
dotnet restore

# Update appsettings.json — set a strong JwtSettings:SecretKey
# Then run migrations
dotnet ef database update

# Start the API
dotnet run
# → http://localhost:5000
```

#### Frontend

```bash
cd frontend

# Install dependencies
npm install

# Copy and edit environment variables
cp .env.example .env
# REACT_APP_API_URL=http://localhost:5000

# Start dev server
npm start
# → http://localhost:3000
```

## 📚 API Reference

All endpoints are versioned under `/api/v1`. Project and attachment endpoints require a `Bearer` token in the `Authorization` header.

### Auth

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| POST | `/api/v1/auth/register` | ✗ | Create a new account |
| POST | `/api/v1/auth/login` | ✗ | Authenticate — returns `token` + `refreshToken` + `role` |
| POST | `/api/v1/auth/refresh` | ✗ | Rotate refresh token — returns new `token` + `refreshToken` |
| POST | `/api/v1/auth/logout` | ✓ | Revoke the current refresh token |
| GET | `/api/v1/auth/me` | ✓ | Get the current user's profile and role |

### Projects

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| GET | `/api/v1/projects` | ✓ | List projects — supports `page`, `pageSize`, `search`, `status`, `sortBy` |
| GET | `/api/v1/projects/{id}` | ✓ | Get a specific project |
| GET | `/api/v1/projects/stats` | ✓ | Aggregated stats for the current user (cached) |
| POST | `/api/v1/projects` | ✓ | Create a project |
| PUT | `/api/v1/projects/{id}` | ✓ | Update a project |
| DELETE | `/api/v1/projects/{id}` | ✓ | Delete a project |

### Attachments

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| GET | `/api/v1/projects/{id}/attachments` | ✓ | List attachments for a project |
| POST | `/api/v1/projects/{id}/attachments` | ✓ | Upload a file (`multipart/form-data`) |
| GET | `/api/v1/projects/{id}/attachments/{fileId}` | ✓ | Download a file |
| DELETE | `/api/v1/projects/{id}/attachments/{fileId}` | ✓ | Delete a file |

### Admin *(requires `Admin` role)*

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/v1/admin/users` | List all users (paginated + searchable) |
| PUT | `/api/v1/admin/users/{id}/toggle-active` | Enable / disable a user account |
| PUT | `/api/v1/admin/users/{id}/role` | Change a user's role |

## 🗂️ Project Structure

```
enterprise-project-management/
├── .github/
│   └── workflows/
│       └── ci.yml                          # GitHub Actions CI pipeline
├── backend/
│   ├── Controllers/                        # HTTP layer (versioned /api/v1)
│   │   ├── AdminController.cs              # Admin-only user management
│   │   ├── AttachmentController.cs         # File upload / download
│   │   ├── AuthController.cs               # Register, login, refresh, logout
│   │   └── ProjectController.cs            # CRUD + stats
│   ├── Data/
│   │   └── AppDbContext.cs                 # EF Core DbContext + index config
│   ├── DTOs/                               # Request / response contracts
│   │   ├── AttachmentResponseDto.cs
│   │   ├── AuthDto.cs
│   │   ├── PagedResult.cs                  # Generic paged wrapper
│   │   ├── ProjectDto.cs
│   │   ├── ProjectQueryParams.cs           # Pagination + filter params
│   │   ├── ProjectResponseDto.cs
│   │   └── ProjectStatsDto.cs
│   ├── Mappings/
│   │   └── MappingProfile.cs               # AutoMapper profile
│   ├── Middleware/
│   │   └── GlobalExceptionHandlerMiddleware.cs
│   ├── Migrations/                         # EF Core migrations
│   ├── Models/                             # Domain entities
│   │   ├── Project.cs
│   │   ├── ProjectAttachment.cs
│   │   ├── ProjectStatus.cs                # Status enum (0-100)
│   │   ├── RefreshToken.cs
│   │   └── User.cs
│   ├── Services/                           # Business logic
│   │   ├── AuthenticationService.cs
│   │   ├── FileUploadService.cs
│   │   ├── IFileUploadService.cs
│   │   ├── IProjectService.cs
│   │   └── ProjectService.cs
│   ├── Validators/                         # FluentValidation validators
│   │   ├── AuthRequestValidator.cs
│   │   └── ProjectDtoValidator.cs
│   ├── Dockerfile
│   ├── Program.cs
│   ├── appsettings.json
│   └── appsettings.Production.json
├── frontend/
│   ├── public/
│   ├── src/
│   │   ├── context/
│   │   │   └── AuthContext.js              # Global auth state + 401 interceptor
│   │   ├── components/
│   │   │   ├── Navbar.js                   # Top nav with role-aware links
│   │   │   └── ProtectedRoute.js           # Route guard (auth + role)
│   │   ├── pages/
│   │   │   ├── DashboardPage.js            # Stats dashboard
│   │   │   ├── LoginPage.js
│   │   │   ├── ProjectsPage.js             # CRUD + attachments UI
│   │   │   └── RegisterPage.js
│   │   ├── App.js                          # React Router shell
│   │   ├── App.test.js                     # Jest / RTL test suite
│   │   ├── App.css
│   │   ├── index.css                       # Global styles
│   │   └── index.js
│   ├── Dockerfile
│   ├── .env.example
│   └── package.json
├── docker-compose.yml
├── .gitignore
├── ARCHITECTURE.md
├── CONTRIBUTING.md
├── DEPLOYMENT.md
├── QUICKSTART.md
├── TESTING.md
├── TROUBLESHOOTING.md
└── README.md
```

## 🔐 Environment Configuration

### Backend `appsettings.json`
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=EnterpriseDb;Integrated Security=true;Encrypt=false;"
  },
  "JwtSettings": {
    "SecretKey": "<min-32-char-secret>",
    "Issuer": "ProjectManagementAPI",
    "Audience": "ProjectManagementApp",
    "ExpirationMinutes": 15,
    "RefreshTokenExpireDays": 7
  },
  "Cors": {
    "AllowedOrigins": ["http://localhost:3000"]
  }
}
```

### Frontend `.env`
```
REACT_APP_API_URL=http://localhost:5000
```

## 🧪 Testing

```bash
# Frontend unit + integration tests
cd frontend
npm test -- --watchAll=false

# Backend (add xUnit project to run)
cd backend
dotnet test
```

## 🐳 Docker

```bash
# Build and start all services
docker-compose up --build

# Run in detached mode
docker-compose up -d

# Stop all services
docker-compose down

# Remove volumes (wipes database)
docker-compose down -v
```

## 🚢 Deployment

### Azure App Service (Backend)
```bash
az webapp config appsettings set \
  --resource-group MyRG --name MyApp \
  --settings \
    "ConnectionStrings__DefaultConnection=Server=tcp:..." \
    "JwtSettings__SecretKey=<strong-secret>" \
    "Cors__AllowedOrigins__0=https://enterprise-pm.vercel.app"
```

### Vercel / Netlify (Frontend)
```
REACT_APP_API_URL=https://enterprise-pm-api.azurewebsites.net
npm run build   # deploy the build/ folder
```

See [DEPLOYMENT.md](DEPLOYMENT.md) for full deployment guides (Azure, AWS, Docker).

## 🤝 Contributing

Contributions are welcome! Please read [CONTRIBUTING.md](CONTRIBUTING.md) first.

## 📝 License

This project is licensed under the MIT License — see [LICENSE](LICENSE) for details.

## 👤 Author

**Swapnil Phuke**
- GitHub: [@SwapnilPhuke](https://github.com/SwapnilPhuke)
- LinkedIn: [linkedin.com/in/swapnil-phuke-6a9430136](https://www.linkedin.com/in/swapnil-phuke-6a9430136)
- Email: phukeswapnil7@gmail.com

---

> ⭐ If you found this project useful, please consider giving it a star!
