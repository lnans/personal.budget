#!/bin/bash
set -e

dotnet tool restore
dotnet format . --exclude ./src/Persistence/Migrations -v d
dotnet csharpier format .
