dotnet tool restore
dotnet run --project tests/Domain.Tests/Domain.Tests.csproj -- --coverage --coverage-output coverage.cobertura.xml --coverage-settings ./tests/settings.coverage.xml
dotnet run --project tests/Api.Tests/Api.Tests.csproj -- --coverage --coverage-output coverage.cobertura.xml --coverage-settings ./tests/settings.coverage.xml

if [ $? -eq 0 ]; then
    dotnet ReportGenerator -reports:tests/**/coverage.cobertura.xml -targetdir:.coverage
    open .coverage/index.html
fi
