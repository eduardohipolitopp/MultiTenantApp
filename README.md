# MultiTenantApp

A FullStack .NET 10 application with Blazor Server frontend, .NET API backend, and Hangfire background jobs service, featuring DDD, SOLID, TDD, Multi-tenancy, JWT Auth, OpenTelemetry, Health Checks, and comprehensive validation.

## Architecture

- **Domain**: Entities, Interfaces, Attributes (DDD with Soft Delete support).
- **Application**: Services, DTOs, Validators (FluentValidation).
- **Infrastructure**: EF Core, Repositories, TenantProvider, Jobs.
- **Api**: Controllers, Middleware (Health Checks, Rate Limiting, Logging).
- **Web**: Blazor Server Frontend.
- **Hangfire**: Dedicated background jobs service with PostgreSQL storage.

## Features

### Core Features
- **Multi-tenancy**: Single DB with `TenantId` filter and tenant isolation.
- **Authentication**: ASP.NET Identity + JWT with refresh tokens.
- **Authorization**: Role-based (Admin, SystemAdmin, TenantAdmin, User).
- **Soft Delete**: Automatic `IsDeleted`, `DeletedAt`, `UpdatedAt` fields.
- **Background Jobs**: Hangfire with PostgreSQL storage and dashboard.
- **Health Checks**: PostgreSQL, MongoDB, Redis monitoring.
- **Rate Limiting**: Global, per-tenant, per-user, per-IP limits.
- **Caching**: Redis with decorator pattern and TTL management.

### Observability & Monitoring
- **OpenTelemetry**: Traces, Metrics, Logs with OTLP export.
- **Serilog**: Structured logging with multiple sinks.
- **Prometheus**: Metrics collection and alerting.
- **Grafana**: Dashboards for visualization.
- **Tempo**: Distributed tracing storage.
- **Loki**: Log aggregation and querying.

### Development & DevOps
- **Docker**: Full containerization with docker-compose.
- **Migrations**: EF Core auto-migrations in development.
- **Swagger**: API documentation with XML comments.
- **FluentValidation**: Comprehensive input validation.
- **Unit Tests**: xUnit with FluentAssertions and Moq.

### Storage & Infrastructure
- **PostgreSQL**: Primary database with EF Core.
- **MongoDB**: Audit logs and request/response logging.
- **Redis**: Distributed caching and rate limiting.
- **MinIO**: S3-compatible file storage.
- **MailHog**: Email testing in development.

## Getting Started

### Prerequisites

- Docker Desktop (latest version)
- .NET 10 SDK (optional, for local development)
- Git

### Quick Start with Docker

1. **Clone the repository:**
   ```bash
   git clone <repository-url>
   cd MultiTenantApp
   ```

2. **Start all services:**
   ```bash
   docker-compose up --build
   ```

3. **Wait for initialization** (database migrations run automatically).

### Services Overview

The application starts the following services:

| Service | Port | Description |
|---------|------|-------------|
| **API** | 5000 | REST API with Swagger, Health Checks |
| **Web** | 5002 | Blazor Server frontend |
| **Hangfire** | 5003 | Background jobs dashboard (Dev only) |
| **PostgreSQL** | 5432 | Primary database |
| **MongoDB** | 27017 | Audit logs and request logging |
| **Redis** | 6379 | Distributed cache and rate limiting |
| **Grafana** | 3000 | Monitoring dashboards |
| **Prometheus** | 9090 | Metrics collection and alerting |
| **Tempo** | 3200 | Distributed tracing storage |
| **Loki** | 3100 | Log aggregation and querying |
| **MinIO** | 9000/9001 | S3-compatible file storage |
| **MailHog** | 8025 | Email testing in development |

### Accessing the Application

1. **Web Application:**
   - URL: `http://localhost:5002`
   - Login with seeded admin users:
     - **Tenant A**: `admin@tenant-a.com` / `Password123!`
     - **Tenant B**: `admin@tenant-b.com` / `Password123!`

2. **API Documentation:**
   - Swagger: `http://localhost:5000/swagger`
   - Health Checks: `http://localhost:5000/health`

3. **Background Jobs:**
   - Dashboard: `http://localhost:5003/hangfire` (Development only)

