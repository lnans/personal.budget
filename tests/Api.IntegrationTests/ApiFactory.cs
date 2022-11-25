using System.Data.Common;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using Respawn;
using Xunit;

namespace Api.IntegrationTests;

public class ApiFactory : WebApplicationFactory<IApiMaker>, IAsyncLifetime
{
    private readonly PostgreSqlTestcontainer _dbContainer =
        new TestcontainersBuilder<PostgreSqlTestcontainer>()
            .WithDatabase(new PostgreSqlTestcontainerConfiguration
            {
                Database = "testdb",
                Username = "user",
                Password = "passwd"
            }).Build();

    private DbConnection _dbConnection = default!;
    private Respawner _respawner = default!;

    public HttpClient ApiClient { get; private set; } = default!;

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        _dbConnection = new NpgsqlConnection(_dbContainer.ConnectionString);
        ApiClient = CreateClient();
        ApiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", FakeJwtManager.GenerateJwtToken());
        await _dbConnection.OpenAsync();
        _respawner = await Respawner.CreateAsync(_dbConnection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = new[] { "public" }
        });
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.StopAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptionsBuilder<ApplicationDbContext>));
            services.AddScoped(sp => new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseNpgsql(_dbContainer.ConnectionString)
                .UseApplicationServiceProvider(sp)
                .Options);

            services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    IssuerSigningKey = FakeJwtManager.SecurityKey,
                    ValidIssuer = FakeJwtManager.Issuer,
                    ValidAudience = FakeJwtManager.Audience
                };
            });
        });
    }

    public async Task ResetDatabaseAsync()
    {
        await _respawner.ResetAsync(_dbConnection);
    }
}