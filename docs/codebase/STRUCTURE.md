# Codebase Structure

## Core Sections (Required)

### 1) Top-Level Map

| Path                 | Purpose                                                                             | Evidence                     |
| -------------------- | ----------------------------------------------------------------------------------- | ---------------------------- |
| `backend/`           | .NET 10 REST API — Clean Architecture (Domain / Application / Infrastructure / Api) | backend/Personal.Budget.slnx |
| `frontend/`          | React 19 SPA — Vite, TypeScript, TanStack Query                                     | frontend/package.json        |
| `docker-compose.yml` | Orchestrates `budget-db`, `budget-api`, `budget-web` for local and prod             | docker-compose.yml           |
| `run-server.sh`      | Wrapper that loads optional env overrides then runs `docker compose up -d --build`  | run-server.sh                |
| `.github/`           | CI (GitHub Actions), Copilot instructions, agent prompts, skills                    | .github/workflows/           |
| `docs/codebase/`     | This documentation                                                                  | docs/codebase/               |

---

### 2) Backend Directory Map (`backend/src/`)

| Path                                                    | Purpose                                                                                         |
| ------------------------------------------------------- | ----------------------------------------------------------------------------------------------- |
| `src/Api/`                                              | ASP.NET Core host; Minimal API endpoints, DI wiring, middleware, auth config                    |
| `src/Api/Endpoints/`                                    | Feature endpoint modules implementing `IEndpoints` (discovered by reflection)                   |
| `src/Api/Contracts/`                                    | Request/response DTOs used at the HTTP boundary                                                 |
| `src/Api/Authentication/`                               | JWT token generation helpers (`AuthTokenGenerator`, `AuthContext`)                              |
| `src/Api/Extensions/`                                   | `ErrorExtensions.cs` — maps `ErrorOr<T>` → RFC 7807 `ProblemDetails`                            |
| `src/Api/Middlewares/`                                  | `LogMiddleware`, exception handling middleware                                                  |
| `src/Application/`                                      | Use-case orchestration (CQRS handlers, validators, decorators)                                  |
| `src/Application/Features/`                             | One sub-folder per domain feature (`Accounts`, `AccountOperations`, `Authentication`, `Tags`)   |
| `src/Application/Features/{Feature}/Commands/{Action}/` | `*Command.cs`, `*Handler.cs`, `*Response.cs`, `*Validator.cs`                                   |
| `src/Application/Features/{Feature}/Queries/{Action}/`  | `*Query.cs`, `*Handler.cs`, `*Response.cs`                                                      |
| `src/Application/Decorators/`                           | `LoggingDecorator.cs`, `ValidationDecorator.cs` wrapping all handlers via Scrutor               |
| `src/Application/Interfaces/`                           | `IAppDbContext`, `IAuthContext`, `ICommand`, `ICommandHandler`, `IQuery`, `IQueryHandler`       |
| `src/Domain/`                                           | Pure domain entities and errors (no framework dependencies)                                     |
| `src/Domain/Accounts/`                                  | `Account` entity, `AccountErrors`, `AccountConstants`                                           |
| `src/Domain/AccountOperations/`                         | `AccountOperation` entity, errors, constants                                                    |
| `src/Domain/Tags/`                                      | `Tag` entity, errors, constants                                                                 |
| `src/Domain/Users/`                                     | `User` entity                                                                                   |
| `src/Domain/Entity.cs`                                  | Abstract base with `Id` (GUID v7), `CreatedAt`, `UpdatedAt`, `DeletedAt`                        |
| `src/Infrastructure/`                                   | EF Core persistence, JWT auth implementation                                                    |
| `src/Infrastructure/Persistence/`                       | `AppDbContext`, `AppDbContextInitializer`, `AppDbContextFactory`, EF Configurations, Migrations |
| `src/Infrastructure/Authentication/`                    | `PasswordHasher.cs`                                                                             |

---

### 3) Backend Test Map (`backend/tests/`)

| Path                        | Purpose                                                                    |
| --------------------------- | -------------------------------------------------------------------------- |
| `tests/Api.Tests/`          | Integration tests using Testcontainers + `WebApplicationFactory` + Respawn |
| `tests/Domain.Tests/`       | Pure domain unit tests                                                     |
| `tests/Architecture.Tests/` | NetArchTest architecture constraints                                       |
| `tests/TestFixtures/`       | Shared test data factories (`{Entity}Fixture.CreateValid{Entity}(...)`)    |

---

