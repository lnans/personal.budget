using Application.Features.Tags.Commands.DeleteTag;
using Application.Features.Tags.Queries.GetTags;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TestFixtures.Domain;

namespace Api.Tests.Tags;

[Collection(ApiTestCollection.CollectionName)]
public class DeleteTagTests : ApiTestBase
{
    private const string Endpoint = "/tags";

    public DeleteTagTests(ApiTestFixture factory)
        : base(factory) { }

    [Fact]
    public async Task DeleteTag_WithValidId_ShouldSoftDeleteTag()
    {
        var tag = TagFixture.CreateValidTag(User.Id);
        DbContext.Tags.Add(tag);
        await DbContext.SaveChangesAsync(CancellationToken);

        var response = await ApiClient.LoggedAs(UserToken).DeleteAsync($"{Endpoint}/{tag.Id}", CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<DeleteTagResponse>(CancellationToken);

        result.ShouldBeSuccessful();
        result.Response.ShouldNotBeNull();
        result.Response.Id.ShouldBe(tag.Id);
        result.Response.Name.ShouldBe(tag.Name);
        result.Response.Color.ShouldBe(tag.Color);
        result.Response.DeletedAt.ShouldBeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task DeleteTag_AfterDeletion_ShouldNotBeReturnedInGetTags()
    {
        var tagToDelete = TagFixture.CreateValidTag(User.Id, name: "Delete Me");
        var tagToKeep = TagFixture.CreateValidTag(User.Id, name: "Keep Me");
        DbContext.Tags.AddRange(tagToDelete, tagToKeep);
        await DbContext.SaveChangesAsync(CancellationToken);

        await ApiClient.LoggedAs(UserToken).DeleteAsync($"{Endpoint}/{tagToDelete.Id}", CancellationToken);

        var getResponse = await ApiClient.LoggedAs(UserToken).GetAsync(Endpoint, CancellationToken);
        var getResult = await getResponse.ReadResponseOrProblemAsync<List<GetTagsResponse>>(CancellationToken);

        getResult.ShouldBeSuccessful();
        getResult.Response.ShouldNotBeNull();
        getResult.Response.Count.ShouldBe(1);
        getResult.Response.ShouldNotContain(t => t.Id == tagToDelete.Id);
        getResult.Response.ShouldContain(t => t.Id == tagToKeep.Id);
    }

    [Fact]
    public async Task DeleteTag_WithNonExistentId_ShouldReturnNotFound()
    {
        var nonExistentId = Guid.NewGuid();

        var response = await ApiClient
            .LoggedAs(UserToken)
            .DeleteAsync($"{Endpoint}/{nonExistentId}", CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<DeleteTagResponse>(CancellationToken);

        result.ShouldBeProblem();
        result.Problem.ShouldNotBeNull();
        result.Problem.Status.ShouldBe(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task DeleteTag_WhenAlreadyDeleted_ShouldReturnNotFound()
    {
        var tag = TagFixture.CreateValidTag(User.Id);
        DbContext.Tags.Add(tag);
        await DbContext.SaveChangesAsync(CancellationToken);

        var firstDeleteResponse = await ApiClient
            .LoggedAs(UserToken)
            .DeleteAsync($"{Endpoint}/{tag.Id}", CancellationToken);
        var firstResult = await firstDeleteResponse.ReadResponseOrProblemAsync<DeleteTagResponse>(CancellationToken);

        var secondDeleteResponse = await ApiClient
            .LoggedAs(UserToken)
            .DeleteAsync($"{Endpoint}/{tag.Id}", CancellationToken);
        var secondResult = await secondDeleteResponse.ReadResponseOrProblemAsync<DeleteTagResponse>(CancellationToken);

        firstResult.ShouldBeSuccessful();
        secondResult.ShouldBeProblem();
        secondResult.Problem.ShouldNotBeNull();
        secondResult.Problem.Status.ShouldBe(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task DeleteTag_ShouldPersistInDatabase()
    {
        var tag = TagFixture.CreateValidTag(User.Id);
        DbContext.Tags.Add(tag);
        await DbContext.SaveChangesAsync(CancellationToken);
        var tagId = tag.Id;

        var response = await ApiClient.LoggedAs(UserToken).DeleteAsync($"{Endpoint}/{tagId}", CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<DeleteTagResponse>(CancellationToken);

        result.ShouldBeSuccessful();

        using var freshScope = CreateFreshScope();
        var freshDbContext = freshScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tagInDb = await freshDbContext
            .Tags.IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == tagId, CancellationToken);
        tagInDb.ShouldNotBeNull();
        tagInDb.DeletedAt.ShouldNotBeNull();
        tagInDb.DeletedAt.Value.ShouldBeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task DeleteTag_ShouldUpdateUpdatedAtTimestamp()
    {
        var tag = TagFixture.CreateValidTag(User.Id);
        DbContext.Tags.Add(tag);
        await DbContext.SaveChangesAsync(CancellationToken);

        var response = await ApiClient.LoggedAs(UserToken).DeleteAsync($"{Endpoint}/{tag.Id}", CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<DeleteTagResponse>(CancellationToken);

        result.ShouldBeSuccessful();
        result.Response.ShouldNotBeNull();
        result.Response.UpdatedAt.ShouldBeCloseTo(result.Response.DeletedAt, TimeSpan.FromMilliseconds(1));
    }
}
