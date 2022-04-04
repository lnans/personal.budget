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
        var operationTag = new OperationTag
        {
            Id = Guid.NewGuid().ToString(),
            Name = "string",
            Color = "#123456",
            OwnerId = DefaultUser.Id
        };
        DbContext.OperationTags.Add(operationTag);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await HttpClient.DeleteAsync($"OperationTags/{operationTag.Id}");
        var operationTagInDb = DbContext.OperationTags.FirstOrDefault();

        // Assert
        Check.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        Check.That(operationTagInDb).IsNull();
    }
    
    [Test]
    public async Task DeleteOperationTag_WithUnkownId_ShouldReturn_ErrorResponse()
    {
        // Arrange
        var operationTag = new OperationTag
        {
            Id = Guid.NewGuid().ToString(),
            Name = "string",
            Color = "#123456",
            OwnerId = DefaultUser.Id
        };
        DbContext.OperationTags.Add(operationTag);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await HttpClient.DeleteAsync("OperationTags/none");
        var operationTagInDb = DbContext.OperationTags.FirstOrDefault();

        // Assert
        Check.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
        Check.That(operationTagInDb).IsNotNull();
    }
}