using System.Net.Http.Json;
using Api.Contracts.Tags;
using Application.Features.Tags.Commands.CreateTag;
using Domain.Tags;
using Microsoft.AspNetCore.Http;

namespace Api.Tests.Tags;

[Collection(ApiTestCollection.CollectionName)]
public class CreateTagTests : ApiTestBase
{
    private const string Endpoint = "/tags";

    public CreateTagTests(ApiTestFixture factory)
        : base(factory) { }

    [Fact]
    public async Task CreateTag_WithValidData_ShouldCreateTag()
    {
        var request = new CreateTagRequest("Groceries", "#FF5733");

        var response = await ApiClient.LoggedAs(UserToken).PostAsJsonAsync(Endpoint, request, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<CreateTagResponse>(CancellationToken);

        result.ShouldBeSuccessful();
        result.Response.ShouldNotBeNull();
        result.Response.Name.ShouldBe(request.Name);
        result.Response.Color.ShouldBe(request.Color);
        result.Response.Id.ShouldNotBe(Guid.Empty);
        result.Response.CreatedAt.ShouldBeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
        result.Response.UpdatedAt.ShouldBeCloseTo(result.Response.CreatedAt, TimeSpan.FromMilliseconds(1));
    }

    [Fact]
    public async Task CreateTag_WithEmptyName_ShouldReturnValidationError()
    {
        var request = new CreateTagRequest("", "#FF5733");

        var response = await ApiClient.LoggedAs(UserToken).PostAsJsonAsync(Endpoint, request, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<CreateTagResponse>(CancellationToken);

        result.ShouldBeProblem();
        result.Problem.ShouldNotBeNull();
        result.Problem.Status.ShouldBe(StatusCodes.Status400BadRequest);
        result.Problem.ShouldHaveError(TagErrors.TagNameRequired.Code);
    }

    [Fact]
    public async Task CreateTag_WithTooLongName_ShouldReturnValidationError()
    {
        var request = new CreateTagRequest(new string('a', TagConstants.MaxNameLength + 1), "#FF5733");

        var response = await ApiClient.LoggedAs(UserToken).PostAsJsonAsync(Endpoint, request, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<CreateTagResponse>(CancellationToken);

        result.ShouldBeProblem();
        result.Problem.ShouldNotBeNull();
        result.Problem.Status.ShouldBe(StatusCodes.Status400BadRequest);
        result.Problem.ShouldHaveError(TagErrors.TagNameTooLong.Code);
    }

    [Fact]
    public async Task CreateTag_WithEmptyColor_ShouldReturnValidationError()
    {
        var request = new CreateTagRequest("Groceries", "");

        var response = await ApiClient.LoggedAs(UserToken).PostAsJsonAsync(Endpoint, request, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<CreateTagResponse>(CancellationToken);

        result.ShouldBeProblem();
        result.Problem.ShouldNotBeNull();
        result.Problem.Status.ShouldBe(StatusCodes.Status400BadRequest);
        result.Problem.ShouldHaveError(TagErrors.TagColorRequired.Code);
    }

    [Fact]
    public async Task CreateTag_WithInvalidColor_ShouldReturnValidationError()
    {
        var request = new CreateTagRequest("Groceries", "invalid");

        var response = await ApiClient.LoggedAs(UserToken).PostAsJsonAsync(Endpoint, request, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<CreateTagResponse>(CancellationToken);

        result.ShouldBeProblem();
        result.Problem.ShouldNotBeNull();
        result.Problem.Status.ShouldBe(StatusCodes.Status400BadRequest);
        result.Problem.ShouldHaveError(TagErrors.TagColorInvalid.Code);
    }

    [Fact]
    public async Task CreateTag_ShouldPersistInDatabase()
    {
        var request = new CreateTagRequest("Persistent Tag", "#AABBCC");

        var response = await ApiClient.LoggedAs(UserToken).PostAsJsonAsync(Endpoint, request, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<CreateTagResponse>(CancellationToken);

        result.ShouldBeSuccessful();
        result.Response.ShouldNotBeNull();

        var tagInDb = await DbContext.Tags.FindAsync([result.Response.Id], CancellationToken);
        tagInDb.ShouldNotBeNull();
        tagInDb.Name.ShouldBe(request.Name);
        tagInDb.Color.ShouldBe(request.Color);
        tagInDb.UserId.ShouldBe(User.Id);
        tagInDb.CreatedAt.ShouldBeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
        tagInDb.UpdatedAt.ShouldBeCloseTo(tagInDb.CreatedAt, TimeSpan.FromMilliseconds(1));
    }
}
