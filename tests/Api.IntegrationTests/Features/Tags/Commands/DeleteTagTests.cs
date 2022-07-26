using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Domain.Entities;
using NFluent;
using NUnit.Framework;

namespace Api.IntegrationTests.Features.Tags.Commands;

[TestFixture]
public class DeleteTagTests : TestBase
{
    [Test]
    public async Task DeleteTag_WithKnownId_Should_DeleteTag()
    {
        // Arrange
        var dbContext = GetDbContext();
        var tag = new Tag
        {
            Id = Guid.NewGuid().ToString(),
            Name = "string",
            Color = "#123456",
            OwnerId = DefaultUser.Id
        };
        dbContext.Tags.Add(tag);
        await dbContext.SaveChangesAsync();

        // Act
        var response = await HttpClient.DeleteAsync($"Tags/{tag.Id}");
        var tagInDb = GetDbContext().Tags.FirstOrDefault();

        // Assert
        Check.That(response.StatusCode).IsEqualTo(HttpStatusCode.NoContent);
        Check.That(tagInDb).IsNull();
    }

    [Test]
    public async Task DeleteTag_WithUnknownId_ShouldReturn_ErrorResponse()
    {
        // Arrange
        var dbContext = GetDbContext();
        var tag = new Tag
        {
            Id = Guid.NewGuid().ToString(),
            Name = "string",
            Color = "#123456",
            OwnerId = DefaultUser.Id
        };
        dbContext.Tags.Add(tag);
        await dbContext.SaveChangesAsync();

        // Act
        var response = await HttpClient.DeleteAsync("Tags/none");
        var tagInDb = GetDbContext().Tags.FirstOrDefault();

        // Assert
        Check.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
        Check.That(tagInDb).IsNotNull();
    }
}