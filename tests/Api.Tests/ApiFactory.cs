using Application.Interfaces;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Api.Tests;

public class ApiFactory(string dbConnectionString) : WebApplicationFactory<IApiAssemblyMarker>
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.UseSerilog((_, _) => { }); // remove logger during test runs
        return base.CreateHost(builder);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder) =>
        builder
            .ConfigureServices(services =>
            {
                services.RemoveAll(typeof(DbContextOptions<AppDbContext>));
                services.AddDbContext<IAppDbContext, AppDbContext>(options => options.UseNpgsql(dbConnectionString));
            })
            .ConfigureLogging(logging => logging.ClearProviders());
}
