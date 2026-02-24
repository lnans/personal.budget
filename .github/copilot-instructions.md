# Copilot instructions for Personal.Budget

## Big picture architecture

- Solution follows Clean Architecture with pragmatic EF Core usage in `Application` (`src/Api`, `src/Application`, `src/Domain`, `src/Infrastructure`).
- Typical request flow: Minimal API endpoint -> `ICommandHandler`/`IQueryHandler` resolution from DI -> handler in `Application.Features.*` -> domain methods/entities -> `IAppDbContext` persistence -> `ErrorOr<T>` mapped to HTTP Problem Details.
- Endpoints are discovered by reflection via `IEndPoints` and `MapApiEndpoints()` (`src/Api/Configurations/EndpointsConfiguration.cs`), so new endpoint modules must implement `IEndPoints`.
- API startup wiring lives in `src/Api/Program.cs` and delegates per-layer DI to `AddApiServices`, `AddApplicationServices`, `AddInfrastructureServices`.

## Request/response and error handling patterns

- Use CQRS folders per feature: `Features/{Feature}/Commands/{Action}` and `Features/{Feature}/Queries/{Action}` (see `CreateAccount*` in `src/Application/Features/Accounts/Commands/CreateAccount/`).
- Command folders usually contain `{Action}Command.cs`, `{Action}Handler.cs`, `{Action}Response.cs`, `{Action}Validator.cs`; query folders usually contain `{Action}Query.cs`, `{Action}Handler.cs`, `{Action}Response.cs`.
- Commands/queries use custom handler interfaces (`ICommandHandler<TCommand>`, `ICommandHandler<TCommand,TResponse>`, `IQueryHandler<TQuery,TResponse>`); write operations generally return `ErrorOr<Response>`.
- Keep business validation in both FluentValidation and domain factories/methods. Example: `CreateAccountValidator` + `Account.Create(...)`.
- API handlers should return `result.ToOkResultOrProblem(context)` to map `ErrorOr` to RFC7807 payloads (`src/Api/Extensions/ResultExtensions.cs`, `src/Api/Errors/Problems.cs`).
- Cross-cutting concerns are implemented with decorators registered in `AddApplicationServices`: `ValidationDecorator` and `LoggingDecorator` around command/query handlers; validators are auto-registered from the `Application` assembly.
- Decorator order matters: `LoggingDecorator` is the outer layer and `ValidationDecorator` runs before the concrete handler.

## Domain and persistence conventions

- Domain entities inherit `Entity` (`Id` as GUID v7, `CreatedAt`, `UpdatedAt`, `DeletedAt`). Soft delete is timestamp-based.
- Prefer domain errors from `*Errors.cs` using stable codes (e.g., `Account.Name.Required` in `src/Domain/Accounts/AccountErrors.cs`).
- `IAuthContext.CurrentUserId` is the boundary for authenticated user identity; API implementation reads JWT claim `nameidentifier`/`sub`.
- `AppDbContextInitializer` runs migrations at startup and seeds default user from `BUDGET_USER` / `BUDGET_PASSWORD` when DB is empty.

## Critical workflows

- Build: `dotnet build Personal.Budget.sln`
- Run API locally with containers: `docker-compose up -d --build` (or `./run-server.sh`).
- Run tests with project convention: `./run-test.sh` (uses Microsoft Testing Platform via `dotnet run` on test projects).
- Coverage report: `./run-test.sh --coverage` (HTML generated in `.coverage/`).
- Format: `./format.sh` (runs `dotnet format` + `dotnet csharpier` and excludes migrations).

## Testing and integration specifics

- `tests/Api.Tests` are integration tests using Testcontainers PostgreSQL + `WebApplicationFactory` + Respawn reset between tests (`ApiTestFixture`).
- `tests/Domain.Tests` are pure domain unit tests with shared assertions in `TestBase`.
- Keep API test state isolated by using fixture helpers (`ApiTestBase`, `ResetFixtureStateAsync`, `CreateFreshScope`).
- API tests should inherit from `ApiTestBase` and use collection fixture wiring from `ApiTestCollection`.
- Test naming convention: `{Method}_{Scenario}_{ExpectedResult}` for API and domain tests.
- Domain test data factories live in `tests/TestFixtures/Domain` (pattern: `{Entity}Fixture.CreateValid{Entity}(...)`).

## Project-specific guardrails

- This repo is strict on style/analyzers (`.editorconfig`, `TreatWarningsAsErrors=true`); prefer `var`, file-scoped namespaces, and pass cancellation tokens through async calls.
- Style specifics to respect: avoid `this.` qualifiers, prefer keyword types (`string` not `String`), use braces, and keep Allman new-line brace formatting.
- Prefer modern C# idioms already enforced here: pattern matching, null-propagation, collection/object initializers, expression-bodied members where appropriate.
- Central package versions are managed in `Directory.Packages.props`; do not pin versions inside individual `.csproj` files unless unavoidable.
- For EF migrations, run from `src/Infrastructure` (existing project convention) rather than adding startup/project path flags.
