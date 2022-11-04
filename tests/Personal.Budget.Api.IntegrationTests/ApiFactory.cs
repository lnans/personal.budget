using System.Data.Common;
using System.Net.Http;
using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using Personal.Budget.Api.Persistence;
using Respawn;
using Xunit;

namespace Personal.Budget.Api.IntegrationTests;

public class ApiFactory : WebApplicationFactory<IApiMaker>, IAsyncLifetime
{
    private DbConnection _dbConnection = default!;
    private Respawner _respawner = default!;

    private readonly PostgreSqlTestcontainer _dbContainer =
        new TestcontainersBuilder<PostgreSqlTestcontainer>()
            .WithDatabase(new PostgreSqlTestcontainerConfiguration()
            {
                Database = "testdb",
                Username = "user",
                Password = "passwd"
            }).Build();

    public HttpClient HttpClient { get; private set; } = default!;
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptionsBuilder<ApiDbContext>));
            services.AddScoped(sp => new DbContextOptionsBuilder<ApiDbContext>()
                .UseNpgsql(_dbContainer.ConnectionString)
                .UseApplicationServiceProvider(sp)
                .Options);
        });
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        _dbConnection = new NpgsqlConnection(_dbContainer.ConnectionString);
        HttpClient = CreateClient();
        await _dbConnection.OpenAsync();
        _respawner = await Respawner.CreateAsync(_dbConnection, new RespawnerOptions()
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = new[] { "public" }
        });
    }

    public async Task ResetDatabaseAsync()
    {
        await _respawner.ResetAsync(_dbConnection);
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.StopAsync();
    }
}