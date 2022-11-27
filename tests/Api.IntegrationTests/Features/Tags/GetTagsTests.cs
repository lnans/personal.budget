using Application.Features.Tags.GetTags;

namespace Api.IntegrationTests.Features.Tags;

[Collection("Shared")]
public class GetTagsTests : TestBase
{
    public GetTagsTests(ApiFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetAllTags_ShouldReturn_TagsList()
    {
        // Arrange
        var dbContext = DbContext();
        var tags = new List<Tag>();
        Enumerable
            .Range(0, 20)
            .ToList()
            .ForEach(arg => tags.Add(new Tag
            {
                Name = $"string{arg}",
                OwnerId = FakeJwtManager.UserId,
                Color = "#123456"
            }));
        await dbContext.Tags.AddRangeAsync(tags);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        // Act
        var response = await Api.GetAsync("tags");
        var result = await response.Content.ReadFromJsonOrDefaultAsync<IEnumerable<GetTagsResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.Count().Should().Be(20);
    }
}