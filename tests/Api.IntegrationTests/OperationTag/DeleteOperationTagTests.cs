using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Domain.Common;
using Microsoft.EntityFrameworkCore;
using NFluent;
using NUnit.Framework;

namespace Api.IntegrationTests.OperationTag;

public class DeleteOperationTagTests : TestBase
{
    [Test]
    public async Task Delete_ExistingTag_ShouldReturn_Ok()
    {
        // Arrange
        var operationTag = new Domain.Entities.OperationTag {Name = "test"};
        await DbContext.OperationTags.AddAsync(operationTag);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await HttpClient.DeleteAsync($"/OperationTags/{operationTag.Id}");

        // Assert
        var operationTagsCount = await DbContext.OperationTags.CountAsync();
        Check.That(response).IsNotNull();
        Check.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        Check.That(operationTagsCount).IsEqualTo(0);
    }
    [Test]
    public async Task Delete_UnknownTag_ShouldReturn_NotFound()
    {
        // Act
        var response = await HttpClient.DeleteAsync("/OperationTags/1");
        var result = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        // Assert
        Check.That(result).IsNotNull();
        Check.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
        Check.That(result?.Message).IsEqualTo("errors.operation_tag.not_found");
    }
}