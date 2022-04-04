using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Domain.Entities;
using NFluent;
using NUnit.Framework;

namespace Api.IntegrationTests.Commands.OperationTags;

[TestFixture]
public class DeleteOperationTagTests : TestBase
{
    [Test]
    public async Task DeleteOperationTag_WithKnownId_Should_DeleteOperationTag()
    {
        // Arrange
        var dbContext = GetDbContext();
        var operationTag = new OperationTag
        {
            Id = Guid.NewGuid().ToString(),
            Name = "string",
            Color = "#123456",
            OwnerId = DefaultUser.Id
        };
        dbContext.OperationTags.Add(operationTag);
        await dbContext.SaveChangesAsync();

        // Act
        var response = await HttpClient.DeleteAsync($"OperationTags/{operationTag.Id}");
        var operationTagInDb = GetDbContext().OperationTags.FirstOrDefault();

        // Assert
        Check.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        Check.That(operationTagInDb).IsNull();
    }
    
    [Test]
    public async Task DeleteOperationTag_WithUnknownId_ShouldReturn_ErrorResponse()
    {
        // Arrange
        var dbContext = GetDbContext();
        var operationTag = new OperationTag
        {
            Id = Guid.NewGuid().ToString(),
            Name = "string",
            Color = "#123456",
            OwnerId = DefaultUser.Id
        };
        dbContext.OperationTags.Add(operationTag);
        await dbContext.SaveChangesAsync();

        // Act
        var response = await HttpClient.DeleteAsync("OperationTags/none");
        var operationTagInDb = GetDbContext().OperationTags.FirstOrDefault();

        // Assert
        Check.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
        Check.That(operationTagInDb).IsNotNull();
    }
}