<p align="center">
    <img src="./doc/logo.png" />
</p>

# Budget. Web

**Budget.** is a personal finance management web application designed to organize and track spending and savings over time. This repository contains the frontend implementation built with modern React technologies, following **Clean Architecture** principles.

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

This project uses Docker Compose for containerized deployment:

```bash
# Build and start all services
docker-compose up --build

# Run in detached mode
docker-compose up -d --build

# View logs
docker-compose logs -f web

# Stop services
docker-compose down

# Stop and remove volumes
docker-compose down -v
```

- Web Application: http://localhost:8081

## Running with Backend

The web application requires the backend API to be running. To run the full application:

1. **Start the backend API**: Follow the instructions in the [backend repository](https://github.com/lnans/personal.budget)

2. **Configure the API URL**: Create a `.env` file in the root of the project:

   ```bash
   VITE_API_URL=http://localhost:8080
   ```

3. **Start the web application**: Run the development server as described in the [Development](#development) section.

## Environment Variables

| Variable       | Description                                      | Required |
| -------------- | ------------------------------------------------ | -------- |
| `VITE_API_URL` | Backend API URL (must be a valid HTTP/HTTPS URL) | Yes      |

## License

This project is licensed under the MIT License - see the [LICENSE](https://github.com/lnans/personal.budget/blob/main/LICENSE) file for details.
