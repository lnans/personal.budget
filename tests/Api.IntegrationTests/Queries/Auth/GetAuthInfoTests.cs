using System.Net;
using System.Threading.Tasks;
using Application.Queries.Auth;
using NFluent;
using NUnit.Framework;

namespace Api.IntegrationTests.Queries.Auth;

[TestFixture]
public class GetAuthInfoTests : TestBase
{
    [Test]
    public async Task GetInfo_ShouldReturn_AuthInfo()
    {
        // Act
        var response = await HttpClient.GetAsync("auth");
        var result = await response.Content.ReadFromJsonOrDefaultAsync<GetAuthInfoResponse>();

        // Asset
        Check.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        Check.That(result).IsNotNull();
        Check.That(result?.Id).IsEqualTo(DefaultUser.Id);
        Check.That(result?.Username).IsEqualTo(DefaultUser.Username);
    }
}