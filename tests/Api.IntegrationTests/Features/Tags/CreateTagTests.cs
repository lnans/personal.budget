using Application.Features.Tags.CreateTag;

namespace Api.IntegrationTests.Features.Tags;

[Collection("Shared")]
public class CreateTagTests : TestBase
{
    public CreateTagTests(ApiFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task CreateTag_Should_CreateTag()
    {
        // Arrange
        var request = new CreateTagRequest
        {
            Name = "tag",
            Color = "#123456"
        };

        // Act
        var response = await Api.PostAsJsonAsync("tags", request);
        var tagInDb = await DbContext().Tags.FirstOrDefaultAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        tagInDb.Should().NotBeNull();
        tagInDb!.Color.Should().Be(request.Color);
        tagInDb.Name.Should().Be(request.Name);
    }

    [Fact]
    public async Task CreateTag_WhenAlreadyExist_ShouldReturn_409Conflict()
    {
        // Arrange
        var dbContext = DbContext();
        var tag = new Tag
        {
            Name = "tag",
            OwnerId = FakeJwtManager.UserId,
            Color = "#123456"
        };
        await dbContext.Tags.AddAsync(tag);
        await dbContext.SaveChangesAsync(CancellationToken.None);
        var request = new CreateTagRequest
        {
            Name = "tag",
            Color = "#123456"
        };

        // Act
        var response = await Api.PostAsJsonAsync("tags", request);
        var tagsCount = await DbContext().Tags.CountAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        tagsCount.Should().Be(1);
    }

    [Theory]
    [InlineData("", "")]
    [InlineData("", "colorinvalid")]
    [InlineData(null, "")]
    [InlineData("", null)]
    [InlineData(null, null)]
    public async Task CreateTag_WhenFormIsInvalid_ShouldReturn_400BadRequest(string name, string color)
    {
        // Arrange
        var request = new CreateTagRequest
        {
            Name = name,
            Color = color
        };

        // Act
        var response = await Api.PostAsJsonAsync("tags", request);
        var tagInDb = await DbContext().Tags.FirstOrDefaultAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        tagInDb.Should().BeNull();
    }
}