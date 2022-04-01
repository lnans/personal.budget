using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using Application;
using Domain.Common;
using Domain.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Api.IntegrationTests;

public abstract class TestBase
{
    protected HttpClient HttpClient = null!;
    protected IApplicationDbContext DbContext = null!;
    protected User DefaultUser = null!;
    
    [OneTimeSetUp]
    protected void Setup()
    {
        var api = new ApiForTest();

        var scope = api.Services.CreateScope();
        DbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        DefaultUser = DbContext.Users.First();

        var jwtSettings = scope.ServiceProvider.GetRequiredService<JwtSettings>();
        var authToken = Utils.CreateJwtToken(jwtSettings, DefaultUser);
        var authHeader = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, authToken);
        HttpClient = api.CreateClient();
        HttpClient.DefaultRequestHeaders.Authorization = authHeader;
    }
}