### 4) Frontend Directory Map (`frontend/src/`)

| Path                           | Purpose                                                                                                                 |
| ------------------------------ | ----------------------------------------------------------------------------------------------------------------------- |
| `src/main.tsx`                 | Vite entry point; mounts `<AppRouter />`                                                                                |
| `src/App.tsx`                  | Root component rendered inside the protected route shell                                                                |
| `src/app/auth/`                | `AuthPage.tsx` — login/register page (unauthenticated route)                                                            |
| `src/app/main/`                | `MainLayout.tsx` + feature pages (`operations/OperationsPage.tsx`)                                                      |
| `src/api/endpoints/`           | TanStack Query hooks per domain (`AccountsEndpoints.ts`, `AccountOperationsEndpoints.ts`, `AuthenticationEndpoints.ts`) |
| `src/api/QueryKeys.ts`         | Centralised query key factory                                                                                           |
| `src/api/QueryClient.ts`       | TanStack QueryClient configuration                                                                                      |
| `src/config/axios.ts`          | Axios instance with JWT injection interceptor and error-normalisation interceptor                                       |
| `src/config/I18next.ts`        | i18next initialisation                                                                                                  |
| `src/features/accounts/`       | Account-specific components, hooks, stores                                                                              |
| `src/features/authentication/` | Auth store (Zustand), auth-related hooks                                                                                |
| `src/components/ui/`           | Shared headless UI components (shadcn/ui pattern)                                                                       |
| `src/components/forms/`        | Reusable form controls                                                                                                  |
| `src/providers/`               | `AuthProvider.tsx`, `QueryProvider.tsx`, `RouterProvider.tsx`                                                           |
| `src/types/`                   | TypeScript DTOs and form types per domain                                                                               |
| `src/hooks/`                   | Shared global hooks (`useMediaQuery`, `useMobile`, `useSearchParams`)                                                   |
| `src/lib/`                     | `utils.ts` (`cn()`), `lazyimport.tsx`                                                                                   |
| `src/env.ts`                   | Typed Zod-validated env (`EnvSchema`)                                                                                   |
| `public/locales/en.json`       | All user-visible i18n strings                                                                                           |

---

### 5) Entry Points

| Entry point                   | File                                                     |
| ----------------------------- | -------------------------------------------------------- |
| Backend HTTP host             | backend/src/Api/Program.cs                               |
| Backend endpoint registration | backend/src/Api/Configurations/EndpointsConfiguration.cs |
| Frontend SPA bootstrap        | frontend/src/main.tsx                                    |
| Frontend routing root         | frontend/src/providers/RouterProvider.tsx                |
| Docker composition            | docker-compose.yml                                       |

---

### 6) Module Boundaries

| Boundary                  | What belongs here                                                   | What must not be here                      |
| ------------------------- | ------------------------------------------------------------------- | ------------------------------------------ |
| `Domain`                  | Entities, value objects, domain errors, invariants                  | Framework references, EF, HTTP concerns    |
| `Application`             | CQRS handlers (thin orchestrators), validators, decorator pipelines | Business rules, EF queries, HTTP types     |
| `Infrastructure`          | EF `DbContext`, migrations, `PasswordHasher`                        | Business logic, HTTP types                 |
| `Api`                     | Endpoint mapping, HTTP contracts, middleware, JWT wiring            | Business rules, direct DB access           |
| `Frontend features/`      | Feature-scoped components, hooks, stores                            | Global/cross-feature state (use providers) |
| `Frontend api/endpoints/` | TanStack Query hooks only                                           | Direct `axios` calls (use `apiClient`)     |

---

### 7) Naming and Organization Rules

- **Backend files**: PascalCase (`CreateAccountHandler.cs`, `AccountErrors.cs`)
- **Backend namespaces**: match folder structure (enforced by `.editorconfig`)
- **Frontend files**: PascalCase for components/pages (`AuthPage.tsx`), camelCase for hooks and utilities (`useMediaQuery.ts`, `utils.ts`), PascalCase for DTOs (`GetAccountsResponse.ts`)
- **Frontend organization**: feature-sliced (`features/{feature}/components|hooks|stores`) + shared (`components/ui/`)

---

### 8) Evidence

- backend/src/Api/Program.cs
- backend/src/Api/Configurations/EndpointsConfiguration.cs
- frontend/src/providers/RouterProvider.tsx
- frontend/src/main.tsx
- docs/codebase/.codebase-scan.txt (directory tree section)
