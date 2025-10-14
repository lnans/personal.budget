dotnet tool restore
dotnet test -- --coverage --coverage-output-format cobertura --coverage-output coverage.cobertura.xml

if [ $? -eq 0 ]; then
    dotnet ReportGenerator -reports:tests/**/coverage.cobertura.xml -targetdir:.coverage
    open .coverage/index.html
fi
