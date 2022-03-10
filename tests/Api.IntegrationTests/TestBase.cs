using System.Net.Http;
using Application;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Api.IntegrationTests;

public abstract class TestBase
{
    protected HttpClient HttpClient = null!;
    protected IApplicationDbContext DbContext = null!;
    
    [OneTimeSetUp]
    protected void Setup()
    {
        var api = new ApiForTest();
        HttpClient = api.CreateClient();

        var scope = api.Services.CreateScope();
        DbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
    }
}