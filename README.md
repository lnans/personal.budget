<p align="center">
    <img src="./doc/logo.png" />
</p>

# Budget. API

**Budget.** is a personal finance management API designed to organize and track spending and savings over time. This repository contains the backend implementation built with modern .NET technologies, following **Clean Architecture** and **CQRS** principles.

## Technology Stack

-   **.NET 9.0** - Latest .NET framework
-   **Entity Framework Core 9.0** - ORM with PostgreSQL provider
-   **PostgreSQL** - Primary database
-   **MediatR** - Mediator pattern implementation
-   **Scalar** - ASP.NET Core utilities for API endpoints documentation
-   **ErrorOr** - Functional error handling library
-   **Ardalis.GuardClauses** - Guard clauses for input validation
-   **xUnit v3** - Testing framework
-   **Microsoft Testing Platform** - Native test runner with code coverage support
-   **Shouldly** - Assertion library
-   **Testcontainers** - Containerization for testing
-   **Serilog** - Logging library
-   **NSubstitute** - Mocking library
-   **Respawn** - Database reset library
-   **ReportGenerator** - Code coverage report generator

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

-   **Api**: Contains API configurations, endpoints, and middleware. Depends on Application and Infrastructure.
-   **Application**: Defines application-specific business rules and interfaces. Depends only on Domain.
-   **Domain**: Contains core business logic, entities, and domain errors. No dependencies on other layers.
-   **Infrastructure**: Implements external concerns like database access. Depends on Application and Domain.

## Prerequisites

-   [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) or later
-   [PostgreSQL](https://www.postgresql.org/download/) database server
-   [Docker](https://www.docker.com/products/docker-desktop/) for containerization
-   (Optional) [.NET Tools](https://learn.microsoft.com/en-us/dotnet/core/tools/global-tools) for code formatting and EF Core tools for migrations

## Getting Started

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

-   `dotnet format` for standard .NET formatting
-   `CSharpier` for additional code style enforcement

Note: Migration files are excluded from formatting.

## Configuration

The project uses centralized package management:

-   `Directory.Build.props` - Common build properties
-   `Directory.Packages.props` - Centralized NuGet package versions
-   `global.json` - .NET SDK version pinning

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
