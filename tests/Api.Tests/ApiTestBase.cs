using Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Tests;

public class ApiTestBase(ApiTestFixture fixture) : IAsyncLifetime
{
    protected HttpClient ApiClient => fixture.ApiClient;

    protected IAppDbContext DbContext { get; set; } =
        fixture.ScopedServiceProvider.GetRequiredService<IAppDbContext>();

    public async ValueTask DisposeAsync() => await fixture.ResetDatabaseAsync();

    public ValueTask InitializeAsync() => ValueTask.CompletedTask;
}
