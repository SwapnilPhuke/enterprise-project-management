# Architecture

## Overview

This is a full-stack web application built with a clear separation of concerns and modern development patterns. The architecture follows industry best practices for scalability, maintainability, and testability.

## Backend Architecture

### Project Structure

```
backend/
├── Controllers/          # API endpoints and request handling
├── Services/             # Business logic layer
├── Models/               # Entity models (Database)
├── Data/                 # Data access layer (DbContext)
├── DTOs/                 # Data Transfer Objects for API contracts
├── Migrations/           # Entity Framework Core migrations
├── Program.cs            # Application configuration and startup
└── appsettings.json      # Configuration settings
```

### Layered Architecture

1. **Controller Layer**: Handles HTTP requests and responses
   - `ProjectController`: Manages project-related endpoints
   - Implements error handling and logging

2. **Service Layer**: Contains business logic
   - `IProjectService`: Service interface
   - `ProjectService`: Service implementation
   - Handles data validation and business rules

3. **Data Access Layer**: Manages database operations
   - `AppDbContext`: Entity Framework DbContext
   - Uses Entity Framework Core ORM

4. **Model Layer**: Represents business entities
   - `Project`: Project entity with validation attributes

5. **DTO Layer**: Defines API contracts
   - `ProjectDto`: For creating projects
   - `ProjectUpdateDto`: For updating projects

### Design Patterns Used

- **Repository Pattern**: Service interfaces abstract data access
- **Dependency Injection**: Services are injected into controllers
- **Data Transfer Objects**: DTOs separate API contracts from models
- **Entity Framework Core**: ORM for database operations

## Frontend Architecture

### Project Structure

```
frontend/
├── public/               # Static files
│   └── index.html        # HTML template
├── src/
│   ├── App.js            # Main React component
│   ├── App.css           # App styles
│   ├── index.js          # React entry point
│   └── index.css         # Global styles
├── package.json          # Dependencies
└── .env.example          # Environment configuration template
```

### React Component Structure

- **App Component**: Main component handling state management
  - Projects list management
  - Form for creating and editing projects
  - Error and success notifications
  - CRUD operations integration

### State Management

- Uses React Hooks (`useState`, `useEffect`)
- Local component state for form inputs and UI state
- API integration with Axios

### Error Handling

- API error messages displayed to users
- Form validation on the client side
- Loading states during API calls
- Confirmation dialogs for destructive operations

## API Design

### RESTful Endpoints

```
GET    /api/projects           # Get all projects
GET    /api/projects/{id}      # Get specific project
POST   /api/projects           # Create project
PUT    /api/projects/{id}      # Update project
DELETE /api/projects/{id}      # Delete project
```

### Response Format

**Success Response:**
```json
{
  "id": 1,
  "name": "Project Name",
  "description": "Project Description",
  "status": 25,
  "createdAt": "2026-04-19T10:00:00Z",
  "updatedAt": "2026-04-19T10:00:00Z"
}
```

**Error Response:**
```json
{
  "message": "Error message",
  "error": "Detailed error information"
}
```

## Data Flow

1. **Frontend** → User submits form
2. **App.js** → Validates input and calls API
3. **Axios** → Makes HTTP request to backend
4. **Controller** → Receives request and validates
5. **Service** → Processes business logic
6. **DbContext** → Interacts with database
7. **Service** → Returns result to controller
8. **Controller** → Returns response to frontend
9. **App.js** → Updates UI with result

## Technologies Used

### Backend
- **.NET 9.0**: Web framework
- **Entity Framework Core 9.0**: ORM
- **SQL Server**: Database
- **Swagger/OpenAPI**: API documentation

### Frontend
- **React 18.2.0**: UI library
- **Axios 1.6.0**: HTTP client
- **CSS3**: Styling
- **Create React App**: Build tooling

## Security Considerations

- Input validation on both frontend and backend
- SQL injection prevention through parameterized queries (EF Core)
- CORS configuration (can be added)
- HTTPS recommended for production
- Environment-based configuration for sensitive data

## Scalability

- Stateless backend design
- Service layer abstraction enables easy replacement
- Database can be scaled independently
- Frontend can be deployed to CDN
- Horizontal scaling possible with load balancer

## Future Enhancements

- Authentication and authorization (JWT)
- Role-based access control (RBAC)
- Pagination and filtering
- Search functionality
- Real-time updates with SignalR
- Unit and integration tests
- API versioning
- Caching strategies
- Rate limiting
