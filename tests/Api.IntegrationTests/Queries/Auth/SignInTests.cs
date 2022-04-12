using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Application.Queries.Auth;
using Domain.Common;
using NFluent;
using NUnit.Framework;

namespace Api.IntegrationTests.Queries.Auth;

[TestFixture]
public class SignInTests : TestBase
{
    [Test]
    public async Task SignIn_WithGoodCred_ShouldReturn_Token()
    {
        // Arrange
        var command = new SignInRequest("string", "string");

        // Act
        var response = await HttpClient.PostAsJsonAsync("/auth/signin", command);
        var result = await response.Content.ReadFromJsonOrDefaultAsync<SignInResponse>();

        // Assert
        Check.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        Check.That(result).IsNotNull();
        Check.That(result?.Username).IsEqualTo("string");
        Check.That(result?.Token).IsNotEmpty();
    }

    [Test]
    public async Task SignIn_WithWrongUserName_ShouldReturn_Error()
    {
        // Arrange
        var command = new SignInRequest("none", "string");

        // Act
        var response = await HttpClient.PostAsJsonAsync("/auth/signin", command);
        var result = await response.Content.ReadFromJsonOrDefaultAsync<ErrorResponse>();

        // Assert
        Check.That(response.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
        Check.That(result).IsNotNull();
        Check.That(result?.Message).IsEqualTo("errors.auth_failed");
    }

    [Test]
    public async Task SignIn_WithWrongPassword_ShouldReturn_Error()
    {
        // Arrange
        var command = new SignInRequest("string", "none");

        // Act
        var response = await HttpClient.PostAsJsonAsync("/auth/signin", command);
        var result = await response.Content.ReadFromJsonOrDefaultAsync<ErrorResponse>();

        // Assert
        Check.That(response.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
        Check.That(result).IsNotNull();
        Check.That(result?.Message).IsEqualTo("errors.auth_failed");
    }
}