# Copilot instructions for Personal.Budget

## Monorepo structure

This repository is a monorepo containing two independent projects:

- `backend/` — .NET 10 REST API (Clean Architecture, CQRS, EF Core, PostgreSQL)
- `frontend/` — React 19 SPA (Vite, TypeScript, TanStack Query, React Router v7, Tailwind CSS v4)

Always scope changes to the correct sub-project. Backend and frontend are deployed together via `docker-compose.yml` at the repository root.

---

## Backend — Big picture architecture

- Solution follows Clean Architecture with pragmatic EF Core usage in `Application` (`src/Api`, `src/Application`, `src/Domain`, `src/Infrastructure`).
- Typical request flow: Minimal API endpoint -> `ICommandHandler`/`IQueryHandler` resolution from DI -> handler in `Application.Features.*` -> domain methods/entities -> `IAppDbContext` persistence -> `ErrorOr<T>` mapped to HTTP Problem Details.
- Endpoints are discovered by reflection via `IEndpoints` and `MapApiEndpoints()` (`src/Api/Configurations/EndpointsConfiguration.cs`), so new endpoint modules must implement `IEndpoints`.
- API startup wiring lives in `src/Api/Program.cs` and delegates per-layer DI to `AddApiServices`, `AddApplicationServices`, `AddInfrastructureServices`.

## Backend — Request/response and error handling patterns

- Use CQRS folders per feature: `Features/{Feature}/Commands/{Action}` and `Features/{Feature}/Queries/{Action}` (see `CreateAccount*` in `src/Application/Features/Accounts/Commands/CreateAccount/`).
- Command folders usually contain `{Action}Command.cs`, `{Action}Handler.cs`, `{Action}Response.cs`, `{Action}Validator.cs`; query folders usually contain `{Action}Query.cs`, `{Action}Handler.cs`, `{Action}Response.cs`.
- Commands/queries use custom handler interfaces (`ICommandHandler<TCommand>`, `ICommandHandler<TCommand,TResponse>`, `IQueryHandler<TQuery,TResponse>`); write operations generally return `ErrorOr<Response>`.
- Keep business validation in both FluentValidation and domain factories/methods. Example: `CreateAccountValidator` + `Account.Create(...)`.
- API handlers should return `result.ToOkResultOrProblem(context)` to map `ErrorOr` to RFC 7807 payloads (`src/Api/Extensions/ErrorExtensions.cs`).
- Cross-cutting concerns are implemented with decorators registered in `AddApplicationServices`: `ValidationDecorator` and `LoggingDecorator` around command/query handlers; validators are auto-registered from the `Application` assembly.
- Decorator order matters: `LoggingDecorator` is the outer layer and `ValidationDecorator` runs before the concrete handler.

## Backend — Thin handlers, rich domain

- Handlers contain **absolutely no business logic**. They are thin orchestrators that: load entities from `IAppDbContext`, call domain entity methods, persist via `SaveChangesAsync`, and map to a response DTO.
- All business rules, validation, state transitions, and invariant checks live exclusively on **domain entity methods** which return `ErrorOr<T>`.
- Handlers must NOT: check field lengths or null values, compute derived state (e.g. balance), throw or catch business exceptions, or contain conditional branching based on business rules.
- Use the ErrorOr chaining API (`Then`, `ThenAsync`, `ThenDo`, `ThenDoAsync`, `MatchFirst`) to compose handler pipelines in a railway-oriented style.
- `Then` / `ThenAsync` are for steps that can produce a new error (lambda returns `ErrorOr<T>`); `ThenDo` / `ThenDoAsync` are for side effects that cannot fail (lambda returns `void` / `Task`).
- Always end a chain with `MatchFirst` to convert to the final response type.

## Backend — Domain and persistence conventions

- Domain entities inherit `Entity` (`Id` as GUID v7, `CreatedAt`, `UpdatedAt`, `DeletedAt`). Soft delete is timestamp-based.
- Domain entities use **private constructors** and static **factory methods** (`Entity.Create(...)`) that return `ErrorOr<Entity>` to enforce domain validation at creation.
- All business rules and state mutations are methods on domain entities returning `ErrorOr<T>` — never in handlers.
- Prefer domain errors from `*Errors.cs` using stable codes (e.g., `Account.Name.Required` in `src/Domain/Accounts/AccountErrors.cs`).
- `IAuthContext.CurrentUserId` is the boundary for authenticated user identity; API implementation reads JWT claim `nameidentifier`/`sub`.
- `AppDbContextInitializer` runs migrations at startup and seeds a single default user from `BUDGET_USER` / `BUDGET_PASSWORD`. **Single-user model is intentional** — there is no self-registration flow.
- `AccountOperation` has an `IsRecurring` flag (bool) and a many-to-many collection of `Tag` entities.
- `Account.Balance` is **always recomputed by a domain entity method** when operations are added, updated, or deleted — never compute or mutate balance directly in a handler.
- `PasswordHasher` uses **PBKDF2-HMAC-SHA256** with 600,000 iterations and a 16-byte random salt (OWASP-compliant). Use `IPasswordHasher` — never hash passwords manually.

