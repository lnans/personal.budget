# Coding Conventions

## Core Sections (Required)

### 1) Naming Rules

#### Backend (C#)

| Item                         | Rule                                              | Example                                                | Evidence                                     |
| ---------------------------- | ------------------------------------------------- | ------------------------------------------------------ | -------------------------------------------- |
| Files                        | PascalCase, matches type name                     | `CreateAccountHandler.cs`                              | backend/src/Application/Features/            |
| Classes / interfaces         | PascalCase                                        | `ICommandHandler<T>`, `AccountErrors`                  | backend/src/Application/Interfaces/          |
| Methods                      | PascalCase                                        | `Handle(...)`, `Create(...)`                           | backend/src/Domain/Accounts/Account.cs       |
| Local variables / parameters | camelCase                                         | `command`, `cancellationToken`                         | backend/src/Application/Features/            |
| Private fields               | camelCase, no underscore prefix                   | `_dbContext` (underscore IS used for injected deps)    | backend/src/Application/Features/            |
| Constants                    | PascalCase in static class                        | `AccountConstants.MaxNameLength`                       | backend/src/Domain/Accounts/                 |
| Error codes                  | `Entity.Property.Issue` string format             | `"Account.Name.Required"`                              | backend/src/Domain/Accounts/AccountErrors.cs |
| Namespace                    | Matches folder path (enforced by `.editorconfig`) | `Application.Features.Accounts.Commands.CreateAccount` | backend/.editorconfig                        |

#### Frontend (TypeScript/React)

| Item                     | Rule                               | Example                             | Evidence                                     |
| ------------------------ | ---------------------------------- | ----------------------------------- | -------------------------------------------- |
| React components / pages | PascalCase                         | `AuthPage.tsx`, `MainLayout.tsx`    | frontend/src/app/                            |
| Hooks                    | camelCase, `use` prefix            | `useMediaQuery.ts`, `useAuthStore`  | frontend/src/hooks/                          |
| Utility functions        | camelCase                          | `cn()`, `lazyImport()`              | frontend/src/lib/                            |
| Types / interfaces       | PascalCase                         | `GetAccountsResponse`, `Problem`    | frontend/src/types/                          |
| Zustand stores           | camelCase, `use*Store` convention  | `useAuthStore`                      | frontend/src/features/authentication/stores/ |
| Query key factory keys   | camelCase object keys              | `queryKeys.accounts.all`            | frontend/src/api/QueryKeys.ts                |
| i18n keys                | dot-separated lowercase namespaces | `errors.NetworkError`, `auth.login` | frontend/public/locales/en.json              |
| Env variables            | `VITE_` prefix (Vite convention)   | `VITE_API_URL`                      | frontend/src/env.ts                          |

---

### 2) Formatting and Linting

#### Backend

- **Formatter**: `dotnet csharpier` (opinionated) + `dotnet format` (Roslyn style)
- **Config**: backend/.editorconfig (130+ rules, `TreatWarningsAsErrors=true`)
- **Run**: `./format.sh` from `backend/`
- Key enforced rules:
  - `var` always preferred (`csharp_style_var_elsewhere = true:error`)
  - File-scoped namespaces (`csharp_style_namespace_declarations = file_scoped:error`)
  - `this.` qualifier forbidden
  - Keyword types enforced (`string` not `String`)
  - Allman brace style (new line before `{`)
  - System using directives sorted first
  - Max line length: 120 characters

#### Frontend

- **Formatter**: Prettier 3 (`frontend/.prettierrc.json`)
- **Linter**: ESLint 9 (`frontend/eslint.config.js`) with plugins:
  - `typescript-eslint` (strict mode)
  - `eslint-plugin-react`, `eslint-plugin-react-hooks`, `eslint-plugin-react-x`, `eslint-plugin-react-dom`, `eslint-plugin-react-refresh`
  - `@tanstack/eslint-plugin-query`
  - `eslint-plugin-import`
  - `eslint-config-prettier` (disables formatting rules that conflict with Prettier)
- **Run**: `yarn lint:fix` / `yarn format` / `yarn validate` (all checks)
- TypeScript strict mode enabled (`tsconfig.app.json`)
- No `any`, no non-null `!` assertions

---

### 3) Import and Module Conventions

#### Backend

- Using directives: outside namespace, System namespaces first (editorconfig enforced)
- No barrel (`index.cs`) exports — each file is directly referenced

#### Frontend

- **Path aliases**: `@/` maps to `frontend/src/` (configured in `vite.config.ts` and `tsconfig.app.json`)
- Prefer `@/` absolute imports over relative imports for anything outside the same directory
- No barrel `index.ts` files observed — direct named imports from feature files
- i18n translations accessed via `t()` hook — no raw strings in UI code

---

### 4) Error and Logging Conventions

#### Backend

- **Domain layer**: Business invariants return `ErrorOr<T>` — never throw exceptions for business rules
- **Application layer**: Handlers chain `ErrorOr` via `.Then()` / `.ThenDo()` / `.ThenAsync()` / `.MatchFirst()`; validation errors short-circuit in `ValidationDecorator`
- **Api layer**: `result.ToOkResultOrProblem(context)` maps any `ErrorOr` error to RFC 7807 `ProblemDetails`
- **Logging**: Serilog structured logging; `LoggingDecorator` logs command/query name + error codes on failure; `LogMiddleware` logs all HTTP requests
- Error codes are stable string identifiers (`"Account.Name.Required"`) surfaced in `ProblemDetails.Extensions`

#### Frontend

- Axios error interceptor normalises all API errors to the `Problem` type (`frontend/src/types/Problem.ts`)
- Network errors produce a synthetic `Problem` with `type: "errors.NetworkError"`
- UI displays errors via `sonner` toasts and/or inline form validation messages

---

### 5) Testing Conventions

- Backend test naming: `{Method}_{Scenario}_{ExpectedResult}` (e.g., `Post_WithValidRequest_ReturnsCreated`)
- Integration tests inherit from `ApiTestBase`, use `ApiTestCollection` xUnit collection fixture
- Domain tests inherit from `TestBase`, use plain xUnit
- Test data factories live in `tests/TestFixtures/Domain/` — pattern: `{Entity}Fixture.CreateValid{Entity}(...)`
- Database reset between each test via Respawn (`ResetFixtureStateAsync`)
- Architecture constraints enforced in `Architecture.Tests/` via NetArchTest

---

### 6) Evidence

- backend/.editorconfig
- backend/Directory.Build.props (`TreatWarningsAsErrors=true`, `Nullable=enable`)
- frontend/eslint.config.js
- frontend/.prettierrc.json
- frontend/tsconfig.app.json
- backend/src/Application/Features/Accounts/Commands/CreateAccount/CreateAccountHandler.cs
- frontend/src/api/QueryKeys.ts
