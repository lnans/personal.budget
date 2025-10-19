using System.Data.Common;
using Api.Tests;
using DotNet.Testcontainers.Builders;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;

[assembly: AssemblyFixture(typeof(ApiTestFixture))]

namespace Api.Tests;

public class ApiTestFixture
{
    private const string DbName = "budget-sqldb-tests";
    private const string DbUser = "postgres";
    private const string DbPassword = "$trongP4ssword";
    private const int DbPort = 5432;

    private readonly PostgreSqlContainer _dbTestContainer;
    private readonly DbConnection _dbConnection;
    private readonly Respawner _respawner;
    private readonly ApiFactory _webApplicationFactory;

    public ApiTestFixture()
    {
        _dbTestContainer = new PostgreSqlBuilder()
            .WithDatabase(DbName)
            .WithUsername(DbUser)
            .WithPassword(DbPassword)
            .WithPortBinding(DbPort, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilExternalTcpPortIsAvailable(DbPort))
            .WithLogger(new NullLogger<PostgreSqlContainer>()) // Remove this line to see container logs
            .Build();

        _dbTestContainer.StartAsync().Wait();
        var dbConnectionString = _dbTestContainer.GetConnectionString();

        _webApplicationFactory = new ApiFactory(dbConnectionString);

        using var dbContext = ScopedServiceProvider.GetRequiredService<AppDbContext>();
        dbContext.Database.Migrate();

        ApiClient = _webApplicationFactory.CreateClient();

        _dbConnection = new NpgsqlConnection(dbConnectionString);
        _dbConnection.Open();
        _respawner = Respawner
            .CreateAsync(
                _dbConnection,
                new RespawnerOptions
                {
                    DbAdapter = DbAdapter.Postgres,
                    TablesToIgnore = ["__EFMigrationsHistory"],
                    SchemasToInclude = ["public"],
                }
            )
            .GetAwaiter()
            .GetResult();
    }

    public HttpClient ApiClient { get; }

    public IServiceProvider ScopedServiceProvider => _webApplicationFactory.Services.CreateScope().ServiceProvider;

    public async Task ResetDatabaseAsync() => await _respawner.ResetAsync(_dbConnection);

    public async Task DisposeAsync()
    {
        await _dbConnection.CloseAsync();
        await _dbTestContainer.StopAsync();
        await _dbTestContainer.DisposeAsync();

        ApiClient.Dispose();
        await _webApplicationFactory.DisposeAsync();
    }
}
