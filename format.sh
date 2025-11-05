#!/bin/bash
set -e

dotnet tool restore
dotnet format . --exclude ./src/Infrastructure/Persistence/Migrations -v d
dotnet csharpier format .