## Critical workflows

- Backend build: `dotnet build Personal.Budget.slnx` (run from `backend/`)
- Run the full stack (DB + API + web) from the **repository root**: `./run-server.sh` or `docker compose up -d --build` (compose file is at the repo root).
- Backend tests: `./run-test.sh` from `backend/` (uses Microsoft Testing Platform via `dotnet run` on test projects).
- Backend coverage: `./run-test.sh --coverage` (HTML generated in `backend/.coverage/`).
- Backend format: `./format.sh` from `backend/` (runs `dotnet format` + `dotnet csharpier` and excludes migrations).
- Frontend dev: `yarn dev` from `frontend/` (Vite dev server).
- Frontend build: `yarn build` from `frontend/`.
- Frontend lint: `yarn lint:fix` from `frontend/`.
- Frontend type check: `yarn type:check` from `frontend/`.
- Frontend format: `yarn format` from `frontend/` (Prettier + i18n key formatter).
- Frontend validate (all checks): `yarn validate` from `frontend/`.

## Backend — Testing and integration specifics

- `tests/Api.Tests` are integration tests using Testcontainers PostgreSQL + `WebApplicationFactory` + Respawn reset between tests (`ApiTestFixture`).
- `tests/Domain.Tests` are pure domain unit tests with shared assertions in `TestBase`.
- `tests/Architecture.Tests` enforce structural rules (layer access, handler/validator placement, decorator patterns) using NetArchTest.
- Keep API test state isolated by using fixture helpers (`ApiTestBase`, `ResetFixtureStateAsync`, `CreateFreshScope`).
- API tests should inherit from `ApiTestBase` and use collection fixture wiring from `ApiTestCollection`.
- Test naming convention: `{Method}_{Scenario}_{ExpectedResult}` for API and domain tests.
- Domain test data factories live in `tests/TestFixtures/Domain` (pattern: `{Entity}Fixture.CreateValid{Entity}(...)`).
- Update `tests/Architecture.Tests` when a new entity, handler, or structural pattern is introduced.

## Backend — Project-specific guardrails

- This repo is strict on style/analyzers (`.editorconfig`, `TreatWarningsAsErrors=true`); prefer `var`, file-scoped namespaces, and pass cancellation tokens through async calls.
- Style specifics to respect: avoid `this.` qualifiers, prefer keyword types (`string` not `String`), use braces, and keep Allman new-line brace formatting.
- Prefer modern C# idioms already enforced here: pattern matching, null-propagation, collection/object initializers, expression-bodied members where appropriate.
- Central package versions are managed in `Directory.Packages.props`; do not pin versions inside individual `.csproj` files unless unavoidable.
- For EF migrations, run from `src/Infrastructure` (existing project convention) rather than adding startup/project path flags.

---

## Frontend — Big picture architecture

- React 19 SPA built with Vite, TypeScript, and Tailwind CSS v4.
- Feature-sliced structure under `frontend/src/features/{feature}/` with sub-folders: `components/`, `hooks/`, `stores/`.
- Shared UI components live in `frontend/src/components/`.
- App-level routing lives in `frontend/src/app/` split by auth boundary (`auth/`, `main/`).
- Global providers (React Query, i18n, Router) are wired in `frontend/src/providers/`.

## Frontend — Data fetching conventions

- All API calls use **TanStack Query** (`useQuery`, `useMutation`) via custom hooks in `frontend/src/api/endpoints/`.
- Axios instance is configured in `frontend/src/config/axios.ts`.
- Query keys are centralised in `frontend/src/api/QueryKeys.ts` — always use the key factory, never inline strings.
- Response/request DTOs are TypeScript types in `frontend/src/types/{feature}/`.
- `onSuccess` in mutations must call `context.client.invalidateQueries(...)` to keep the cache consistent.

## Frontend — Code style rules

- TypeScript strict mode — no `any`, no `!` non-null assertions.
- Functional components only — no class components.
- Forms use **React Hook Form** with **Zod** resolvers; form DTOs live in `src/types/{feature}/forms/`.
- Global state (non-server state) uses **Zustand** stores in `features/{feature}/stores/`.
- Internationalisation via **i18next** — all user-visible strings go through `t()`. Translation keys live in `frontend/public/locales/en.json`.
- Styling: Tailwind CSS v4 utility classes; component variants via `class-variance-authority` (`cva`); class merging via `tailwind-merge` + `clsx` (aliased as `cn()`).
- Use `shadcn/ui` component patterns (Radix UI primitives + `cn()`) for all new UI components.
