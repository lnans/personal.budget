# Architecture

## Core Sections (Required)

### 1) Architectural Style

- **Backend**: Clean Architecture with pragmatic CQRS and railway-oriented error handling
  - Four explicit layers: `Domain` → `Application` → `Infrastructure` → `Api`
  - CQRS pattern without MediatR — custom `ICommandHandler<T>` / `IQueryHandler<T,R>` interfaces, registered by Scrutor assembly scanning
  - `ErrorOr<T>` permeates every layer boundary; HTTP adapts results to RFC 7807 Problem Details
- **Frontend**: Feature-sliced SPA with a clear server-state / client-state split
  - Server state: TanStack Query (fetch, cache, invalidation)
  - Client state: Zustand stores, one per feature
  - Routing: React Router v7 with lazy-loaded routes and an auth guard

---

### 2) System Flow — Backend Request Lifecycle

```
HTTP Request
  → ASP.NET Core Minimal API endpoint (src/Api/Endpoints/*.cs)
      → resolves ICommandHandler<TCmd,TResp> or IQueryHandler<TQuery,TResp> from DI
          [Outer] LoggingDecorator   — logs command/query name + result
          [Inner] ValidationDecorator — runs FluentValidation; short-circuits on failure
          [Core]  Concrete Handler   — thin orchestrator:
                      loads entities via IAppDbContext
                      calls domain entity method (returns ErrorOr<T>)
                      persists via SaveChangesAsync
                      maps to response DTO
  → handler returns ErrorOr<Response>
  → endpoint calls result.ToOkResultOrProblem(context)
  ← HTTP 200 OK (body) or 4xx/5xx ProblemDetails (RFC 7807)
```

Evidence: backend/src/Api/Endpoints/AccountsEndpoints.cs, backend/src/Application/Features/Accounts/Commands/CreateAccount/CreateAccountHandler.cs, backend/src/Api/Extensions/ErrorExtensions.cs

---

### 3) Layer / Module Responsibilities

