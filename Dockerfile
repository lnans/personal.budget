FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env
WORKDIR /app
COPY . ./
RUN dotnet restore
RUN dotnet publish ./src/Api/Api.csproj -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY --from=build-env /app/out .
ENV ASPNETCORE_URLS="http://[::]:5038;https://[::]:5039"
EXPOSE 5038
EXPOSE 5039
ENTRYPOINT ["dotnet", "Api.dll"]