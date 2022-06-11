using System;
using System.Net;
using System.Threading.Tasks;
using Application.Commands.OperationTags;
using Domain.Common;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using NFluent;
using NUnit.Framework;

namespace Api.IntegrationTests.Commands.OperationTags;

[TestFixture]
public class UpdateOperationTagTests : TestBase
{
    [Test]
    public async Task UpdateOperationTag_WithValidRequest_Should_UpdateOperationTag()
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
        var request = new UpdateOperationTagRequest("updated", "#000000");

        // Act
        var response = await HttpClient.PatchAsJsonAsync($"operationTags/{operationTag.Id}", request);
        var result = await response.Content.ReadFromJsonOrDefaultAsync<UpdateOperationTagResponse>();
        var operationTagInDb = await GetDbContext().OperationTags.FirstOrDefaultAsync();

        // Assert
        Check.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        Check.That(operationTagInDb).IsNotNull();
        Check.That(operationTagInDb?.Name).IsEqualTo(request.Name);
        Check.That(operationTagInDb?.Color).IsEqualTo(request.Color);
        Check.That(result).IsNotNull();
        Check.That(result?.Name).IsEqualTo(request.Name);
        Check.That(result?.Color).IsEqualTo(request.Color);
    }

    [TestCase("", "")]
    [TestCase("string", "")]
    [TestCase("", "#123456")]
    [TestCase("string", "456789564")]
    public async Task UpdateOperationTag_WithWrongRequest_ShouldReturn_ErrorResponse(string name, string color)
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
        var request = new UpdateOperationTagRequest(name, color);

        // Act
        var response = await HttpClient.PatchAsJsonAsync($"operationTags/{operationTag.Id}", request);
        var result = await response.Content.ReadFromJsonOrDefaultAsync<ErrorResponse>();
        var operationTagInDb = await GetDbContext().OperationTags.FirstOrDefaultAsync();

        // Assert
        Check.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
        Check.That(operationTagInDb).IsNotNull();
        Check.That(operationTagInDb?.Name).IsEqualTo(operationTag.Name);
        Check.That(operationTagInDb?.Color).IsEqualTo(operationTag.Color);
        Check.That(result).IsNotNull();
        Check.That(result?.Message).IsNotEmpty();
    }
}