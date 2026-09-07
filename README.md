# E-Commerce API — ASP.NET Core Project

An Order Management API built with ASP.NET Core 10, Entity Framework Core, and PostgreSQL. The goal was to go beyond basic CRUD and build something closer to a real production system: relational data, a proper service layer, authentication, automated testing, containerization, and a CI pipeline.

## Domain

A small e-commerce system with five related entities:

```
Category  1 ──── * Product
Customer  1 ──── * Order
Order     1 ──── * OrderItem  * ──── 1  Product
```

- **Category** — groups products
- **Product** — has a price, stock quantity, and belongs to a category
- **Customer** — linked to a registered user account
- **Order** — placed by a customer, contains multiple order items
- **OrderItem** — links an order to a product, with quantity and a *snapshotted* unit price (frozen at purchase time, so later price changes never affect past orders)

## Tech stack

- .NET 10 / ASP.NET Core
- Entity Framework Core + PostgreSQL (Npgsql), running in Docker
- ASP.NET Core Identity + JWT for authentication
- AutoMapper (entity ↔ DTO mapping)
- FluentValidation (request validation)
- xUnit, Moq, FluentAssertions, EF Core InMemory/SQLite (testing)
- Docker + docker-compose (containerized API + database)
- GitHub Actions (CI — build & test on every push/PR)

## Getting started

### Run with Docker

```bash
cp .env.example .env        # fill in your own values (see below)
docker compose up --build
```

The API will be available at `http://localhost:5080`, with Swagger UI at `http://localhost:5080/swagger`.

**`.env` values needed:**
```
POSTGRES_PASSWORD=devpassword
JWT_KEY=generate-your-own-with-openssl-rand-base64-32
```

### Run locally (without Docker)

```bash
cd EcommerceApi.Api
dotnet user-secrets set "Jwt:Key" "your-generated-key"
dotnet ef database update
dotnet run
```

### Run the tests

```bash
cd EcommerceApi.Tests
dotnet test
```

## API overview

| Area | Endpoints | Auth required |
|---|---|---|
| Auth | `POST /api/auth/register`, `POST /api/auth/login` | No |
| Categories | Full CRUD at `/api/categories` | Writes: Admin only |
| Products | Full CRUD at `/api/products`, filterable by category/price | Writes: Admin only |
| Customers | Read + create at `/api/customers` | No |
| Orders | Create + read at `/api/orders`, with stock validation and price snapshotting | Any authenticated user |

New registrations default to the `Customer` role; an `Admin` role must be assigned manually (via direct database access) — there's no self-service way to become an admin, intentionally.

## What this project covers

1. **Setup** — solution structure, PostgreSQL via Docker from day one (a production-realistic choice over SQLite)
2. **Domain modeling** — EF Core relationships (one-to-many, join-entity many-to-many), Fluent API configuration, `DeleteBehavior` decisions (`Restrict` vs `Cascade`)
3. **DTOs & mapping** — why entities should never be returned directly from an API (circular references, over-posting), AutoMapper profiles
4. **Service layer & DI** — async services behind interfaces, `AsNoTracking()` for reads, eager/explicit loading, real business logic (stock checks, price snapshotting) that lives in services, not in DTOs or attributes
5. **Validation** — FluentValidation for rules data annotations can't express cleanly (collection validation, cross-field rules)
6. **Centralized error handling** — custom exception types (`NotFoundException`, `BusinessRuleException`, `ConflictException`) mapped to HTTP status codes in one middleware, replacing per-controller try/catch
7. **Authentication & authorization** — ASP.NET Core Identity + JWT, role-based `[Authorize]`, secrets kept out of source control via User Secrets
8. **Testing** — unit tests for service logic (including regression tests for real bugs found along the way), integration tests via `WebApplicationFactory` exercising the real HTTP pipeline
9. **Docker** — multi-stage Dockerfile, docker-compose for the full stack, environment-variable-based configuration
10. **CI** — GitHub Actions running the full test suite on every push and pull request

## Notable bugs hit and fixed along the way

- **Transactions vs. `SaveChangesAsync()`** — an explicit `BeginTransactionAsync()` was unnecessary (and buggy) when a single `SaveChangesAsync()` call already wraps its changes atomically.
- **AutoMapper + positional records** — records with a positional constructor confuse AutoMapper's constructor resolution; switching read DTOs to property-initializer style fixed it.
- **EF Core's InMemory provider doesn't enforce foreign keys** — a test verifying a `Restrict`-behavior constraint needed SQLite's in-memory mode instead, since InMemory silently doesn't replicate real relational constraint enforcement.
- **Same `DbContext` instance for test setup and the action under test** hid a bug, using a fresh context for the actual test action (mirroring a real per-request scope) was necessary to reproduce real behavior.
- **`WebApplicationFactory` needs its own config overrides** — User Secrets don't load outside the `Development` environment, so JWT settings had to be supplied explicitly via `UseSetting(...)` for the test host.
- **macOS's case-insensitive filesystem hid a folder/file casing mismatch** that only surfaced once CI ran on Linux.