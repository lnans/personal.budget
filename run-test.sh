#!/bin/bash
set -e

dotnet tool restore
COVERAGE_HTML=false
CI_MODE=false
for arg in "$@"; do
  if [ "$arg" = "--coverage" ]; then
    COVERAGE_HTML=true
  fi
  if [ "$arg" = "--ci" ]; then
    CI_MODE=true
  fi
done
COVERAGE_ARGS="--coverage \
  --coverage-output coverage.cobertura.xml \
  --coverage-settings ./tests/settings.coverage.xml \
  --method-display method \
  --method-display-options replaceUnderscoreWithSpace \
  --no-progress \
  --output detailed"

dotnet run --project tests/Domain.Tests/Domain.Tests.csproj -- $COVERAGE_ARGS
dotnet run --project tests/Api.Tests/Api.Tests.csproj -- $COVERAGE_ARGS

if [ $? -eq 0 ] && [ "$COVERAGE_HTML" = true ]; then
    dotnet ReportGenerator -reports:tests/**/coverage.cobertura.xml -targetdir:.coverage
    if [ "$CI_MODE" = false ]; then
        open .coverage/index.html
    fi
fi
