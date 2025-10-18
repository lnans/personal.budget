using System.Data.Common;
using Api.Tests;
using Application.Interfaces;
using DotNet.Testcontainers.Builders;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
    private readonly WebApplicationFactory<IApiAssemblyMarker> _webApplicationFactory;

    public ApiTestFixture()
    {
        _dbTestContainer = new PostgreSqlBuilder()
            .WithDatabase(DbName)
            .WithUsername(DbUser)
            .WithPassword(DbPassword)
            .WithPortBinding(DbPort, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilExternalTcpPortIsAvailable(DbPort))
            .Build();

        _dbTestContainer.StartAsync().Wait();
        var dbConnectionString = _dbTestContainer.GetConnectionString();

        _webApplicationFactory = new WebApplicationFactory<IApiAssemblyMarker>().WithWebHostBuilder(builder =>
            builder.ConfigureServices(services =>
            {
                services.RemoveAll(typeof(DbContextOptions<AppDbContext>));
                services.AddDbContext<IAppDbContext, AppDbContext>(options => options.UseNpgsql(dbConnectionString));
            })
        );

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
        await _dbTestContainer.StopAsync();
        await _dbTestContainer.DisposeAsync();

        ApiClient.Dispose();
        await _webApplicationFactory.DisposeAsync();
    }
}
