using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Application.Commands.OperationTag;
using Domain.Common;
using NFluent;
using NUnit.Framework;

namespace Api.IntegrationTests.OperationTag;

public class UpdateOperationTagTests : TestBase
{
    [Test]
    public async Task Put_WithValidRequest_ShouldReturn_OperationTagUpdated()
    {
        // Arrange
        var operationTag = new Domain.Entities.OperationTag() {Id = Guid.NewGuid().ToString(), Name = "", Color = ""};
        await DbContext.OperationTags.AddAsync(operationTag);
        await DbContext.SaveChangesAsync();
        var request = new UpdateOperationTagRequest("tag", "#123456");

        // Act
        var response = await HttpClient.PutAsJsonAsync($"/operationTags/{operationTag.Id}", request);
        var result = await response.Content.ReadFromJsonAsync<UpdateOperationTagResponse>();

        // Assert
        Check.That(result).IsNotNull();
        Check.That(result?.Id).IsEqualTo(operationTag.Id);
        Check.That(result?.Name).IsEqualTo("tag");
        Check.That(result?.Color).IsEqualTo("#123456");
    }
    
    [Test]
    public async Task Put_WithUnknownId_ShouldReturn_ErrorResponse()
    {
        // Arrange
        var operationTag = new Domain.Entities.OperationTag() {Id = Guid.NewGuid().ToString(), Name = "", Color = ""};
        var unkownId = Guid.NewGuid();
        await DbContext.OperationTags.AddAsync(operationTag);
        await DbContext.SaveChangesAsync();
        var request = new UpdateOperationTagRequest("tag", "#123456");

        // Act
        var response = await HttpClient.PutAsJsonAsync($"/operationTags/{unkownId}", request);
        var result = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        // Assert
        Check.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
        Check.That(result).IsNotNull();
        Check.That(result?.Message).IsEqualTo("errors.operation_tag.not_found");
    }
    
    [TestCase("", "")]
    [TestCase("name", "")]
    [TestCase("name", "123")]
    [TestCase("", "123")]
    [TestCase("", "#123")] 
    public async Task Put_NotValidCommand_ShouldReturn_ErrorResponse(string name, string color)
    {
        // Arrange
        var operationTag = new Domain.Entities.OperationTag() {Id = Guid.NewGuid().ToString(), Name = "exist", Color = ""};
        await DbContext.OperationTags.AddAsync(operationTag);
        await DbContext.SaveChangesAsync();
        var request = new UpdateOperationTagRequest(name, color);

        // Act
        var response = await HttpClient.PutAsJsonAsync($"/operationTags/{operationTag.Id}", request);
        var result = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        // Assert
        Check.That(result).IsNotNull();
        Check.That(result?.Message).IsNotEmpty();
    }
}