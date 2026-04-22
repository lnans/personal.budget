# Technology Stack

## Core Sections (Required)

### 1) Runtime Summary

#### Backend

| Area                | Value                              | Evidence                                                   |
| ------------------- | ---------------------------------- | ---------------------------------------------------------- |
| Primary language    | C# 13                              | backend/Directory.Build.props (`<TargetFramework>net10.0`) |
| Runtime + version   | .NET 10                            | backend/Directory.Build.props                              |
| Package manager     | NuGet (Central Package Management) | backend/Directory.Packages.props                           |
| Module/build system | `dotnet build` / `dotnet run`      | backend/Personal.Budget.slnx                               |

#### Frontend

| Area                | Value                     | Evidence                                                  |
| ------------------- | ------------------------- | --------------------------------------------------------- |
| Primary language    | TypeScript ~5.9.3         | frontend/package.json                                     |
| Runtime             | Node.js (build-time only) | frontend/package.json                                     |
| Package manager     | Yarn 4.12.0 (Berry)       | frontend/package.json (`"packageManager": "yarn@4.12.0"`) |
| Module/build system | Vite 7                    | frontend/package.json                                     |

---

### 2) Production Frameworks and Dependencies

#### Backend

| Dependency                                    | Version | Role in system                                       | Evidence                         |
| --------------------------------------------- | ------- | ---------------------------------------------------- | -------------------------------- |
| ASP.NET Core 10 (Minimal APIs)                | 10.0.3  | HTTP host, routing, DI container                     | backend/src/Api/Program.cs       |
| Entity Framework Core                         | 10.0.3  | ORM, migrations, query/persistence                   | backend/Directory.Packages.props |
| Npgsql.EF Core PostgreSQL                     | 10.0.0  | EF Core provider for PostgreSQL                      | backend/Directory.Packages.props |
| ErrorOr                                       | 2.0.1   | Railway-oriented result type for domain errors       | backend/Directory.Packages.props |
| FluentValidation                              | 12.1.1  | Input validation (auto-registered via assembly scan) | backend/Directory.Packages.props |
| Scrutor                                       | 7.0.0   | Decorator registration (`TryDecorate`)               | backend/Directory.Packages.props |
| Serilog                                       | 4.3.1   | Structured logging                                   | backend/Directory.Packages.props |
| Serilog.AspNetCore                            | 10.0.0  | ASP.NET Core integration                             | backend/Directory.Packages.props |
| Microsoft.AspNetCore.Authentication.JwtBearer | 10.0.3  | JWT authentication                                   | backend/Directory.Packages.props |
| Scalar.AspNetCore                             | 2.12.47 | OpenAPI UI (replaces Swagger UI)                     | backend/Directory.Packages.props |
| Ardalis.GuardClauses                          | 5.0.0   | Guard utilities for input defense                    | backend/Directory.Packages.props |

#### Frontend

| Dependency               | Version     | Role in system                      | Evidence              |
| ------------------------ | ----------- | ----------------------------------- | --------------------- |
| React 19                 | ^19.2.4     | UI rendering                        | frontend/package.json |
| React Router v7          | ^7.13.0     | Client-side routing                 | frontend/package.json |
| TanStack Query v5        | ^5.90.21    | Server state, caching, invalidation | frontend/package.json |
| Axios                    | ^1.13.5     | HTTP client with interceptors       | frontend/package.json |
| Zustand v5               | ^5.0.11     | Global client state (auth store)    | frontend/package.json |
| React Hook Form          | ^7.71.2     | Form state management               | frontend/package.json |
| Zod v4                   | ^4.3.6      | Schema validation + RHF resolver    | frontend/package.json |
| i18next                  | ^25.8.13    | Internationalisation                | frontend/package.json |
| Tailwind CSS v4          | ^4.2.0      | Utility-first styling               | frontend/package.json |
| Radix UI (various)       | ^1–^2       | Accessible headless UI primitives   | frontend/package.json |
| class-variance-authority | ^0.7.1      | Component variant builder (`cva`)   | frontend/package.json |
| tailwind-merge + clsx    | ^3.5 / ^2.1 | Class merging utility (`cn()`)      | frontend/package.json |
| sonner                   | ^2.0.7      | Toast notifications                 | frontend/package.json |
| lucide-react             | ^0.563.0    | Icon set                            | frontend/package.json |

---

### 3) Development Toolchain

| Tool                                      | Purpose                                          | Evidence                         |
| ----------------------------------------- | ------------------------------------------------ | -------------------------------- |
| xunit.v3.mtp-v2 (3.2.2)                   | Backend test runner (Microsoft Testing Platform) | backend/Directory.Packages.props |
| NSubstitute (5.3.0)                       | Mocking in backend unit tests                    | backend/Directory.Packages.props |
| Shouldly (4.3.0)                          | Fluent assertion library                         | backend/Directory.Packages.props |
| Testcontainers.PostgreSql (4.10.0)        | Real PostgreSQL container in integration tests   | backend/Directory.Packages.props |
| Respawn (7.0.0)                           | Database reset between integration tests         | backend/Directory.Packages.props |
| NetArchTest.Rules (1.3.2)                 | Architecture constraint enforcement              | backend/Directory.Packages.props |
| Microsoft.Testing.Extensions.CodeCoverage | Code coverage collection                         | backend/Directory.Packages.props |
| CSharpier                                 | Opinionated C# formatter                         | backend/format.sh                |
| dotnet format                             | Roslyn code style enforcement                    | backend/format.sh                |
| ESLint 9                                  | TypeScript linting                               | frontend/eslint.config.js        |
| Prettier 3                                | TypeScript/JSON formatting                       | frontend/.prettierrc.json        |
| typescript-eslint                         | TS-aware ESLint rules                            | frontend/package.json            |

---

### 4) Key Commands

```bash
# Backend (run from backend/)
dotnet build Personal.Budget.sln          # build
./run-test.sh                              # all backend tests
./run-test.sh --coverage                   # tests + HTML coverage
./format.sh                                # format (dotnet format + csharpier)

# Frontend (run from frontend/)
yarn dev                                   # Vite dev server
yarn build                                 # TypeScript compile + Vite build
yarn lint:fix                              # ESLint with auto-fix
yarn type:check                            # tsc noEmit
yarn format                                # Prettier + i18n key formatter
yarn validate                              # lint:fix + type:check + format

# Full stack (run from repo root)
./run-server.sh                            # docker compose up -d --build
docker compose down
```

---

### 5) Environment and Config

- Config sources: `docker-compose.yml` (defaults), root `.env` override, `backend/.env.production`, `frontend/.env` / `frontend/.env.production`
- Required backend env vars: `ConnectionStrings__Database`, `Auth__SecretKey`, `Auth__Issuer`, `Auth__Audience`, `Auth__ExpirationMinutes`, `Auth__RefreshTokenExpirationDays`, `BUDGET_USER`, `BUDGET_PASSWORD`
- Required frontend env vars: `VITE_API_URL` (baked in at Vite build time)
- Default credentials in compose: `admin` / `admin` (change in production)
- API ports: `127.0.0.1:8080` (API), `127.0.0.1:8081` (web), `127.0.0.1:5432` (DB)

---

### 6) Evidence

- backend/Directory.Packages.props
- backend/Directory.Build.props
- frontend/package.json
- docker-compose.yml
- .github/workflows/backend.yml
- .github/workflows/frontend.yml
