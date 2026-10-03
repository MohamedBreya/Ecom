# Ecom API

ASP.NET Core 8 Web API for a small e-commerce catalogue: **categories**, **products** and **product photos**.
SQL Server via EF Core, AutoMapper, Swagger UI, and a generated OpenAPI contract plus a typed TypeScript client for the frontend.

| Doc | What is in it |
|---|---|
| [docs/API.md](docs/API.md) | Every endpoint, request/response examples, validation rules, error codes |
| [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) | Layers, request pipeline, data model, key flows, design decisions |
| [docs/REFACTORING.md](docs/REFACTORING.md) | What changed in the refactor, breaking changes, how it was verified, open follow-ups |
| [contracts/README.md](contracts/README.md) | OpenAPI spec and TypeScript types/client for the frontend |

## Solution layout

```
Ecom.sln
├── Ecom.core            Domain: entities, DTOs, repository/service interfaces, exceptions (no EF, no web code)
├── Ecom.infrastructure  EF Core DbContext + migrations, repositories, image storage, DI registration
├── Ecom.API             Controllers, middleware, mapping profiles, Program.cs
└── contracts            openapi.json + TypeScript types and client
```

## Getting started

**Prerequisites:** [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0), a SQL Server instance (Docker recommended), and the EF tool: `dotnet tool install --global dotnet-ef`.

### 1. Database

Using Docker (works on macOS/Linux/Windows; on Apple Silicon it runs under emulation):

```bash
cp .env.example .env        # then set MSSQL_SA_PASSWORD
docker compose up -d
```

Then tell the API where it is. Keep the password out of source control, e.g. with user secrets:

```bash
dotnet user-secrets init --project Ecom.API
dotnet user-secrets set "ConnectionStrings:Ecom" "Server=localhost,1433;Database=Ecom;User Id=sa;Password=<your password>;TrustServerCertificate=True" --project Ecom.API
```

`appsettings.json` still contains the original Windows/SQL Express connection string, which keeps working on that machine. Any other source (user secrets, the `ConnectionStrings__Ecom` environment variable) overrides it.

### 2. Create the schema

```bash
dotnet ef database update --project Ecom.infrastructure --startup-project Ecom.API
```

### 3. Run

```bash
dotnet run --project Ecom.API --launch-profile http
```

Swagger UI opens at <http://localhost:5159/swagger>. A few ready-made requests are in [Ecom.API/Ecom.API.http](Ecom.API/Ecom.API.http).

## Configuration

| Key | Default | Purpose |
|---|---|---|
| `ConnectionStrings:Ecom` | original SQL Express string | SQL Server connection |
| `Cors:AllowedOrigins` | `[]` (Development: ports 3000, 4200, 5173 on localhost) | Browser origins allowed to call the API. Set this for production. |
| `RateLimiting:PermitLimit` | `100` | Requests allowed per client IP per window |
| `RateLimiting:WindowSeconds` | `60` | Length of the rate-limit window |

## Images

Uploaded product photos are stored in `Ecom.API/wwwroot/Images/<product-name>/<random>.<ext>` and served as static files at `/Images/...`.
The folder content is git-ignored; only `.gitkeep` is tracked. See [docs/API.md](docs/API.md#images) for the rules (types, size limit).

## Migrations

```bash
dotnet ef migrations add <Name> --project Ecom.infrastructure --startup-project Ecom.API --output-dir Data/Migrations
dotnet ef database update      --project Ecom.infrastructure --startup-project Ecom.API
```

## Notes

- AutoMapper 15 is commercially licensed for production use; it logs a license notice at startup. Development and testing are allowed without a key.
- There is no authentication yet: every endpoint is public. Add it before exposing write endpoints.
