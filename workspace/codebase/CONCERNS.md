# Codebase Concerns

## Core Sections (Required)

### 1) Top Risks (Prioritized)

| Severity | Concern                                                                 | Evidence                                               | Impact                                                                                                  | Suggested action                                                                           |
| -------- | ----------------------------------------------------------------------- | ------------------------------------------------------ | ------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------ |
| High     | CORS is fully open (`AllowAnyOrigin / AllowAnyHeader / AllowAnyMethod`) | backend/src/Api/Program.cs                             | Any origin can call the API; relevant if the stack is ever exposed publicly                             | Scope CORS to known frontend origin(s) via configuration                                   |
| High     | No refresh-token revocation                                             | backend/src/Api/Authentication/, docker-compose.yml    | A stolen refresh token is valid for 7 days with no server-side ability to invalidate it                 | Store refresh tokens in DB; add revocation endpoint                                        |
| Medium   | `VITE_API_URL` baked at image build time                                | frontend/Dockerfile, frontend/src/env.ts               | Changing API host requires a full frontend image rebuild                                                | Consider runtime env injection via nginx env-subst or a server-side config endpoint        |
| Medium   | Weak compose defaults in source                                         | docker-compose.yml                                     | `admin`/`admin` credentials and weak JWT key in default config; risk if compose is run without override | Document mandatory override checklist; consider fail-fast guard if default key is detected |
| Low      | No query result caching                                                 | backend/src/Infrastructure/Persistence/AppDbContext.cs | All reads hit the DB directly; acceptable for personal-scale use, not for higher load                   | Add in-memory or distributed cache for read-heavy queries if usage grows                   |
| Low      | No frontend test suite (planned)                                        | frontend/package.json                                  | Component regressions caught only by manual testing until implemented                                   | Add Vitest + React Testing Library for critical components                                 |

---

### 2) Technical Debt

| Debt item                              | Why it exists                                                                                 | Where                                                                                             | Risk if ignored                                             | Suggested fix                                                                     |
| -------------------------------------- | --------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------- | ----------------------------------------------------------- | --------------------------------------------------------------------------------- |
| Single-user seeding only               | **By design** — private deployment; self-registration is intentionally not supported          | backend/src/Infrastructure/Persistence/AppDbContextInitializer.cs                                 | N/A (intentional)                                           | No action required                                                                |
| No pagination cursor strategy          | `PaginatedQuery` uses offset pagination                                                       | backend/src/Application/Models/PaginatedQuery.cs                                                  | Offset pagination degrades at large offsets                 | Switch to keyset (cursor) pagination for `AccountOperations`                      |
| No frontend tests                      | Frontend was added later; test infra not yet set up                                           | frontend/                                                                                         | Regressions caught only by manual testing until implemented | **Planned** — Vitest + React Testing Library; prioritise auth store and API hooks |
| No distributed tracing / APM           | Project is personal-scale; not yet needed                                                     | backend/src/Api/Program.cs                                                                        | Debugging production issues is log-only                     | Add OpenTelemetry SDK when usage grows                                            |
| `TimeProvider.System` registered twice | Registered in both `Application.DependencyInjection` and `Infrastructure.DependencyInjection` | backend/src/Application/DependencyInjection.cs, backend/src/Infrastructure/DependencyInjection.cs | Last registration wins (singleton); subtle if ever changed  | Remove duplicate from one of the two layers                                       |

---

### 3) Security Concerns

| Risk                                       | OWASP category                                   | Evidence                                                    | Current mitigation                                                                                                                                       | Gap                                                                 |
| ------------------------------------------ | ------------------------------------------------ | ----------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------- |
| Weak default JWT secret                    | A02 – Cryptographic Failures                     | docker-compose.yml (`your-super-secret-key-...`)            | Documented that production must override; key is an env var                                                                                              | No runtime guard; app starts with weak key if not overridden        |
| Weak default credentials (`admin`/`admin`) | A07 – Identification and Authentication Failures | docker-compose.yml                                          | Env-var based; `run-server.sh` loads `.env` overrides                                                                                                    | No runtime warning or rejection of default passwords                |
| Open CORS policy                           | A05 – Security Misconfiguration                  | backend/src/Api/Program.cs                                  | None                                                                                                                                                     | Must be scoped to allowed origin before any public exposure         |
| No refresh-token rotation/revocation       | A07 – Identification and Authentication Failures | backend/src/Api/Authentication/                             | Tokens expire after 7 days                                                                                                                               | Compromise window is 7 days; no server-side invalidation            |
| Password hashing                           | A02 – Cryptographic Failures                     | backend/src/Infrastructure/Authentication/PasswordHasher.cs | PBKDF2-HMAC-SHA256 with 600,000 iterations (OWASP-recommended), 16-byte random salt, constant-time compare via `CryptographicOperations.FixedTimeEquals` | None — implementation is correct                                    |
| Error details in Problem responses         | A05 – Security Misconfiguration                  | backend/src/Api/Extensions/ErrorExtensions.cs               | Error codes are stable domain strings, not stack traces                                                                                                  | Verify no stack trace or internal detail leaks in Unexpected errors |

