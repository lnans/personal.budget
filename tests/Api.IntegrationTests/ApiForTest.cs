using Application.Common.Interfaces;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Api.IntegrationTests;

internal class ApiForTest : WebApplicationFactory<Program>
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        var root = new InMemoryDatabaseRoot();

        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<ApplicationDbDbContext>));
            services.AddDbContext<IApplicationDbContext, ApplicationDbDbContext>(options =>
                options.UseInMemoryDatabase("Testing", root));
        });

        return base.CreateHost(builder);
    }
}