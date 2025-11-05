using System.Data.Common;
using Domain.Users;
using DotNet.Testcontainers.Builders;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;

namespace Api.Tests;

public class ApiTestFixture : IAsyncLifetime
{
    private const string DbName = "budget-sqldb-tests";
    private const string DbUser = "postgres";
    private const string DbPassword = "$trongP4ssword";
    private const int DbPort = 5432;

    private const string DefaultUserLogin = "testuser";
    private const string DefaultUserPassword = "TestPassword123!";

    private PostgreSqlContainer _dbTestContainer = null!;
    private DbConnection _dbConnection = null!;
    private Respawner _respawner = null!;
    private ApiFactory _webApplicationFactory = null!;
    private string _cachedPasswordHash = null!;

    public HttpClient ApiClient { get; private set; } = null!;
    public IServiceProvider Services => _webApplicationFactory.Services;
    public User User { get; private set; } = null!;
    public string UserPassword { get; } = DefaultUserPassword;
    public string UserToken { get; private set; } = null!;
    public string UserRefreshToken { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
        _dbTestContainer = new PostgreSqlBuilder()
            .WithDatabase(DbName)
            .WithUsername(DbUser)
            .WithPassword(DbPassword)
            .WithPortBinding(DbPort, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilExternalTcpPortIsAvailable(DbPort))
            .WithLogger(new NullLogger<PostgreSqlContainer>()) // Remove this line to see container logs
            .Build();

        await _dbTestContainer.StartAsync();
        var dbConnectionString = _dbTestContainer.GetConnectionString();

        _webApplicationFactory = new ApiFactory(dbConnectionString);
        using var scope = _webApplicationFactory.Services.CreateScope();

        // Hash password once and cache it for all tests
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        _cachedPasswordHash = passwordHasher.Hash(DefaultUserPassword);

        await using var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.MigrateAsync();
        await InitUserAsync(dbContext);

        ApiClient = _webApplicationFactory.CreateClient();

        _dbConnection = new NpgsqlConnection(dbConnectionString);
        await _dbConnection.OpenAsync();
        _respawner = await Respawner.CreateAsync(
            _dbConnection,
            new RespawnerOptions
            {
                DbAdapter = DbAdapter.Postgres,
                TablesToIgnore = ["__EFMigrationsHistory"],
                SchemasToInclude = ["public"],
            }
        );
    }

    public async Task ResetFixtureStateAsync()
    {
        ApiClient.DefaultRequestHeaders.Authorization = null;
        await _respawner.ResetAsync(_dbConnection);
        using var scope = _webApplicationFactory.Services.CreateScope();
        await using var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await InitUserAsync(dbContext);
    }

    public async ValueTask DisposeAsync()
    {
        await _dbConnection.CloseAsync();
        await _dbTestContainer.StopAsync();
        await _dbTestContainer.DisposeAsync();

        ApiClient.Dispose();
        await _webApplicationFactory.DisposeAsync();
    }

    private async Task InitUserAsync(AppDbContext dbContext)
    {
        var authTokenGenerator = Services.GetRequiredService<IAuthTokenGenerator>();
        User = User.Create(DefaultUserLogin, _cachedPasswordHash, DateTimeOffset.UtcNow).Value;
        UserToken = User.GenerateAuthToken(authTokenGenerator);
        UserRefreshToken = User.GenerateRefreshToken(authTokenGenerator);

        await dbContext.Users.AddAsync(User);
        await dbContext.SaveChangesAsync();
    }
}
