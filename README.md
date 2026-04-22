<p align="center">
    <img src="budget.png" alt="Budget." />
</p>

# Budget.

## Table of contents

- [About](#about)
- [Repository layout](#repository-layout)
- [Run with Docker](#run-with-docker)
  - [Local development](#local-development)
  - [Production on a server](#production-on-a-server)
- [Documentation](#documentation)
- [License](#license)

## About

**Budget.** is a personal finance application for organizing and tracking spending and savings over time. It pairs a **REST API** (accounts, operations, tags, authentication) with a **web UI** so you can manage your budget from the browser.

This repository is a **monorepo**: the API lives under [`backend/`](backend/), the SPA under [`frontend/`](frontend/), and **Docker Compose** at the root runs the database, API, and static frontend together.

## Repository layout

| Path                                       | Role                                                        |
| ------------------------------------------ | ----------------------------------------------------------- |
| [`backend/`](backend/)                     | .NET 10 API, EF Core, PostgreSQL                            |
| [`frontend/`](frontend/)                   | React + Vite SPA                                            |
| [`docker-compose.yml`](docker-compose.yml) | `budget-db`, `budget-api`, `budget-web`                     |
| [`run-server.sh`](run-server.sh)           | Optional env overrides, then `docker compose up -d --build` |

## Run with Docker

Prerequisites: [Docker](https://www.docker.com/) with **Docker Compose v2** (or `docker-compose`). All commands below assume your **current directory is the repository root**.

### Local development

You do **not** need to create or edit any env files for a normal local run: [`docker-compose.yml`](docker-compose.yml) supplies defaults for the API and database, and the frontend image is built with the committed [`frontend/.env`](frontend/.env) (Vite variables for the Docker build).

1. **Start the stack** from the repo root:

   ```bash
   ./run-server.sh
   ```

   Or: `docker compose up -d --build`

2. **Endpoints** (default compose file binds ports to **127.0.0.1** only on the host):
   - API: http://127.0.0.1:8080 (docs: http://127.0.0.1:8080/docs)
   - Web: http://127.0.0.1:8081

3. **Logs and stop:**

   ```bash
   docker compose logs -f budget-api
   docker compose down
   ```

Optional: use a root **`.env`**, **`backend/.env.production`**, or **`frontend/.env.production`** to override defaults—[`run-server.sh`](run-server.sh) loads them when present (see script comments). Use **strong** secrets and non-default app credentials when the stack is reachable beyond your own machine (see [Production on a server](#production-on-a-server)).

### Production on a server

On a VPS or dedicated host you still run the same Compose project, but you should treat secrets, URLs, and exposure differently.

1. **Secrets:** set `JWT_SECRET_KEY`, database passwords, and seed credentials via **root `.env`** and/or **`backend/.env.production`** (never commit these files). The repo root [`.gitignore`](.gitignore) ignores `.env`.

2. **`VITE_API_URL`:** the web app is built with Vite, so API base URL is fixed at **image build time**. In `frontend/.env.production`, set `VITE_API_URL` to the **public origin** clients will use (for example `https://api.yourdomain.com`). Rebuild the `budget-web` image after changing it (`docker compose build budget-web` or `./run-server.sh`).

3. **Exposing services:** the default [`docker-compose.yml`](docker-compose.yml) publishes ports on **127.0.0.1** only. That is appropriate when a **reverse proxy** on the same host (Caddy, nginx, Traefik) terminates TLS and forwards to `127.0.0.1:8080` / `127.0.0.1:8081`. If you must publish directly to the internet, change the port mappings (and tighten firewall rules) deliberately rather than relying on defaults.

4. **HTTPS and domain:** use your proxy or platform (Kubernetes Ingress, PaaS, etc.) for TLS certificates and hostnames; keep the API and SPA URLs consistent with what you put in `VITE_API_URL` and any CORS settings.

5. **Operations:** use `docker compose up -d --build` for deployments, `docker compose pull` / rebuild when base images change, and back up the **`postgres-data`** volume for the database.

## Documentation

- **[Backend (API)](backend/README.md)** — stack, architecture, local `dotnet` workflow, tests, formatting
- **[Frontend (Web)](frontend/README.md)** — stack, architecture, Yarn scripts, Docker notes

### Codebase documentation

Detailed documentation lives under [`docs/codebase/`](docs/codebase/):

| Document                                           | Description                                                    |
| -------------------------------------------------- | -------------------------------------------------------------- |
| [`STACK.md`](docs/codebase/STACK.md)               | Languages, runtimes, frameworks, and all key dependencies      |
| [`STRUCTURE.md`](docs/codebase/STRUCTURE.md)       | Directory layout, entry points, and module boundaries          |
| [`ARCHITECTURE.md`](docs/codebase/ARCHITECTURE.md) | Architectural style, request lifecycle, and design patterns    |
| [`CONVENTIONS.md`](docs/codebase/CONVENTIONS.md)   | Naming, formatting, error handling, and coding standards       |
| [`INTEGRATIONS.md`](docs/codebase/INTEGRATIONS.md) | External services, database, authentication, and observability |
| [`TESTING.md`](docs/codebase/TESTING.md)           | Test stack, layout, scope matrix, and isolation strategy       |
| [`CONCERNS.md`](docs/codebase/CONCERNS.md)         | Known risks, technical debt, and security concerns             |

## License

This project is licensed under the MIT License — see the [LICENSE](backend/LICENSE) file.
