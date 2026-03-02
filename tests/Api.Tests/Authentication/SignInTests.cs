using System.Net.Http.Json;
using Api.Contracts.Authentication;
using Application.Features.Authentication.Commands.SignIn;
using Domain.Users;
using Microsoft.AspNetCore.Http;
using TestFixtures.Domain;

namespace Api.Tests.Authentication;

[Collection(ApiTestCollection.CollectionName)]
public class SignInTests : ApiTestBase
{
    private const string Endpoint = "/auth/signin";

    public SignInTests(ApiTestFixture factory)
        : base(factory) { }

    [Fact]
    public async Task SignIn_ReturnsToken_WhenCredentialsAreValid()
    {
        // Arrange
        var request = new SignInRequest(User.Login, UserPassword);

        // Act
        var response = await ApiClient.PostAsJsonAsync(Endpoint, request, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<SignInResponse>(CancellationToken);

        // Assert
        result.ShouldBeSuccessful();
        result.Response.ShouldNotBeNull();
        result.Response.Bearer.ShouldNotBeNullOrWhiteSpace();
        result.Response.RefreshToken.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task SignIn_ReturnsTokens_WithDifferentValues()
    {
        // Arrange
        var request = new SignInRequest(User.Login, UserPassword);

        // Act
        var response = await ApiClient.PostAsJsonAsync(Endpoint, request, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<SignInResponse>(CancellationToken);

        // Assert
        result.ShouldBeSuccessful();
        result.Response.ShouldNotBeNull();
        result.Response.Bearer.ShouldNotBe(result.Response.RefreshToken);
    }

    [Fact]
    public async Task SignIn_ReturnsUnauthorized_WhenUserDoesNotExist()
    {
        // Arrange
        var request = new SignInRequest("nonexistentuser", "SomePassword123!");

        // Act
        var response = await ApiClient.PostAsJsonAsync(Endpoint, request, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<SignInResponse>(CancellationToken);

        // Assert
        result.ShouldBeProblem();
        result.Problem.ShouldNotBeNull();
        result.Problem.Status.ShouldBe(StatusCodes.Status401Unauthorized);
    }

    [Fact]
    public async Task SignIn_ReturnsUnauthorized_WhenPasswordIsIncorrect()
    {
        // Arrange
        var request = new SignInRequest(User.Login, "WrongPassword123!");

        // Act
        var response = await ApiClient.PostAsJsonAsync(Endpoint, request, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<SignInResponse>(CancellationToken);

        // Assert
        result.ShouldBeProblem();
        result.Problem.ShouldNotBeNull();
        result.Problem.Status.ShouldBe(StatusCodes.Status401Unauthorized);
    }

    [Fact]
    public async Task SignIn_ReturnsBadRequest_WhenLoginIsEmpty()
    {
        // Arrange
        var request = new SignInRequest(string.Empty, "SomePassword123!");

        // Act
        var response = await ApiClient.PostAsJsonAsync(Endpoint, request, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<SignInResponse>(CancellationToken);

        // Assert
        result.ShouldBeProblem();
        result.Problem.ShouldNotBeNull();
        result.Problem.Status.ShouldBe(StatusCodes.Status400BadRequest);
        result.Problem.ShouldHaveValidationError("Login", UserErrors.UserLoginRequired.Code);
    }

    [Fact]
    public async Task SignIn_ReturnsBadRequest_WhenLoginIsTooLong()
    {
        // Arrange
        var longLogin = UserFixture.GenerateLongLogin();
        var request = new SignInRequest(longLogin, "SomePassword123!");

        // Act
        var response = await ApiClient.PostAsJsonAsync(Endpoint, request, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<SignInResponse>(CancellationToken);

        // Assert
        result.ShouldBeProblem();
        result.Problem.ShouldNotBeNull();
        result.Problem.Status.ShouldBe(StatusCodes.Status400BadRequest);
        result.Problem.ShouldHaveValidationError("Login", UserErrors.UserLoginTooLong.Code);
    }

    [Fact]
    public async Task SignIn_ReturnsBadRequest_WhenPasswordIsEmpty()
    {
        // Arrange
        var request = new SignInRequest("testuser", string.Empty);

        // Act
        var response = await ApiClient.PostAsJsonAsync(Endpoint, request, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<SignInResponse>(CancellationToken);

        // Assert
        result.ShouldBeProblem();
        result.Problem.ShouldNotBeNull();
        result.Problem.Status.ShouldBe(StatusCodes.Status400BadRequest);
        result.Problem.ShouldHaveValidationError("Password", UserErrors.UserPasswordRequired.Code);
    }

    [Fact]
    public async Task SignIn_ReturnsBadRequest_WhenBothLoginAndPasswordAreEmpty()
    {
        // Arrange
        var request = new SignInRequest(string.Empty, string.Empty);

        // Act
        var response = await ApiClient.PostAsJsonAsync(Endpoint, request, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<SignInResponse>(CancellationToken);

        // Assert
        result.ShouldBeProblem();
        result.Problem.ShouldNotBeNull();
        result.Problem.Status.ShouldBe(StatusCodes.Status400BadRequest);
        result.Problem.ShouldHaveValidationErrors(
            ("Login", UserErrors.UserLoginRequired.Code),
            ("Password", UserErrors.UserPasswordRequired.Code)
        );
    }
}
