using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Application.Commands.OperationTags;
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
        var operationTag = new OperationTag
        {
            Id = Guid.NewGuid().ToString(),
            Name = "string",
            Color = "#123456",
            OwnerId = DefaultUser.Id
        };
        DbContext.OperationTags.Add(operationTag);
        await DbContext.SaveChangesAsync();
        var request = new UpdateOperationTagRequest("updated", "#000000");

        // Act
        var response = await HttpClient.PatchAsJsonAsync($"operationTags/{operationTag.Id}", request);
        var result = await response.Content.ReadFromJsonOrDefaultAsync<UpdateOperationTagResponse>();
        var operationTagInDb = await DbContext.OperationTags.FirstOrDefaultAsync();

        // Assert
        Check.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        Check.That(operationTagInDb).IsNotNull();
        Check.That(operationTagInDb?.Name).IsEqualTo(request.Name);
        Check.That(operationTagInDb?.Color).IsEqualTo(request.Color);
        Check.That(result).IsNotNull();
        Check.That(result?.Name).IsEqualTo(request.Name);
        Check.That(result?.Color).IsEqualTo(request.Color);
    }
}