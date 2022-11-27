using Application.Features.Tags.UpdateTag;

namespace Api.IntegrationTests.Features.Tags;

[Collection("Shared")]
public class UpdateTagTests : TestBase
{
    public UpdateTagTests(ApiFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task UpdateTag_Should_UpdateTag()
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
        var request = new UpdateTagRequest { Name = "updated", Color = "#000000" };

        // Act
        var response = await Api.PatchAsJsonAsync($"tags/{tag.Id}", request);
        var tagInDb = await DbContext().Tags.FirstOrDefaultAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        tagInDb!.Name.Should().Be(request.Name);
        tagInDb.Color.Should().Be(request.Color);
    }

    [Fact]
    public async Task UpdateTag_WhenAlreadyExist_ShouldReturn_409Conflict()
    {
        // Arrange
        var dbContext = DbContext();
        var tag1 = new Tag
        {
            Name = "tag1",
            OwnerId = FakeJwtManager.UserId,
            Color = "#123456"
        };
        var tag2 = new Tag
        {
            Name = "tag2",
            OwnerId = FakeJwtManager.UserId,
            Color = "#123456"
        };
        await dbContext.Tags.AddRangeAsync(tag1, tag2);
        await dbContext.SaveChangesAsync(CancellationToken.None);
        var request = new UpdateTagRequest { Name = tag2.Name, Color = "#000000" };

        // Act
        var response = await Api.PatchAsJsonAsync($"tags/{tag1.Id}", request);
        var tagInDb = await DbContext().Tags.FirstOrDefaultAsync(t => t.Id == tag1.Id);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        tagInDb!.Name.Should().NotBe(request.Name);
        tagInDb.Color.Should().NotBe(request.Color);
    }

    [Theory]
    [InlineData("", "")]
    [InlineData("", "colorinvalid")]
    [InlineData(null, "")]
    [InlineData("", null)]
    [InlineData(null, null)]
    public async Task UpdateTag_WhenFormIsInvalid_ShouldReturn_400BadRequest(string name, string color)
    {
        // Arrange
        var request = new UpdateTagRequest
        {
            Name = name,
            Color = color
        };

        // Act
        var response = await Api.PatchAsJsonAsync($"tags/{Guid.NewGuid()}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}