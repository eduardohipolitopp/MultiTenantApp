# MultiTenantApp

A FullStack .NET 8 application with Blazor Server frontend and .NET API backend, featuring DDD, SOLID, TDD, Multi-tenancy, JWT Auth, and OpenTelemetry.

## Architecture

- **Domain**: Entities, Interfaces (DDD).
- **Application**: Services, DTOs.
- **Infrastructure**: EF Core, Repositories, TenantProvider.
- **Api**: Controllers, Middleware.
- **Web**: Blazor Server Frontend.

## Features

- **Multi-tenancy**: Database per tenant logic (Single DB with `TenantId` filter).
- **Authentication**: ASP.NET Identity + JWT.
- **Authorization**: Role-based (Admin, User).
- **Observability**: OpenTelemetry (Traces, Metrics, Logs).
- **Docker**: Full containerization.

## Getting Started

### Prerequisites

- Docker Desktop
- .NET 8 SDK (optional, for local dev)

### Running with Docker

1. Clone the repository.
2. Run `docker-compose up --build`.

The application will start the following services:
- **Postgres**: Database (Port 5432)
- **Otel Collector**: Observability (Ports 4317, 4318, 8888)
- **API**: Backend (Port 5000)
- **Web**: Frontend (Port 5002)

### Accessing the App

1. Open `http://localhost:5002` in your browser.
2. Login with one of the seeded admin users:
    - **Tenant A**: `admin@tenant-a.com` / `Password123!` (Select Tenant A)
    - **Tenant B**: `admin@tenant-b.com` / `Password123!` (Select Tenant B)

### API Endpoints

You can access the API Swagger (if enabled in prod/docker, currently dev only) or use Postman.

**Login Request:**
```json
POST http://localhost:5000/api/Auth/login
{
  "email": "admin@tenant-a.com",
  "password": "Password123!",
  "tenantId": "tenant-a"
}
```

**Get Products (Requires Bearer Token):**
```
GET http://localhost:5000/api/Products
Authorization: Bearer <TOKEN>
```

## Tests

Run unit tests with:
```bash
dotnet test
```
