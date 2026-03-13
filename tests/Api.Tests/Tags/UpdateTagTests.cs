using System.Net.Http.Json;
using Api.Contracts.Tags;
using Application.Features.Tags.Commands.UpdateTag;
using Domain.Tags;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TestFixtures.Domain;

namespace Api.Tests.Tags;

[Collection(ApiTestCollection.CollectionName)]
public class UpdateTagTests : ApiTestBase
{
    private const string BaseEndpoint = "/tags";

    public UpdateTagTests(ApiTestFixture factory)
        : base(factory) { }

    [Fact]
    public async Task UpdateTag_WithValidData_ShouldUpdateTag()
    {
        var tag = TagFixture.CreateValidTag(User.Id, name: "Original", color: "#FF0000");
        DbContext.Tags.Add(tag);
        await DbContext.SaveChangesAsync(CancellationToken);

        var originalCreatedAt = tag.CreatedAt;
        var request = new UpdateTagRequest("Updated", "#00FF00");

        var response = await ApiClient
            .LoggedAs(UserToken)
            .PutAsJsonAsync($"{BaseEndpoint}/{tag.Id}", request, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<UpdateTagResponse>(CancellationToken);

        result.ShouldBeSuccessful();
        result.Response.ShouldNotBeNull();
        result.Response.Id.ShouldBe(tag.Id);
        result.Response.Name.ShouldBe(request.Name);
        result.Response.Color.ShouldBe(request.Color);
        result.Response.CreatedAt.ShouldBeCloseTo(originalCreatedAt, TimeSpan.FromMilliseconds(1));
        result.Response.UpdatedAt.ShouldBeGreaterThan(result.Response.CreatedAt);
        result.Response.UpdatedAt.ShouldBeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task UpdateTag_WithEmptyName_ShouldReturnValidationError()
    {
        var tag = TagFixture.CreateValidTag(User.Id);
        DbContext.Tags.Add(tag);
        await DbContext.SaveChangesAsync(CancellationToken);

        var request = new UpdateTagRequest("", "#00FF00");

        var response = await ApiClient
            .LoggedAs(UserToken)
            .PutAsJsonAsync($"{BaseEndpoint}/{tag.Id}", request, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<UpdateTagResponse>(CancellationToken);

        result.ShouldBeProblem();
        result.Problem.ShouldNotBeNull();
        result.Problem.Status.ShouldBe(StatusCodes.Status400BadRequest);
        result.Problem.ShouldHaveError(TagErrors.TagNameRequired.Code);
    }

    [Fact]
    public async Task UpdateTag_WithTooLongName_ShouldReturnValidationError()
    {
        var tag = TagFixture.CreateValidTag(User.Id);
        DbContext.Tags.Add(tag);
        await DbContext.SaveChangesAsync(CancellationToken);

        var request = new UpdateTagRequest(TagFixture.GenerateLongTagName(), "#00FF00");

        var response = await ApiClient
            .LoggedAs(UserToken)
            .PutAsJsonAsync($"{BaseEndpoint}/{tag.Id}", request, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<UpdateTagResponse>(CancellationToken);

        result.ShouldBeProblem();
        result.Problem.ShouldNotBeNull();
        result.Problem.Status.ShouldBe(StatusCodes.Status400BadRequest);
        result.Problem.ShouldHaveError(TagErrors.TagNameTooLong.Code);
    }

    [Fact]
    public async Task UpdateTag_WithInvalidColor_ShouldReturnValidationError()
    {
        var tag = TagFixture.CreateValidTag(User.Id);
        DbContext.Tags.Add(tag);
        await DbContext.SaveChangesAsync(CancellationToken);

        var request = new UpdateTagRequest("Updated", "invalid");

        var response = await ApiClient
            .LoggedAs(UserToken)
            .PutAsJsonAsync($"{BaseEndpoint}/{tag.Id}", request, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<UpdateTagResponse>(CancellationToken);

        result.ShouldBeProblem();
        result.Problem.ShouldNotBeNull();
        result.Problem.Status.ShouldBe(StatusCodes.Status400BadRequest);
        result.Problem.ShouldHaveError(TagErrors.TagColorInvalid.Code);
    }

    [Fact]
    public async Task UpdateTag_WithNonExistentId_ShouldReturnNotFound()
    {
        var nonExistentId = Guid.NewGuid();
        var request = new UpdateTagRequest("Updated", "#00FF00");

        var response = await ApiClient
            .LoggedAs(UserToken)
            .PutAsJsonAsync($"{BaseEndpoint}/{nonExistentId}", request, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<UpdateTagResponse>(CancellationToken);

        result.ShouldBeProblem();
        result.Problem.ShouldNotBeNull();
        result.Problem.Status.ShouldBe(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task UpdateTag_ShouldPersistInDatabase()
    {
        var tag = TagFixture.CreateValidTag(User.Id, name: "Original", color: "#FF0000");
        DbContext.Tags.Add(tag);
        await DbContext.SaveChangesAsync(CancellationToken);

        var originalCreatedAt = tag.CreatedAt;
        var request = new UpdateTagRequest("Updated", "#00FF00");

        var response = await ApiClient
            .LoggedAs(UserToken)
            .PutAsJsonAsync($"{BaseEndpoint}/{tag.Id}", request, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<UpdateTagResponse>(CancellationToken);

        result.ShouldBeSuccessful();
        result.Response.ShouldNotBeNull();

        var tagInDb = await DbContext.Tags.AsNoTracking().FirstOrDefaultAsync(t => t.Id == tag.Id, CancellationToken);
        tagInDb.ShouldNotBeNull();
        tagInDb.Name.ShouldBe(request.Name);
        tagInDb.Color.ShouldBe(request.Color);
        tagInDb.UserId.ShouldBe(User.Id);
        tagInDb.CreatedAt.ShouldBeCloseTo(originalCreatedAt, TimeSpan.FromMilliseconds(1));
        tagInDb.UpdatedAt.ShouldBeGreaterThan(tagInDb.CreatedAt);
        tagInDb.UpdatedAt.ShouldBeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }
}
