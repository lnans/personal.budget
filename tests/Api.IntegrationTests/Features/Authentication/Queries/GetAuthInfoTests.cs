using System.Net;
using System.Threading.Tasks;
using Application.Features.Authentication.Queries.GetAuthInfo;
using NFluent;
using NUnit.Framework;

namespace Api.IntegrationTests.Features.Authentication.Queries;

[TestFixture]
public class GetAuthInfoTests : TestBase
{
    [Test]
    public async Task GetInfo_ShouldReturn_AuthInfo()
    {
        // Act
        var response = await HttpClient.GetAsync("auth/me");
        var result = await response.Content.ReadFromJsonOrDefaultAsync<AuthenticationInfoDto>();

        // Asset
        Check.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        Check.That(result).IsNotNull();
        Check.That(result?.Id).IsEqualTo(DefaultUser.Id);
        Check.That(result?.Username).IsEqualTo(DefaultUser.Username);
    }
}