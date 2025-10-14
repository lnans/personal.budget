<p align="center">
    <img src="./doc/logo.png" />
</p>

# Budget. API

**Budget.** is a personal finance management API designed to organize and track spending and savings over time. This repository contains the backend implementation built with modern .NET technologies, following **Clean Architecture** and **CQRS** principles.

## Technology Stack

-   **.NET 10.0** (RC) - Latest .NET framework
-   **Entity Framework Core 10.0** - ORM with PostgreSQL provider
-   **PostgreSQL** - Primary database
-   **ErrorOr** - Functional error handling library
-   **xUnit v3** - Testing framework
-   **Shouldly** - Assertion library
-   **Microsoft Testing Platform** - Native test runner with code coverage support

## Architecture

This project follows **Clean Architecture** principles with clear separation of concerns:

```
src/
├── Domain/           # Domain business rules
├── Application/      # Application layer
└── Infrastructure/   # External concerns (DB, etc.)
```

### Layers

-   **Domain**: Contains core business logic, entities, and domain errors. No dependencies on other layers.
-   **Application**: Defines application-specific business rules and interfaces. Depends only on Domain.
-   **Infrastructure**: Implements external concerns like database access. Depends on Application and Domain.

## Prerequisites

-   [.NET 10.0 SDK (RC)](https://dotnet.microsoft.com/download/dotnet/10.0) or later
-   [PostgreSQL](https://www.postgresql.org/download/) database server
-   (Optional) [.NET Tools](https://learn.microsoft.com/en-us/dotnet/core/tools/global-tools) for code formatting

## Getting Started

```bash
git clone https://github.com/lnans/personal.budget.git
cd personal.budget

dotnet build
dotnet test
```

## Code Coverage

Generate and view code coverage report:

```bash
./coverage.sh
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

---

**Note**: This project uses .NET 10.0 RC, which is a pre-release version. Ensure you have the correct SDK version installed as specified in `global.json`.
