using System.Net.Http.Json;
using System.Threading.Tasks;
using Application.Commands.Auth;
using Domain.Common;
using NFluent;
using NUnit.Framework;

namespace Api.IntegrationTests.Commands.Auth;

[TestFixture]
public class SignInTests : TestBase
{
    [Test]
    public async Task SignIn_WithGoodCred_ShouldReturn_Token()
    {
        // Arrange
        var command = new SignInCommand("string", "string");
        
        // Act
        var response = await HttpClient.PostAsJsonAsync("/auth/signin", command);
        var result = await response.Content.ReadFromJsonAsync<SignInResponse>();
        
        // Assert
        Check.That(result).IsNotNull();
        Check.That(result?.Username).IsEqualTo("string");
        Check.That(result?.Token).IsNotEmpty();
    }
    
    [Test]
    public async Task SignIn_WithWrongUserName_ShouldReturn_Error()
    {
        // Arrange
        var command = new SignInCommand("none", "string");
        
        // Act
        var response = await HttpClient.PostAsJsonAsync("/auth/signin", command);
        var result = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        
        // Assert
        Check.That(result).IsNotNull();
        Check.That(result?.Message).IsEqualTo("errors.auth_failed");
    }
    
    [Test]
    public async Task SignIn_WithWrongPassword_ShouldReturn_Error()
    {
        // Arrange
        var command = new SignInCommand("string", "none");
        
        // Act
        var response = await HttpClient.PostAsJsonAsync("/auth/signin", command);
        var result = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        
        // Assert
        Check.That(result).IsNotNull();
        Check.That(result?.Message).IsEqualTo("errors.auth_failed");
    }
}