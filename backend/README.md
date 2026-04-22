<p align="center">
    <img src="../budget.png" />
</p>

# Budget. API

Backend for **Budget.**: .NET 10, **Clean Architecture**, and **CQRS**. See the [repository README](../README.md) for product overview and how to run the full stack with Docker.

## Technology Stack

- **.NET 10.0** - Latest .NET framework
- **Entity Framework Core 10.0** - ORM with PostgreSQL provider
- **PostgreSQL** - Primary database
- **Scrutor** - Assembly scanning and dependency injection
- **FluentValidation** - Validation library
- **Scalar** - ASP.NET Core utilities for API endpoints documentation
- **ErrorOr** - Functional error handling library
- **Ardalis.GuardClauses** - Guard clauses for input validation
- **xUnit v3** - Testing framework
- **Microsoft Testing Platform** - Native test runner with code coverage support
- **Shouldly** - Assertion library
- **Testcontainers** - Containerization for testing
- **Serilog** - Logging library
- **NSubstitute** - Mocking library
- **Respawn** - Database reset library
- **ReportGenerator** - Code coverage report generator

## Architecture

This project follows **Clean Architecture** principles with clear separation of concerns:

```
src/
├── Api/
│   ├── Configurations/ # API configurations
│   ├── Endpoints/      # API endpoints
│   ├── Middleware/     # API middleware
├── Domain/             # Domain business rules and entities
├── Application/
│   ├── Interfaces/     # Application interfaces
│   ├── Behaviors/      # Application behaviors
│   ├── Features/       # Application features
└── Infrastructure/
    ├── Persistence/    # Persistence implementation
    └── Authentication/ # Authentication implementation
```

> NB: But with pragmatic approach about EF Core, which is referenced in the Application layer.

### Layers

- **Api**: Contains API configurations, endpoints, and middleware. Depends on Application and Infrastructure.
- **Application**: Defines application-specific business rules and interfaces. Depends only on Domain.
- **Domain**: Contains core business logic, entities, and domain errors. No dependencies on other layers.
- **Infrastructure**: Implements external concerns like database access. Depends on Application and Domain.

## Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) or later
- [PostgreSQL](https://www.postgresql.org/download/) database server
- [Docker](https://www.docker.com/products/docker-desktop/) for containerization
- (Optional) [.NET Tools](https://learn.microsoft.com/en-us/dotnet/core/tools/global-tools) for code formatting and EF Core tools for migrations

## Run the project

Docker Compose is defined at the **repository root** (monorepo: `backend/` + `frontend/`). From the repo root:

```bash
# Build and start database, API, and web (recommended)
./run-server.sh

# Or using Docker Compose v2 from the repo root
docker compose up -d --build

# View logs
docker compose logs -f budget-api

# Stop services
docker compose down

# Stop and remove volumes (destroys database data in the volume)
docker compose down -v
```

No env files are required for local Docker; compose defaults are enough. To override API or DB settings, copy [`backend/.env.production.template`](.env.production.template) to `backend/.env.production` and/or add a `.env` at the [repository root](../README.md#local-development).

- API: http://localhost:8080
- API Documentation: http://localhost:8080/docs
- Default User: admin
- Default Password: admin

## Development

To run the API, you need to have a PostgreSQL database server running. You can use Docker to run a PostgreSQL container:

```bash
docker run -d --name budget-postgres -p 5432:5432 -e POSTGRES_USER=postgres -e POSTGRES_PASSWORD=postgrespw -e POSTGRES_DB=budget-sqldb postgres
```

Then, you can run the API:

```bash
dotnet run --project src/Api/Api.csproj
```

## Code Coverage

Generate and view code coverage report:

```bash
./run-test.sh --coverage
```

This script will:

1. Run all tests with coverage enabled
2. Generate a Cobertura coverage report
3. Create an HTML report using ReportGenerator
4. Open the coverage report in your browser

## Code Formatting

Format the codebase:

```bash
./format.sh
```

This script uses:

- `dotnet format` for standard .NET formatting
- `CSharpier` for additional code style enforcement

Note: Migration files are excluded from formatting.

## Configuration

The project uses centralized package management:

- `Directory.Build.props` - Common build properties
- `Directory.Packages.props` - Centralized NuGet package versions
- `global.json` - .NET SDK version pinning

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Codebase documentation

Detailed documentation for the full monorepo lives under [`docs/codebase/`](../docs/codebase/):

| Document | Description |
| -------- | ----------- |
| [`STACK.md`](../docs/codebase/STACK.md) | Languages, runtimes, frameworks, and all key dependencies |
| [`STRUCTURE.md`](../docs/codebase/STRUCTURE.md) | Directory layout, entry points, and module boundaries |
| [`ARCHITECTURE.md`](../docs/codebase/ARCHITECTURE.md) | Architectural style, request lifecycle, and design patterns |
| [`CONVENTIONS.md`](../docs/codebase/CONVENTIONS.md) | Naming, formatting, error handling, and coding standards |
| [`INTEGRATIONS.md`](../docs/codebase/INTEGRATIONS.md) | External services, database, authentication, and observability |
| [`TESTING.md`](../docs/codebase/TESTING.md) | Test stack, layout, scope matrix, and isolation strategy |
| [`CONCERNS.md`](../docs/codebase/CONCERNS.md) | Known risks, technical debt, and security concerns |
