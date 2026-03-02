using System.Net.Http.Json;
using Api.Contracts.Authentication;
using Application.Features.Authentication.Commands.RefreshToken;
using Microsoft.AspNetCore.Http;

namespace Api.Tests.Authentication;

[Collection(ApiTestCollection.CollectionName)]
public class RefreshTokenTests : ApiTestBase
{
    private const string Endpoint = "/auth/refresh";

    public RefreshTokenTests(ApiTestFixture factory)
        : base(factory) { }

    [Fact]
    public async Task RefreshToken_ReturnsNewTokens_WhenRefreshTokenIsValid()
    {
        // Arrange
        var request = new RefreshTokenRequest(UserRefreshToken);

        // Act
        var response = await ApiClient.PostAsJsonAsync(Endpoint, request, CancellationToken);
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
        var request = new RefreshTokenRequest("invalid-token");

        // Act
        var response = await ApiClient.PostAsJsonAsync(Endpoint, request, CancellationToken);
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
        var request = new RefreshTokenRequest(
            "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJleHAiOjE2MDAwMDAwMDAsInN1YiI6IjEyMzQ1Njc4OTAifQ.invalid"
        );

        // Act
        var response = await ApiClient.PostAsJsonAsync(Endpoint, request, CancellationToken);
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
        var request = new RefreshTokenRequest(UserToken);

        // Act
        var response = await ApiClient.PostAsJsonAsync(Endpoint, request, CancellationToken);
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
        var request = new RefreshTokenRequest(string.Empty);

        // Act
        var response = await ApiClient.PostAsJsonAsync(Endpoint, request, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<RefreshTokenResponse>(CancellationToken);

        // Assert
        result.ShouldBeProblem();
        result.Problem.ShouldNotBeNull();
        result.Problem.Status.ShouldBe(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task RefreshToken_CanBeUsedMultipleTimes_ToGetNewTokens()
    {
        var firstRefreshRequest = new RefreshTokenRequest(UserRefreshToken);
        var firstRefreshResponse = await ApiClient.PostAsJsonAsync(Endpoint, firstRefreshRequest, CancellationToken);
        var firstRefreshResult = await firstRefreshResponse.ReadResponseOrProblemAsync<RefreshTokenResponse>(
            CancellationToken
        );
        firstRefreshResult.ShouldBeSuccessful();

        var secondRefreshToken = firstRefreshResult.Response!.RefreshToken;
        var secondRefreshRequest = new RefreshTokenRequest(secondRefreshToken);
        var secondRefreshResponse = await ApiClient.PostAsJsonAsync(Endpoint, secondRefreshRequest, CancellationToken);
        var secondRefreshResult = await secondRefreshResponse.ReadResponseOrProblemAsync<RefreshTokenResponse>(
            CancellationToken
        );
        secondRefreshResult.ShouldBeSuccessful();
    }
}
