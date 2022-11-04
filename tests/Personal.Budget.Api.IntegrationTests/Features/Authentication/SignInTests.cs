using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Personal.Budget.Api.Features.Authentication.SignIn;
using Xunit;

namespace Personal.Budget.Api.IntegrationTests.Features.Authentication;

[Collection("Shared")]
public class SignInTests : IAsyncLifetime
{
    private readonly HttpClient _api;
    private readonly Func<Task> _resetDatabase;

    public SignInTests(ApiFactory factory)
    {
        _api = factory.HttpClient;
        _resetDatabase = factory.ResetDatabaseAsync;
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _resetDatabase();

    [Fact]
    public async Task SignIn_WithValidCreds_ShouldReturn_200()
    {
        // Arrange
        var request = new SignInRequest { Password = "string", Username = "string" };

        // Act
        var response = await _api.PostAsJsonAsync("auth", request);
        var responseText = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<SignInResponse>(responseText);

        // Assert
        response.StatusCode
            .Should().Be(HttpStatusCode.OK);
        result
            .Should().NotBeNull();
        result!.Token
            .Should().NotBeEmpty();
    }

    [Fact]
    public async Task SignIn_WithNotValidCreds_ShouldReturn_400()
    {
        // Arrange
        var request = new SignInRequest { Password = "wrong", Username = "wrong" };

        // Act
        var response = await _api.PostAsJsonAsync("auth", request);

        // Assert
        response.StatusCode
            .Should().Be(HttpStatusCode.Unauthorized);
    }
}