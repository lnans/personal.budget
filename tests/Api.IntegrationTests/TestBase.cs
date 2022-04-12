using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Application.Common.Helpers;
using Application.Common.Interfaces;
using Domain.Common;
using Domain.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Api.IntegrationTests;

public abstract class TestBase
{
    private ApiForTest _api = null!;
    protected User DefaultUser = null!;
    protected HttpClient HttpClient = null!;

    [OneTimeSetUp]
    protected void OneTimeSetup()
    {
        _api = new ApiForTest();

        var scope = _api.Services.CreateScope();
        DefaultUser = GetDbContext().Users.First();

        var jwtSettings = scope.ServiceProvider.GetRequiredService<JwtSettings>();
        var authToken = JwtHelper.CreateJwtToken(jwtSettings, DefaultUser);
        var authHeader = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, authToken);
        HttpClient = _api.CreateClient();
        HttpClient.DefaultRequestHeaders.Authorization = authHeader;
    }

    [SetUp]
    protected async Task Setup()
    {
        var dbContext = GetDbContext();
        dbContext.OperationTags.RemoveRange(dbContext.OperationTags);
        dbContext.Operations.RemoveRange(dbContext.Operations);
        dbContext.Accounts.RemoveRange(dbContext.Accounts);
        await dbContext.SaveChangesAsync();
    }

    protected IApplicationDbContext GetDbContext()
    {
        var scope = _api.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
    }
}