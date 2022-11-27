namespace Api.IntegrationTests;

public abstract class TestBase : IAsyncLifetime
{
    private readonly Func<Task> _resetDatabase;

    protected readonly HttpClient Api;
    protected readonly Func<IApplicationDbContext> DbContext;

    protected TestBase(ApiFactory factory)
    {
        _resetDatabase = factory.ResetDatabaseAsync;
        Api = factory.ApiClient;
        DbContext = factory.DbContext;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _resetDatabase();
}