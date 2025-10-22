using Application.Interfaces;
using Domain.Users;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Tests;

public class ApiTestBase(ApiTestFixture fixture) : IAsyncLifetime
{
    private IServiceScope? _scope;
    private IAppDbContext? _dbContext;

    protected CancellationToken CancellationToken => TestContext.Current.CancellationToken;
    protected HttpClient ApiClient => fixture.ApiClient;
    protected User User => fixture.User;
    protected string UserToken => fixture.UserToken;
    protected string UserPassword => fixture.UserPassword;
    protected string UserRefreshToken => fixture.UserRefreshToken;

    /// <summary>
    /// Gets the DbContext for the current test. This context is scoped to the test lifetime.
    /// </summary>
    protected IAppDbContext DbContext => _dbContext ??= _scope!.ServiceProvider.GetRequiredService<IAppDbContext>();

    /// <summary>
    /// Creates a fresh scope with a new DbContext. Useful when you need to query the database
    /// after operations that were performed through the API (which uses a different context).
    /// </summary>
    protected IServiceScope CreateFreshScope() => fixture.Services.CreateScope();

    public async ValueTask DisposeAsync()
    {
        _scope?.Dispose();
        await fixture.ResetFixtureStateAsync();
    }

    public ValueTask InitializeAsync()
    {
        _scope = fixture.Services.CreateScope();
        return ValueTask.CompletedTask;
    }
}
