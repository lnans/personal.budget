using System.Net;
using System.Net.Http.Json;
using Application.Features.Authentication.Commands.SignIn;
using Domain.Users;
using Microsoft.AspNetCore.Http;
using TestFixtures.Domain;

namespace Api.Tests.Authentication;

[Collection(ApiTestCollection.CollectionName)]
public class SignInTests : ApiTestBase
{
    private const string Endpoint = "/auth/sign-in";

    public SignInTests(ApiTestFixture factory)
        : base(factory) { }

    [Fact]
    public async Task SignIn_ReturnsToken_WhenCredentialsAreValid()
    {
        // Arrange
        var query = new SignInCommand { Login = User.Login, Password = UserPassword };

        // Act
        var response = await ApiClient.PostAsJsonAsync(Endpoint, query, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<SignInResponse>(CancellationToken);

        // Assert
        result.ShouldBeSuccessful();
        result.Response.ShouldNotBeNull();
        result.Response.Bearer.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task SignIn_ReturnsUnauthorized_WhenUserDoesNotExist()
    {
        // Arrange
        var query = new SignInCommand { Login = "nonexistentuser", Password = "SomePassword123!" };

        // Act
        var response = await ApiClient.PostAsJsonAsync(Endpoint, query, CancellationToken);
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
        var query = new SignInCommand { Login = User.Login, Password = "WrongPassword123!" };

        // Act
        var response = await ApiClient.PostAsJsonAsync(Endpoint, query, CancellationToken);
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
        var query = new SignInCommand { Login = string.Empty, Password = "SomePassword123!" };

        // Act
        var response = await ApiClient.PostAsJsonAsync(Endpoint, query, CancellationToken);
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
        var query = new SignInCommand { Login = longLogin, Password = "SomePassword123!" };

        // Act
        var response = await ApiClient.PostAsJsonAsync(Endpoint, query, CancellationToken);
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
        var query = new SignInCommand { Login = "testuser", Password = string.Empty };

        // Act
        var response = await ApiClient.PostAsJsonAsync(Endpoint, query, CancellationToken);
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
        var query = new SignInCommand { Login = string.Empty, Password = string.Empty };

        // Act
        var response = await ApiClient.PostAsJsonAsync(Endpoint, query, CancellationToken);
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
