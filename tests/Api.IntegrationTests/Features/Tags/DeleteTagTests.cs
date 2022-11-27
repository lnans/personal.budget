namespace Api.IntegrationTests.Features.Tags;

[Collection("Shared")]
public class DeleteTagTests : TestBase
{
    public DeleteTagTests(ApiFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task DeleteTag_Should_DeleteTag()
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

        // Act
        var response = await Api.DeleteAsync($"tags/{tag.Id}");
        var tagInDb = await DbContext().Tags.FirstOrDefaultAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        tagInDb.Should().BeNull();
    }

    [Fact]
    public async Task DeleteTag_WhenIdNotExist_ShouldReturn_404NotFound()
    {
        // Act
        var response = await Api.DeleteAsync($"tags/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteTag_WhenTagIsAssigned_ShouldReturn_400BadRequest()
    {
        // Arrange
        var dbContext = DbContext();
        var account = new Account
        {
            OwnerId = FakeJwtManager.UserId,
            InitialBalance = 0,
            Balance = 0,
            Bank = "bank",
            Name = "name",
            Type = AccountType.Expenses,
            CreationDate = DateTime.UtcNow,
            Operations = new List<Operation>
            {
                new()
                {
                    Amount = 10,
                    Description = "operation",
                    Type = OperationType.Budget,
                    CreationDate = DateTime.UtcNow,
                    OwnerId = FakeJwtManager.UserId,
                    Tags = new List<Tag>
                    {
                        new() { Name = "tag", Color = "#000000", OwnerId = FakeJwtManager.UserId }
                    }
                }
            }
        };
        await dbContext.Accounts.AddAsync(account);
        await dbContext.SaveChangesAsync(CancellationToken.None);
        var tagId = account.Operations!.First().Tags!.First().Id;

        // Act
        var response = await Api.DeleteAsync($"tags/{tagId}");
        var tagInDb = await DbContext().Tags.FirstOrDefaultAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        tagInDb.Should().NotBeNull();
    }
}