---

### 4) Performance and Scaling Concerns

| Concern                                     | Evidence                                                    | Current symptom        | Scaling risk                                                                                                                                     | Suggested improvement                                                     |
| ------------------------------------------- | ----------------------------------------------------------- | ---------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------ | ------------------------------------------------------------------------- |
| Offset pagination on `AccountOperations`    | backend/src/Application/Models/PaginatedQuery.cs            | None at current scale  | Slow queries as operation count grows                                                                                                            | Switch to keyset pagination                                               |
| No DB connection pooling configuration      | backend/src/Infrastructure/DependencyInjection.cs           | None at current scale  | Connection pool exhaustion under concurrent load                                                                                                 | Configure Npgsql pool size explicitly                                     |
| Balance computed at operation-creation time | backend/src/Domain/Accounts/Account.cs, AccountOperation.cs | None at current scale  | Balance is recalculated on delete and update (confirmed). Verify recalculation covers all downstream operations when one is modified mid-history | Ensure integration tests cover balance correctness after mid-list updates |
| No caching                                  | All reads hit PostgreSQL                                    | None at personal scale | Latency grows with data volume                                                                                                                   | Add response caching or in-memory read-through cache for accounts list    |

---

### 5) Fragile / High-Churn Areas

| Area                                       | Why fragile                                                          | Churn signal                         | Safe change strategy                                                             |
| ------------------------------------------ | -------------------------------------------------------------------- | ------------------------------------ | -------------------------------------------------------------------------------- |
| `AccountsEndpoints.cs`                     | Combines account CRUD + nested operation endpoints; growing in scope | 11 commits (top churn file, 90 days) | Split into `AccountsEndpoints.cs` + `AccountOperationsEndpoints.cs` at API layer |
| `UpdateAccountOperation` (command + tests) | Complex update logic with balance recalculation                      | 7 commits on test file, 5 on handler | Add focused unit tests for the balance side-effect before changes                |
| `AddAccountOperation` (command + tests)    | Recurring flag and tags added recently                               | 6 commits on test file, 5 on handler | Same as above; ensure `IsRecurring` + tag attachment is covered                  |
| `Account.cs` (domain entity)               | Core aggregate receiving new features                                | 6 commits                            | Use `Domain.Tests` unit tests to gate domain changes before integration          |
| `PaginatedQuery.cs`                        | Pagination model changed multiple times                              | 6 commits                            | Stabilise model; add architecture test to prevent ad-hoc changes                 |

---

### 6) Resolved Questions

1. **PasswordHasher algorithm**: PBKDF2-HMAC-SHA256 with 600,000 iterations and a 16-byte random salt. Meets OWASP recommendations. No action needed.
2. **Balance recalculation**: Confirmed — balance is recalculated when an operation is deleted or updated.
3. **Single-user model**: Intentional. This is a private deployment; self-registration is not planned.
4. **CI coverage threshold**: Not planned for now.
5. **Frontend tests**: Planned for a future iteration (Vitest + React Testing Library).

---

### 7) Evidence

- docs/codebase/.codebase-scan.txt (High-Churn Files, TODO/FIXME/HACK sections)
- backend/src/Api/Program.cs (CORS config)
- docker-compose.yml (default credentials, JWT env vars)
- backend/src/Application/Models/PaginatedQuery.cs
- backend/src/Infrastructure/Persistence/AppDbContextInitializer.cs
- backend/src/Infrastructure/Authentication/PasswordHasher.cs
- backend/src/Api/Extensions/ErrorExtensions.cs
