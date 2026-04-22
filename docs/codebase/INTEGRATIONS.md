# External Integrations

## Core Sections (Required)

### 1) Integration Inventory

| System              | Type                  | Purpose                                                | Auth model                                  | Criticality | Evidence                                                                   |
| ------------------- | --------------------- | ------------------------------------------------------ | ------------------------------------------- | ----------- | -------------------------------------------------------------------------- |
| PostgreSQL 16       | Database (relational) | Primary data store — accounts, operations, tags, users | Connection string (user/password)           | High        | docker-compose.yml, backend/src/Infrastructure/Persistence/AppDbContext.cs |
| JWT (self-issued)   | Auth mechanism        | Stateless authentication for API calls                 | HMAC-SHA256 signed JWTs (`Auth__SecretKey`) | High        | docker-compose.yml, backend/src/Api/Authentication/                        |
| Scalar (OpenAPI UI) | Dev tooling           | Interactive API docs at `/docs`                        | None (dev-time)                             | Low         | backend/Directory.Packages.props, backend/src/Api/Program.cs               |

No third-party external APIs, message queues, email services, or cloud providers are used.

---

### 2) Data Stores

| Store                         | Role                                             | Access layer                                 | Key risk                                                   | Evidence                                               |
| ----------------------------- | ------------------------------------------------ | -------------------------------------------- | ---------------------------------------------------------- | ------------------------------------------------------ |
| PostgreSQL 16                 | Single relational store for all application data | EF Core (`AppDbContext`) via Npgsql provider | Single point of failure; no read replicas or caching layer | backend/src/Infrastructure/Persistence/AppDbContext.cs |
| Docker volume `postgres-data` | Persistent storage for DB container              | Docker Compose volume mount                  | Volume loss = data loss in containerised deployments       | docker-compose.yml                                     |

No cache layer (Redis, Memcached) exists.

---

### 3) Authentication Flow

```
POST /auth/login  →  validate credentials (PasswordHasher PBKDF2)
                  →  generate JWT (exp: Auth__ExpirationMinutes, default 60 min)
                       + RefreshToken (exp: Auth__RefreshTokenExpirationDays, default 7 days)
                  ← { token, expireAt, refreshToken, refreshTokenExpireAt }

Authenticated request →  Authorization: Bearer <token>
                      →  ASP.NET Core JwtBearer validates signature, issuer, audience
                      →  IAuthContext.CurrentUserId resolved from `nameidentifier`/`sub` claim

POST /auth/refresh →  validate refresh token
                   →  issue new JWT + new refresh token pair
```

Evidence: docker-compose.yml (env vars), backend/src/Api/Authentication/AuthTokenGenerator.cs, backend/src/Infrastructure/Authentication/PasswordHasher.cs

---

### 4) Secrets and Credentials Handling

- **Backend secrets**: passed as environment variables at runtime (`Auth__SecretKey`, `DB_PASSWORD`, `BUDGET_USER`, `BUDGET_PASSWORD`)
- **Compose defaults**: weak defaults supplied inline (`your-super-secret-key-...`, `admin`/`admin`) — suitable only for local dev
- **Production override**: root `.env` or `backend/.env.production` (gitignored) overrides defaults via `run-server.sh`
- **Frontend**: `VITE_API_URL` baked at image build time from `frontend/.env` or `frontend/.env.production`
- No secrets manager, Vault, or cloud-native secret store is used
- **Hardcoding check**: No credentials hardcoded in source; all via env vars in compose and templates

---

### 5) Reliability and Failure Behavior

- **Database healthcheck**: compose `depends_on` with `pg_isready` health check — API starts only after DB is healthy
- **API healthcheck**: `GET /health` endpoint; compose checks every 30s with 3 retries
- **Retry/backoff**: No client-side retry or backoff configured in EF Core or Axios
- **Timeout policy**: No explicit HTTP or DB query timeouts configured beyond defaults
- **Circuit breaker**: None
- **Frontend token refresh**: `AuthProvider` schedules automatic refresh 5 minutes before JWT expiry; falls back to re-login if refresh fails

---

### 6) Observability for Integrations

- **Logging**: Serilog structured logs to console (JSON or plain, configured at startup); `LogMiddleware` logs all HTTP requests; `LoggingDecorator` logs all commands/queries with success/failure
- **EF Core query logging**: suppressed for `QueryExecutionPlanned` and `ContextInitialized` events (reduces noise)
- **No distributed tracing**: no OpenTelemetry, no APM agent configured
- **No metrics endpoint**: no Prometheus `/metrics` or similar
- **Missing visibility**: no per-endpoint latency, no DB query duration metrics, no alerting

---

### 7) Evidence

- docker-compose.yml
- backend/src/Infrastructure/Persistence/AppDbContext.cs
- backend/src/Infrastructure/Persistence/AppDbContextInitializer.cs
- backend/src/Infrastructure/Authentication/PasswordHasher.cs
- backend/src/Api/Authentication/ (AuthContext.cs, AuthTokenGenerator.cs, AuthTokenOptions.cs)
- backend/src/Api/Program.cs (CORS, forwarded headers, health endpoint)
- frontend/src/config/axios.ts
- backend/.env.production.template
- frontend/env.template
