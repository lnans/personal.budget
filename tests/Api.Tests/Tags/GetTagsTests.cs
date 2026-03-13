using Application.Features.Tags.Queries.GetTags;
using TestFixtures.Domain;

namespace Api.Tests.Tags;

[Collection(ApiTestCollection.CollectionName)]
public class GetTagsTests : ApiTestBase
{
    private const string Endpoint = "/tags";

    public GetTagsTests(ApiTestFixture factory)
        : base(factory) { }

    [Fact]
    public async Task GetTags_ReturnsEmptyList_WhenNoTagsExist()
    {
        var response = await ApiClient.LoggedAs(UserToken).GetAsync(Endpoint, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<List<GetTagsResponse>>(CancellationToken);

        result.ShouldBeSuccessful();
        result.Response.ShouldNotBeNull();
        result.Response.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetTags_ReturnsTagsList_WhenTagsExist()
    {
        var tag1 = TagFixture.CreateValidTag(User.Id, name: "Tag One", color: "#FF0000");
        var tag2 = TagFixture.CreateValidTag(User.Id, name: "Tag Two", color: "#00FF00");
        DbContext.Tags.AddRange(tag1, tag2);
        await DbContext.SaveChangesAsync(CancellationToken);

        var response = await ApiClient.LoggedAs(UserToken).GetAsync(Endpoint, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<List<GetTagsResponse>>(CancellationToken);

        result.ShouldBeSuccessful();
        result.Response.ShouldNotBeNull();
        result.Response.Count.ShouldBe(2);

        var resultTag1 = result.Response.FirstOrDefault(tag => tag.Id == tag1.Id);
        resultTag1.ShouldNotBeNull();
        resultTag1.Name.ShouldBe(tag1.Name);
        resultTag1.Color.ShouldBe(tag1.Color);
        resultTag1.CreatedAt.ShouldBeCloseTo(tag1.CreatedAt, TimeSpan.FromMilliseconds(1));
        resultTag1.UpdatedAt.ShouldBeCloseTo(tag1.UpdatedAt, TimeSpan.FromMilliseconds(1));

        var resultTag2 = result.Response.FirstOrDefault(tag => tag.Id == tag2.Id);
        resultTag2.ShouldNotBeNull();
        resultTag2.Name.ShouldBe(tag2.Name);
        resultTag2.Color.ShouldBe(tag2.Color);
    }
}