4. **Monitoring:**
   - Grafana: `http://localhost:3000` (admin/admin)
   - Prometheus: `http://localhost:9090`

### Local Development

For local development without Docker:

1. **Start infrastructure services:**
   ```bash
   docker-compose up postgres redis mongodb
   ```

2. **Run the applications:**
   ```bash
   # Terminal 1 - API
   cd src/MultiTenantApp.Api
   dotnet run

   # Terminal 2 - Web
   cd src/MultiTenantApp.Web
   dotnet run

   # Terminal 3 - Hangfire
   cd src/MultiTenantApp.Hangfire
   dotnet run
   ```

## API Usage

### Authentication

**Login:**
```bash
POST http://localhost:5000/api/Auth/login
Content-Type: application/json

{
  "email": "admin@tenant-a.com",
  "password": "Password123!",
  "tenantId": "tenant-a"
}
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "...",
  "expiration": "2024-01-01T00:00:00Z",
  "user": {
    "id": "...",
    "email": "admin@tenant-a.com",
    "fullName": "Admin User"
  }
}
```

### Products API

**Get Products (Authenticated):**
```bash
GET http://localhost:5000/api/Products
Authorization: Bearer <TOKEN>
```

**Create Product:**
```bash
POST http://localhost:5000/api/Products
Authorization: Bearer <TOKEN>
Content-Type: application/json

{
  "name": "New Product",
  "description": "Product description",
  "price": 99.99
}
```

### Health Checks

- **Overall Health**: `GET /health`
- **Readiness**: `GET /health/ready`
- **Liveness**: `GET /health/live`
- **UI**: `GET /health-ui` (Development only)

## Background Jobs (Hangfire)

The application uses Hangfire for background processing:

### Dashboard Access
- **Development**: `http://localhost:5003/hangfire`
- **Production**: Disabled for security (configure authorization if needed)

### Sample Jobs
- **SampleRecurringJob**: Demonstrates recurring job execution

### Adding New Jobs

1. **Create a job class:**
```csharp
public class MyBackgroundJob
{
    public async Task ExecuteAsync(string parameter)
    {
        // Job logic here
        await Task.Delay(1000);
    }
}
```

2. **Register in Program.cs:**
```csharp
builder.Services.AddScoped<MyBackgroundJob>();
```

3. **Schedule the job:**
```csharp
RecurringJob.AddOrUpdate("my-job", () => myJob.ExecuteAsync("param"), Cron.Hourly);
```

## Testing

### Unit Tests
```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Integration Tests
```bash
# Run specific test project
dotnet test tests/MultiTenantApp.Tests/MultiTenantApp.Tests.csproj
```

## Configuration

### Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection | `Host=postgres;Database=multitenantdb;Username=postgres;Password=postgres` |
| `ConnectionStrings__MongoDb` | MongoDB connection | `mongodb://admin:admin123@mongodb:27017` |
| `Cache__Redis__ConnectionString` | Redis connection | `redis:6379` |
| `JWT__Secret` | JWT signing key | (Required) |
| `OTEL_EXPORTER_OTLP_ENDPOINT` | OpenTelemetry endpoint | `http://otel-collector:4317` |

### Docker Development

For development with hot reload:

```bash
# Build and run with volume mounting
docker-compose -f docker-compose.dev.yml up --build
```

## Security Features

- **JWT Authentication** with refresh tokens
- **Role-based Authorization** with granular permissions
- **Rate Limiting** at multiple levels
- **Input Validation** with comprehensive DTO validation
- **SQL Injection Prevention** via EF Core
- **XSS Protection** in Blazor Server
- **Secure Headers** via middleware

## Monitoring & Observability

### Metrics
- Request/response times
- Database query performance
- Cache hit rates
- Background job execution
- Error rates and exceptions

### Logging
- Structured logging with Serilog
- Request/response logging with MongoDB storage
- Audit trails for all data changes
- Performance monitoring

### Tracing
- End-to-end request tracing
- Database query tracing
- External API call tracing
- Distributed transaction tracking

## Contributing

1. Fork the repository
2. Create a feature branch
3. Write tests for new functionality
4. Ensure all tests pass
5. Submit a pull request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Support

For support and questions:
- Create an issue in the repository
- Check the documentation in `IMPLEMENTATION_GUIDE.md`
- Review the API documentation at `/swagger`
