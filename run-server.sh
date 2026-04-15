#!/usr/bin/env bash
set -euo pipefail

# Loads env files into the shell so Compose and builds see the same values.
# Precedence (later files override earlier): repo `.env` → `backend/.env.production` → frontend prod, else frontend dev `.env`.

root="$(cd "$(dirname "$0")" && pwd)"
cd "$root"

set -a
if [ -f .env ]
then
  # shellcheck disable=SC1091
  . ./.env
fi

if [ -f backend/.env.production ]
then
  # shellcheck disable=SC1091
  . ./backend/.env.production
fi

if [ -f frontend/.env.production ]
then
  # shellcheck disable=SC1091
  . ./frontend/.env.production
elif [ -f frontend/.env ]
then
  # shellcheck disable=SC1091
  . ./frontend/.env
fi
set +a

if docker compose version >/dev/null 2>&1
then
  docker compose up -d --build
elif command -v docker-compose >/dev/null 2>&1
then
  docker-compose up -d --build
else
  echo "Docker Compose is required (docker compose or docker-compose)." >&2
  exit 1
fi
