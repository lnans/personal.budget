# Testing Patterns

## Core Sections (Required)

### 1) Test Stack and Commands

- **Primary test framework**: xunit.v3 (3.2.2) via Microsoft Testing Platform (`dotnet run` on test projects, not `dotnet test`)
- **Assertion library**: Shouldly 4.3.0
- **Mocking**: NSubstitute 5.3.0 (used in domain unit tests)
- **Integration DB**: Testcontainers.PostgreSql 4.10.0 (real Postgres container per fixture)
- **DB reset**: Respawn 7.0.0 (truncates tables between tests without rebuilding the schema)
- **Architecture testing**: NetArchTest.Rules 1.3.2
- **Coverage**: Microsoft.Testing.Extensions.CodeCoverage; HTML report in `backend/.coverage/`

```bash
# Run from backend/
./run-test.sh                  # all tests
./run-test.sh --coverage        # tests + HTML coverage in backend/.coverage/
```

---

### 2) Test Layout

```
backend/tests/
├── Api.Tests/          # Integration tests (HTTP-level)
│   ├── Accounts/
│   ├── AccountOperations/
│   ├── Authentication/
│   └── Tags/
├── Domain.Tests/       # Pure domain unit tests
│   ├── Accounts/
│   ├── AccountOperations/
│   ├── Tags/
│   └── Users/
├── Architecture.Tests/ # Structural/dependency rules
│   └── Application/
└── TestFixtures/       # Shared data factories
    └── Domain/         # {Entity}Fixture.CreateValid{Entity}(...)
```

- Test files are in a **dedicated `tests/` folder** (not co-located with source)
- Test project naming: `{Scope}.Tests.csproj`
- Test method naming: `{Method}_{Scenario}_{ExpectedResult}` (e.g., `Post_WithValidRequest_ReturnsCreatedAccount`)

---

### 3) Test Scope Matrix

| Scope                       | Covered? | Typical target                                                                   | Notes                                                                     |
| --------------------------- | -------- | -------------------------------------------------------------------------------- | ------------------------------------------------------------------------- |
| Unit (domain)               | Yes      | Domain entity factories + mutation methods                                       | Pure C# — no mocks needed; domain is framework-free                       |
| Integration (API)           | Yes      | All endpoint routes (`/accounts`, `/accounts/{id}/operations`, `/tags`, `/auth`) | Testcontainers Postgres + `WebApplicationFactory`; real EF migrations run |
| Architecture                | Yes      | Dependency direction, handler/validator placement, decorator rules               | NetArchTest; enforces Clean Architecture layer rules                      |
| Unit (application handlers) | No       | Command/query handlers                                                           | Handlers are thin; covered indirectly by API integration tests            |
| E2E (browser)               | No       | Full user flows                                                                  | No Playwright or Cypress configured                                       |
| Frontend (component/unit)   | No       | React components, hooks                                                          | No Vitest or Jest configured in frontend                                  |

---

### 4) Mocking and Isolation Strategy

- **Domain tests**: no mocking — domain entities are pure value objects/aggregates; `TimeProvider` is seeded with fixed `DateTimeOffset` values via `TestBase`
- **API integration tests**: real PostgreSQL container (no mocks for persistence); `IAuthContext` is replaced via `ApiFactory` with a test implementation that injects the test user's ID
- **Respawn reset**: `ResetFixtureStateAsync()` in `ApiTestFixture` truncates all data tables (except `__EFMigrationsHistory`) between test runs without tearing down the container
- **Per-collection fixture**: `ApiTestCollection` shares one `ApiTestFixture` (one Postgres container) across all integration tests in the collection; each test calls `ResetFixtureStateAsync` in its `InitializeAsync`
- **Test user**: seeded once per fixture lifecycle; token obtained via real `/auth/login` call in `ApiTestFixture.InitializeAsync`

---

### 5) Coverage and Quality Signals

- Coverage tool: `Microsoft.Testing.Extensions.CodeCoverage` (configured in `backend/tests/settings.coverage.xml`)
- Coverage threshold: [TODO] — no threshold enforced in CI configuration observed
- High-churn test files (last 90 days): `UpdateAccountOperationTests.cs` (7 commits), `AddAccountOperationTests.cs` (6 commits), `UpdateAccountTests.cs` (5 commits) — suggests active feature development in `AccountOperations`
- Architecture tests in `Architecture.Tests/` prevent layer violations from compiling

---

### 6) Evidence

- backend/tests/Api.Tests/ApiTestFixture.cs
- backend/tests/Api.Tests/ApiTestBase.cs
- backend/tests/Api.Tests/ApiTestCollection.cs
- backend/tests/Api.Tests/ApiFactory.cs
- backend/tests/Domain.Tests/TestBase.cs
- backend/tests/TestFixtures/FixtureBase.cs
- backend/tests/Architecture.Tests/TestRules.cs
- backend/tests/settings.coverage.xml
- backend/run-test.sh

## Extended Sections

### Integration Test Fixture Lifecycle

```
ApiTestFixture.InitializeAsync()
  → Start PostgreSqlContainer (Testcontainers)
  → Build ApiFactory (WebApplicationFactory with real DB connection)
  → Run EF migrations on test DB
  → Seed test user (hashed password, cached)
  → Create HttpClient
  → Open Npgsql connection for Respawn
  → Create Respawner (schema: public, ignore __EFMigrationsHistory)
  → Authenticate → store UserToken + UserRefreshToken

Per-test (ApiTestBase.InitializeAsync)
  → ResetFixtureStateAsync() — Respawn truncates tables
  → Re-seed test user
  → Re-authenticate (or use cached token)
```
