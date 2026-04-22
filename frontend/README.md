<p align="center">
    <img src="../budget.png" />
</p>

# Budget. Web

Frontend for **Budget.**: React, TypeScript, and Vite. See the [repository README](../README.md) for product overview and how to run the full stack with Docker.

## Technology Stack

| Category         | Technology        |
| ---------------- | ----------------- |
| Framework        | React 19          |
| Language         | TypeScript 5.9    |
| Build Tool       | Vite 7            |
| Routing          | React Router 7    |
| Data Fetching    | TanStack Query 5  |
| State Management | Zustand 5         |
| Forms            | React Hook Form 7 |
| Validation       | Zod 4             |
| HTTP Client      | Axios             |
| Styling          | Tailwind CSS 4    |
| UI Components    | Radix UI          |
| i18n             | i18next           |
| Notifications    | Sonner            |
| Icons            | Lucide React      |
| Linting          | ESLint 9          |
| Formatting       | Prettier          |

## Architecture

This project follows **Clean Architecture** principles with clear separation of concerns:

```
src/
├── api/                # API client and query configuration
│   └── endpoints/      # API endpoint definitions
├── app/                # Application pages
│   ├── auth/           # Authentication pages
│   └── main/           # Main application layout and pages
├── components/         # Reusable UI components
│   ├── forms/          # Form components (controlled inputs)
│   └── ui/             # UI primitives (Button, Dialog, Sidebar, etc.)
├── config/             # Configuration files (Axios, i18next)
├── features/           # Feature modules (components, stores, logic)
│   └── {domain}/       # Domain-specific features
│       ├── components/ # Domain-specific components
│       └── stores/     # Domain-specific stores
├── hooks/              # Custom React hooks
├── lib/                # Utility functions
├── providers/          # React context providers
└── types/              # TypeScript type definitions
    └── {domain}/       # Domain-specific types
        ├── enums/      # Enum definitions
        ├── forms/      # Form DTOs (Zod schemas)
        ├── queries/    # Query definitions
        └── responses/  # Response DTOs
```

## Prerequisites

- [Node.js](https://nodejs.org/) (v22 or later)
- [Yarn](https://yarnpkg.com/) (v4.12.0 - included via Corepack)
- [Docker](https://www.docker.com/products/docker-desktop/) (optional, for containerization)

## Development

To run the web application in development mode:

```bash
# Install dependencies
yarn install

# Start development server
yarn dev
```

The development server will start at http://localhost:5173 (or the next available port).

### Available Scripts

| Script            | Description                          |
| ----------------- | ------------------------------------ |
| `yarn dev`        | Start development server             |
| `yarn build`      | Build for production                 |
| `yarn preview`    | Preview production build             |
| `yarn lint`       | Run ESLint                           |
| `yarn lint:fix`   | Run ESLint with auto-fix             |
| `yarn type:check` | Run TypeScript type checking         |
| `yarn format`     | Format code with Prettier            |
| `yarn validate`   | Run lint:fix, type:check, and format |

## Docker

Compose orchestrates the **whole stack** from the **repository root** (not from `frontend/`). From the repo root:

```bash
./run-server.sh

# Or
docker compose up -d --build

docker compose logs -f budget-web
docker compose down
```

Local Docker needs no env setup: the image build uses the committed `frontend/.env`. For production builds or custom API URLs, use `frontend/.env.production` (see `.env.production.template`)—`VITE_*` is read at **build** time.

- Web application (container): http://localhost:8081

## Running with Backend

The web application requires the backend API. To run the full application locally:

1. **Docker (full stack)**: From the repo root, run `./run-server.sh` (see [Docker](#docker)).

2. **Development (Vite + API in Docker or locally)**: The repo includes `frontend/.env` with defaults; run `yarn dev` as in [Development](#development). Use `env.template` only if you need to recreate `.env` or change `VITE_API_URL` / `VITE_LIST_PAGE_SIZE`.

## Environment Variables

| Variable       | Description                                      | Required |
| -------------- | ------------------------------------------------ | -------- |
| `VITE_API_URL` | Backend API URL (must be a valid HTTP/HTTPS URL) | Yes      |

## License

This project is licensed under the MIT License - see the [LICENSE](https://github.com/lnans/personal.budget/blob/main/LICENSE) file for details.

## Codebase documentation

Detailed documentation for the full monorepo lives under [`docs/codebase/`](../docs/codebase/):

| Document                                              | Description                                                    |
| ----------------------------------------------------- | -------------------------------------------------------------- |
| [`STACK.md`](../docs/codebase/STACK.md)               | Languages, runtimes, frameworks, and all key dependencies      |
| [`STRUCTURE.md`](../docs/codebase/STRUCTURE.md)       | Directory layout, entry points, and module boundaries          |
| [`ARCHITECTURE.md`](../docs/codebase/ARCHITECTURE.md) | Architectural style, request lifecycle, and design patterns    |
| [`CONVENTIONS.md`](../docs/codebase/CONVENTIONS.md)   | Naming, formatting, error handling, and coding standards       |
| [`INTEGRATIONS.md`](../docs/codebase/INTEGRATIONS.md) | External services, database, authentication, and observability |
| [`TESTING.md`](../docs/codebase/TESTING.md)           | Test stack, layout, scope matrix, and isolation strategy       |
| [`CONCERNS.md`](../docs/codebase/CONCERNS.md)         | Known risks, technical debt, and security concerns             |
