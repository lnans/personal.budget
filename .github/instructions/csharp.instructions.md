---
description: "Guidelines for building C# applications"
applyTo: "**/*.cs"
---

# C# Development

## C# Instructions

- Always use the latest version C#, currently C# 14 features.
- Do not add routine comments to every function; add comments only when behavior is non-obvious.

## General Instructions

- Make only high confidence suggestions when reviewing code changes.
- Write code with good maintainability practices and clear naming first; document design decisions only when context is not obvious from code.
- Handle edge cases and write clear exception handling.
- For libraries or external dependencies, keep usage explicit in code and architecture notes rather than inline boilerplate comments.

## Naming Conventions

- Follow PascalCase for component names, method names, and public members.
- Use camelCase for private fields and local variables.
- Prefix interface names with "I" (e.g., IUserService).

## Formatting

- Apply code-formatting style defined in `.editorconfig`.
- Prefer file-scoped namespace declarations.
- Keep `using` directives outside namespaces, with `System.*` first and no extra grouping.
- Use `var` consistently for local declarations.
- Avoid `this.` qualification unless required for disambiguation.
- Insert a newline before the opening curly brace of any code block (e.g., after `if`, `for`, `while`, `foreach`, `using`, `try`, etc.).
- Use pattern matching and switch expressions wherever possible.
- Use `nameof` instead of string literals when referring to member names.
- Prefer expression-bodied members where suitable, but do not use primary constructors.
- Prefer null propagation, object/collection initializers, tuple swap, and simplified `using` statements.
- Respect analyzer-backed constraints such as forwarding `CancellationToken` and avoiding unused parameters.

## Project Setup and Structure

- Guide users through creating a new .NET project with the appropriate templates.
- Explain the purpose of each generated file and folder to build understanding of the project structure.
- Demonstrate how to organize code using feature folders or domain-driven design principles.
- Show proper separation of concerns with models, services, and data access layers.
- Explain the Program.cs and configuration system in ASP.NET Core 10 including environment-specific settings.

## Nullable Reference Types

- Declare variables non-nullable, and check for `null` at entry points.
- Always use `is null` or `is not null` instead of `== null` or `!= null`.
- Trust the C# null annotations and don't add null checks when the type system says a value cannot be null.

## Data Access Patterns

- Use Entity Framework Core with `IAppDbContext` for data access.
- Database is PostgreSQL via `Npgsql.EntityFrameworkCore.PostgreSQL`.
- EF Core is pragmatically referenced in the Application layer (via `IAppDbContext`) rather than strictly isolated to Infrastructure.
- For EF migrations, run from `src/Infrastructure` (existing project convention).
- Explain efficient query patterns to avoid common performance issues.

## Authentication and Authorization

- Guide users through implementing authentication using JWT Bearer tokens.
- Explain OAuth 2.0 and OpenID Connect concepts as they relate to ASP.NET Core.
- Show how to implement role-based and policy-based authorization.
- Demonstrate integration with Microsoft Entra ID (formerly Azure AD).
- Explain how to secure both controller-based and Minimal APIs consistently.

## Validation and Error Handling

- Use `ErrorOr<T>` for functional error handling instead of exceptions in domain methods and handlers.
- Guide the implementation of model validation using FluentValidation.
- Keep business validation in both FluentValidation validators and domain factory/mutation methods.
- Domain errors are defined in `*Errors.cs` with stable codes (e.g., `Account.Name.Required`); use `Error.Validation(...)`, `Error.NotFound(...)`, etc.
- Handlers return `ErrorOr<TResponse>` and the API layer maps errors to RFC 7807 Problem Details via `ToOkResultOrProblem()`.
- Never throw or catch business exceptions; let `ErrorOr<T>` flow through the chain.

## Domain Entity Conventions

- Domain entities inherit `Entity` base class (GUID v7 `Id`, `CreatedAt`, `UpdatedAt`, nullable `DeletedAt` for soft delete).
- Use **private constructors** and static **factory methods** (`Entity.Create(...)`) that return `ErrorOr<Entity>` to enforce domain validation at creation.
- All business rules, validation, state transitions, and invariant checks live exclusively on **domain entity methods** returning `ErrorOr<T>`.
- Never put business logic in handlers — handlers only load entities, call domain methods, persist, and map to response DTOs.

## Handler Conventions (Thin Handlers, Rich Domain)

- Handlers contain **absolutely no business logic**. They are thin orchestrators: load from `IAppDbContext` → call domain entity methods → persist via `SaveChangesAsync` → map to response DTO.
- Handlers must NOT: check field lengths or null values, compute derived state, throw or catch business exceptions, or contain conditional branching based on business rules.
- Use the ErrorOr chaining API to compose handler pipelines in a railway-oriented style:
    - `Then` / `ThenAsync` — for steps that can produce a new error (lambda returns `ErrorOr<T>`).
    - `ThenDo` / `ThenDoAsync` — for side effects that cannot fail (lambda returns `void` / `Task`).
    - `MatchFirst` — terminal step to convert to the final response type.
- Always end a handler chain with `MatchFirst`.

## API Versioning and Documentation

- Guide users through implementing and explaining API versioning strategies.
- Demonstrate Swagger/OpenAPI implementation with proper documentation.
- Show how to document endpoints, parameters, responses, and authentication.
- Explain versioning in both controller-based and Minimal APIs.
- Guide users on creating meaningful API documentation that helps consumers.

## Logging and Monitoring

- Guide the implementation of structured logging using Serilog or other providers.
- Explain the logging levels and when to use each.
- Demonstrate integration with Application Insights for telemetry collection.
- Show how to implement custom telemetry and correlation IDs for request tracking.
- Explain how to monitor API performance, errors, and usage patterns.

## Testing

- Always include test cases for critical paths of the application.
- Guide users through creating unit tests.
- Do not emit "Act", "Arrange" or "Assert" comments.
- Copy existing style in nearby files for test method names and capitalization.
- Test naming convention: `{Method}_{Scenario}_{ExpectedResult}`.
- Domain tests are pure unit tests inheriting `TestBase`; API tests are integration tests inheriting `ApiTestBase` using Testcontainers PostgreSQL + `WebApplicationFactory` + Respawn.
- Use domain test data factories from `tests/TestFixtures/Domain/` (pattern: `{Entity}Fixture.CreateValid{Entity}(...)`).

## Performance Optimization

- Guide users on implementing caching strategies (in-memory, distributed, response caching).
- Explain asynchronous programming patterns and why they matter for API performance.
- Demonstrate pagination, filtering, and sorting for large data sets.
- Show how to implement compression and other performance optimizations.
- Explain how to measure and benchmark API performance.

## Deployment and DevOps

- Guide users through containerizing their API using .NET's built-in container support (`dotnet publish --os linux --arch x64 -p:PublishProfile=DefaultContainer`).
- Explain the differences between manual Dockerfile creation and .NET's container publishing features.
- Explain CI/CD pipelines for NET applications.
- Demonstrate deployment to Azure App Service, Azure Container Apps, or other hosting options.
- Show how to implement health checks and readiness probes.
- Explain environment-specific configurations for different deployment stages.

## Style rules enforced by analyzers

- Namespace must match folder structure.
- Prefer keyword types (`string`, `int`) over BCL aliases (`String`, `Int32`).
- Always use braces for control-flow statements (Allman style).
- Keep max line length at 120 characters for C# files.
- Do not use primary constructors.
