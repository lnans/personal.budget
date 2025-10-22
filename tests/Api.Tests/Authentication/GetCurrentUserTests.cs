using System.Net;
using Application.Features.Authentication.Queries.GetCurrentUser;
using Domain.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Api.Tests.Authentication;

[Collection(ApiTestCollection.CollectionName)]
public class GetCurrentUserTests : ApiTestBase
{
    private const string Endpoint = "/auth";

    public GetCurrentUserTests(ApiTestFixture factory)
        : base(factory) { }

    [Fact]
    public async Task GetCurrentUser_ReturnsUserDetails_WhenAuthenticated()
    {
        // Act
        var response = await ApiClient.LoggedAs(UserToken).GetAsync(Endpoint, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<GetCurrentUserResponse>(CancellationToken);

        // Assert
        result.ShouldBeSuccessful();
        result.Response.ShouldNotBeNull();
        result.Response.UserId.ShouldBe(User.Id);
        result.Response.Login.ShouldBe(User.Login);
    }

    [Fact]
    public async Task GetCurrentUser_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        // Act
        var response = await ApiClient.GetAsync(Endpoint, CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCurrentUser_ReturnsNotFound_WhenUserDoesNotExistInDatabase()
    {
        // Arrange - Delete the user from the database
        await DbContext.Users.Where(u => u.Id == User.Id).ExecuteDeleteAsync(CancellationToken);
        await DbContext.SaveChangesAsync(CancellationToken);

        // Act - Call the endpoint with a valid token for a deleted user
        var response = await ApiClient.LoggedAs(UserToken).GetAsync(Endpoint, CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        var result = await response.ReadResponseOrProblemAsync<GetCurrentUserResponse>(CancellationToken);
        result.ShouldBeProblem();
        result.Problem.ShouldNotBeNull();
        result.Problem.Status.ShouldBe(StatusCodes.Status404NotFound);
        result.Problem.Detail.ShouldBe(UserErrors.UserNotFound.Description);
        result.Problem.Title.ShouldBe("Not Found");
    }
}
