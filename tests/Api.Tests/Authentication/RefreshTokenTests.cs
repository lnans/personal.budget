using System.Net.Http.Json;
using Application.Features.Authentication.Commands.RefreshToken;
using Application.Features.Authentication.Commands.SignIn;
using Domain.Users;
using Microsoft.AspNetCore.Http;

namespace Api.Tests.Authentication;

[Collection(ApiTestCollection.CollectionName)]
public class RefreshTokenTests : ApiTestBase
{
    private const string Endpoint = "/auth/refresh-token";
    private const string SignInEndpoint = "/auth/sign-in";

    public RefreshTokenTests(ApiTestFixture factory)
        : base(factory) { }

    [Fact]
    public async Task RefreshToken_ReturnsNewTokens_WhenRefreshTokenIsValid()
    {
        // Arrange
        var command = new RefreshTokenCommand { RefreshToken = UserRefreshToken };

        // Act
        var response = await ApiClient.PostAsJsonAsync(Endpoint, command, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<RefreshTokenResponse>(CancellationToken);

        // Assert
        result.ShouldBeSuccessful();
        result.Response.ShouldNotBeNull();
        result.Response.Bearer.ShouldNotBeNullOrWhiteSpace();
        result.Response.RefreshToken.ShouldNotBeNullOrWhiteSpace();

        result.Response.Bearer.ShouldNotBe(UserToken);
        result.Response.RefreshToken.ShouldNotBe(UserRefreshToken);
    }

    [Fact]
    public async Task RefreshToken_ReturnsUnauthorized_WhenRefreshTokenIsInvalid()
    {
        // Arrange
        var command = new RefreshTokenCommand { RefreshToken = "invalid-token" };

        // Act
        var response = await ApiClient.PostAsJsonAsync(Endpoint, command, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<RefreshTokenResponse>(CancellationToken);

        // Assert
        result.ShouldBeProblem();
        result.Problem.ShouldNotBeNull();
        result.Problem.Status.ShouldBe(StatusCodes.Status401Unauthorized);
    }

    [Fact]
    public async Task RefreshToken_ReturnsUnauthorized_WhenRefreshTokenIsExpired()
    {
        // Arrange
        var command = new RefreshTokenCommand
        {
            RefreshToken =
                "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJleHAiOjE2MDAwMDAwMDAsInN1YiI6IjEyMzQ1Njc4OTAifQ.invalid",
        };

        // Act
        var response = await ApiClient.PostAsJsonAsync(Endpoint, command, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<RefreshTokenResponse>(CancellationToken);

        // Assert
        result.ShouldBeProblem();
        result.Problem.ShouldNotBeNull();
        result.Problem.Status.ShouldBe(StatusCodes.Status401Unauthorized);
    }

    [Fact]
    public async Task RefreshToken_ReturnsUnauthorized_WhenAccessTokenIsUsedInsteadOfRefreshToken()
    {
        // Arrange
        var command = new RefreshTokenCommand { RefreshToken = UserToken };

        // Act
        var response = await ApiClient.PostAsJsonAsync(Endpoint, command, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<RefreshTokenResponse>(CancellationToken);

        // Assert
        result.ShouldBeProblem();
        result.Problem.ShouldNotBeNull();
        result.Problem.Status.ShouldBe(StatusCodes.Status401Unauthorized);
    }

    [Fact]
    public async Task RefreshToken_ReturnsBadRequest_WhenRefreshTokenIsEmpty()
    {
        // Arrange
        var command = new RefreshTokenCommand { RefreshToken = string.Empty };

        // Act
        var response = await ApiClient.PostAsJsonAsync(Endpoint, command, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<RefreshTokenResponse>(CancellationToken);

        // Assert
        result.ShouldBeProblem();
        result.Problem.ShouldNotBeNull();
        result.Problem.Status.ShouldBe(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task RefreshToken_CanBeUsedMultipleTimes_ToGetNewTokens()
    {
        var firstRefreshCommand = new RefreshTokenCommand { RefreshToken = UserRefreshToken };
        var firstRefreshResponse = await ApiClient.PostAsJsonAsync(Endpoint, firstRefreshCommand, CancellationToken);
        var firstRefreshResult = await firstRefreshResponse.ReadResponseOrProblemAsync<RefreshTokenResponse>(
            CancellationToken
        );
        firstRefreshResult.ShouldBeSuccessful();

        var secondRefreshToken = firstRefreshResult.Response!.RefreshToken;
        var secondRefreshCommand = new RefreshTokenCommand { RefreshToken = secondRefreshToken };
        var secondRefreshResponse = await ApiClient.PostAsJsonAsync(Endpoint, secondRefreshCommand, CancellationToken);
        var secondRefreshResult = await secondRefreshResponse.ReadResponseOrProblemAsync<RefreshTokenResponse>(
            CancellationToken
        );
        secondRefreshResult.ShouldBeSuccessful();
    }
}