| Layer                       | Owns                                                                                                                            | Must not own                                     | Evidence                                                               |
| --------------------------- | ------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------ | ---------------------------------------------------------------------- |
| **Domain**                  | Entities (`Account`, `AccountOperation`, `Tag`, `User`), domain errors, invariant enforcement via `ErrorOr<T>`, factory methods | Framework references, EF Core, HTTP types        | backend/src/Domain/Accounts/Account.cs                                 |
| **Application**             | Handlers (thin orchestrators), validators, decorator pipeline                                                                   | Business rules, direct DB queries, HTTP types    | backend/src/Application/Features/, backend/src/Application/Decorators/ |
| **Infrastructure**          | `AppDbContext` (EF Core + Npgsql), EF configurations, migrations, `AppDbContextInitializer`, `PasswordHasher`                   | Business logic, HTTP contracts                   | backend/src/Infrastructure/Persistence/                                |
| **Api**                     | Endpoint mapping (`IEndpoints`), HTTP contracts, middleware, JWT options, `IAuthContext` impl                                   | Business rules, direct DB access                 | backend/src/Api/Endpoints/, backend/src/Api/Authentication/            |
| **Frontend api/endpoints/** | TanStack Query `useQuery`/`useMutation` hooks                                                                                   | Direct axios calls (must go through `apiClient`) | frontend/src/api/endpoints/                                            |
| **Frontend features/**      | Feature-scoped components, hooks, Zustand stores                                                                                | Cross-feature logic (goes to shared providers)   | frontend/src/features/                                                 |

---

### 4) Decorator Pipeline (Application Layer)

Scrutor's `TryDecorate` wires the three decorator layers in registration order (outermost last):

```
Request
  → LoggingDecorator   (logs command name, success/failure + error codes)
      → ValidationDecorator (runs FluentValidation; returns Validation error on failure)
          → ConcreteHandler (business-delegating orchestrator)
```

Evidence: backend/src/Application/DependencyInjection.cs, backend/src/Application/Decorators/LoggingDecorator.cs, backend/src/Application/Decorators/ValidationDecorator.cs

---

### 5) Domain Entity Design

- All entities extend `Entity` (GUID v7 `Id`, `CreatedAt`, `UpdatedAt`, `DeletedAt?`)
- **Private constructors + static `Create(...)` factories** returning `ErrorOr<Entity>` — invariants enforced at creation
- Business mutations (e.g., `Account.AddOperation(...)`, `Tag.Update(...)`) are instance methods returning `ErrorOr<T>`
- Soft delete is a nullable `DeletedAt` timestamp — no physical deletes in domain logic
- `Account` owns a collection of `AccountOperation`; operations know their `PreviousBalance` and `NextBalance` at creation time (balance accounting is side-effect-free)

Evidence: backend/src/Domain/Entity.cs, backend/src/Domain/Accounts/Account.cs, backend/src/Domain/AccountOperations/AccountOperation.cs, backend/src/Domain/Tags/Tag.cs

---

### 6) Frontend Architecture

```
main.tsx
  → RouterProvider (React Router v7 BrowserRouter)
      → /auth  → AuthPage (unauthenticated)
      → /      → ProtectedLayout
                    → AuthProvider (validates/refreshes JWT; redirects to /auth on expiry)
                        → App
                            → MainLayout
                                → /operations → OperationsPage
```

- **AuthProvider** handles JWT validity check, automatic token refresh (scheduled 5 min before expiry), and redirect logic.
- **Axios interceptor** (`config/axios.ts`) injects `Authorization: Bearer <token>` on every request and normalises error responses to the `Problem` type.
- **Query invalidation** is the single mechanism for UI refresh after mutations.

Evidence: frontend/src/providers/RouterProvider.tsx, frontend/src/providers/AuthProvider.tsx, frontend/src/config/axios.ts

---

### 7) Reused Patterns

| Pattern                                                            | Where found                     | Why it exists                                                                |
| ------------------------------------------------------------------ | ------------------------------- | ---------------------------------------------------------------------------- |
| Railway-oriented error chaining (`Then` / `ThenDo` / `MatchFirst`) | Application handlers            | Eliminates null/exception-based branching in orchestration code              |
| Decorator (Scrutor `TryDecorate`)                                  | Application DI wiring           | Adds cross-cutting concerns (logging, validation) without modifying handlers |
| Factory method on entity (`Entity.Create(...)`)                    | Domain layer                    | Guarantees invariants are checked before an entity can exist                 |
| Centralised query key factory                                      | `frontend/src/api/QueryKeys.ts` | Single source of truth for TanStack cache keys; prevents key drift           |
| Endpoint discovery by reflection                                   | `EndpointsConfiguration.cs`     | New endpoint modules are auto-wired by implementing `IEndpoints`             |
| Feature-sliced modules                                             | `frontend/src/features/`        | Co-locates domain-scoped code; limits import leakage                         |

---

### 8) Known Architectural Risks

- **CORS is fully open** (`AllowAnyOrigin / AllowAnyHeader / AllowAnyMethod`) — acceptable for a personal project but must be scoped for any multi-tenant or public deployment.
- **`VITE_API_URL` is baked at build time** — changing the API host requires a frontend image rebuild; no runtime injection support.
- **No refresh-token rotation** — refresh tokens are long-lived (7 days) without server-side revocation; a stolen refresh token stays valid until expiry.
- **Single-user seeding** — `AppDbContextInitializer` seeds one user from `BUDGET_USER`/`BUDGET_PASSWORD`; there is no self-registration flow, so adding more users requires manual DB intervention.

---

### 9) Evidence

- backend/src/Api/Program.cs
- backend/src/Application/DependencyInjection.cs
- backend/src/Application/Features/Accounts/Commands/CreateAccount/CreateAccountHandler.cs
- backend/src/Domain/Accounts/Account.cs
- backend/src/Domain/Entity.cs
- backend/src/Api/Extensions/ErrorExtensions.cs
- frontend/src/providers/RouterProvider.tsx
- frontend/src/providers/AuthProvider.tsx
- frontend/src/config/axios.ts
