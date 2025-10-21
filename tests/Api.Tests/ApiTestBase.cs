using Application.Interfaces;
using Domain.Users;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Tests;

public class ApiTestBase(ApiTestFixture fixture) : IAsyncLifetime
{
    protected CancellationToken CancellationToken => TestContext.Current.CancellationToken;
    protected HttpClient ApiClient => fixture.ApiClient;
    protected IAppDbContext DbContext { get; set; } = fixture.ScopedServiceProvider.GetRequiredService<IAppDbContext>();
    protected User User => fixture.User;
    protected string UserToken => fixture.UserToken;
    protected string UserPassword => fixture.UserPassword;
    protected string UserRefreshToken => fixture.UserRefreshToken;

    public async ValueTask DisposeAsync() => await fixture.ResetDatabaseAsync();

    public ValueTask InitializeAsync() => ValueTask.CompletedTask;
}
