using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Application.Features.Authentication.Commands.SignIn;
using Domain.Common;
using NFluent;
using NUnit.Framework;

namespace Api.IntegrationTests.Features.Authentication.Commands;

[TestFixture]
public class SignInTests : TestBase
{
    [Test]
    public async Task SignIn_WithGoodCred_ShouldReturn_Token()
    {
        // Arrange
        var command = new SignInRequest
        {
            Username = "string", Password = "string"
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync("/auth/signin", command);
        var result = await response.Content.ReadFromJsonOrDefaultAsync<AuthenticationDto>();

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
        var command = new SignInRequest
        {
            Username = "none", Password = "string"
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync("/auth/signin", command);
        var result = await response.Content.ReadFromJsonOrDefaultAsync<ErrorResponse>();

        // Assert
        Check.That(response.StatusCode).IsEqualTo(HttpStatusCode.Forbidden);
        Check.That(result).IsNotNull();
        Check.That(result?.Message).IsEqualTo("errors.auth.failed");
    }

    [Test]
    public async Task SignIn_WithWrongPassword_ShouldReturn_Error()
    {
        // Arrange
        var command = new SignInRequest
        {
            Username = "string", Password = "none"
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync("/auth/signin", command);
        var result = await response.Content.ReadFromJsonOrDefaultAsync<ErrorResponse>();

        // Assert
        Check.That(response.StatusCode).IsEqualTo(HttpStatusCode.Forbidden);
        Check.That(result).IsNotNull();
        Check.That(result?.Message).IsEqualTo("errors.auth.failed");
    }
}