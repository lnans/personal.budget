using System.Data.Common;
using Api.Tests;
using Domain.Users;
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

    private const string DefaultUserLogin = "testuser";
    private const string DefaultUserPassword = "TestPassword123!";

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

        var authTokenGenerator = ScopedServiceProvider.GetRequiredService<IAuthTokenGenerator>();
        var passwordHasher = ScopedServiceProvider.GetRequiredService<IPasswordHasher>();
        var userPasswordHash = passwordHasher.Hash(DefaultUserPassword);
        User = User.Create(DefaultUserLogin, userPasswordHash, DateTimeOffset.UtcNow).Value;
        UserToken = User.GenerateAuthToken(authTokenGenerator);
        dbContext.Users.Add(User);
        dbContext.SaveChanges();

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

    public User User { get; }

    public string UserPassword { get; } = DefaultUserPassword;

    public string UserToken { get; }

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
