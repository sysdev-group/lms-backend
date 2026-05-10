# LMS Backend — ASP.NET Core Web API

Modern Modular Learning Management System — backend service built with ASP.NET Core 8, Clean Architecture, EF Core, and PostgreSQL.

---

## Prerequisites

| Tool | Version | Download |
|------|---------|----------|
| Docker Desktop | Latest | https://www.docker.com/products/docker-desktop |
| .NET SDK | 8.0+ | https://dotnet.microsoft.com/download (only needed for running without Docker) |

---

## Running with Docker (Recommended)

This starts the API, database, and frontend together.

```bash
# Clone both repos side by side:
# your-folder/
#   lms-backend/   ← this repo
#   lms-frontend/  ← frontend repo

cd lms-backend

# Start everything
docker-compose up

# First run will take a few minutes — it's downloading images and building
# Once you see "Now listening on: http://[::]:5001" the API is ready
```

**Services running after `docker-compose up`:**
- API: http://localhost:5001
- Swagger UI: http://localhost:5001/swagger
- Database: localhost:5432 (connect with DBeaver/pgAdmin using credentials in docker-compose.yml)
- Frontend: http://localhost:4200

**Useful commands:**
```bash
docker-compose up --build      # Rebuild after code changes
docker-compose down            # Stop all services
docker-compose down -v         # Stop + delete database (fresh start)
docker-compose logs -f lms-api # Watch API logs
```

---

## Running without Docker

If you prefer running the API directly:

```bash
# 1. Start PostgreSQL (you need it running somewhere)
# 2. Update appsettings.Development.json with your connection string

cd src/LMS.API
dotnet run
```

---

## Project Structure

```
src/
  LMS.Domain/         — Entities and Enums. No dependencies on other layers.
  LMS.Application/    — Interfaces and DTOs. Depends only on Domain.
  LMS.Infrastructure/ — EF Core, service implementations. Depends on Application + Domain.
  LMS.API/            — Controllers, middleware, Program.cs. Depends on all layers.
```

**The rule:** dependencies only flow inward. Domain knows nothing. Application knows Domain. Infrastructure knows Application + Domain. API knows everything.

---

## Adding a Database Migration

Run this from the repo root after changing any entity:

```bash
dotnet ef migrations add YourMigrationName \
  --project src/LMS.Infrastructure \
  --startup-project src/LMS.API

dotnet ef database update \
  --project src/LMS.Infrastructure \
  --startup-project src/LMS.API
```

Migrations run automatically on startup in Development mode.

---

## Implementing Your Module

1. Find your service stub in `src/LMS.Infrastructure/Services/StubServices.cs`
2. Move it into its own file (e.g. `CourseService.cs`) in the same folder
3. Inject `AppDbContext` in the constructor — see `AuthService.cs` for the pattern
4. Replace each `throw new NotImplementedException(...)` with real EF Core code
5. The interface is already defined in `src/LMS.Application/Interfaces/IServices.cs`
6. The DTOs are already defined in `src/LMS.Application/DTOs/`
7. The controller stub is already in `src/LMS.API/Controllers/StubControllers.cs`

**The worked example is `AuthService.cs` and `AuthController.cs` — read these first.**

---

## Code Standards

- Methods max ~30 lines — extract private helpers if it gets longer
- Always `async`/`await` — never `.Result` or `.Wait()`
- Map to DTOs before returning from service methods — never expose domain entities to controllers
- Branch naming: `feature/your-name/module-name` (e.g. `feature/ahmed/course-service`)
- Open a PR when done — assigned reviewer will review before merge to `main`

---

## Environment Variables

See `.env.example` for all required variables. Never commit secrets to the repo.
