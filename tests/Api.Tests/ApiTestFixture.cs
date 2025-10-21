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
    private readonly string _cachedPasswordHash;

    public HttpClient ApiClient { get; }
    public IServiceProvider ScopedServiceProvider => _webApplicationFactory.Services.CreateScope().ServiceProvider;
    public User User { get; private set; } = null!;
    public string UserPassword { get; } = DefaultUserPassword;
    public string UserToken { get; private set; } = null!;
    public string UserRefreshToken { get; private set; } = null!;

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

        // Hash password once and cache it for all tests
        var passwordHasher = ScopedServiceProvider.GetRequiredService<IPasswordHasher>();
        _cachedPasswordHash = passwordHasher.Hash(DefaultUserPassword);

        using var dbContext = ScopedServiceProvider.GetRequiredService<AppDbContext>();
        dbContext.Database.Migrate();
        InitUser(dbContext);

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

    public async Task ResetDatabaseAsync()
    {
        await _respawner.ResetAsync(_dbConnection);
        await using var dbContext = ScopedServiceProvider.GetRequiredService<AppDbContext>();
        InitUser(dbContext);
    }

    public async Task DisposeAsync()
    {
        await _dbConnection.CloseAsync();
        await _dbTestContainer.StopAsync();
        await _dbTestContainer.DisposeAsync();

        ApiClient.Dispose();
        await _webApplicationFactory.DisposeAsync();
    }

    private void InitUser(AppDbContext dbContext)
    {
        var authTokenGenerator = ScopedServiceProvider.GetRequiredService<IAuthTokenGenerator>();
        User = User.Create(DefaultUserLogin, _cachedPasswordHash, DateTimeOffset.UtcNow).Value;
        UserToken = User.GenerateAuthToken(authTokenGenerator);
        UserRefreshToken = User.GenerateRefreshToken(authTokenGenerator);
        dbContext.Users.Add(User);
        dbContext.SaveChanges();
    }
}
