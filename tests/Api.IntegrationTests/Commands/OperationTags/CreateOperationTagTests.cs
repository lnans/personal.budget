using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Application.Commands.OperationTags;
using Domain.Common;
using NFluent;
using NUnit.Framework;

namespace Api.IntegrationTests.Commands.OperationTags;

[TestFixture]
public class CreateOperationTagTests : TestBase
{
    [Test]
    public async Task CreateOperationTag_WithValidRequest_Should_CreateOperationTag()
    {
        // Arrange
        var request = new CreateOperationTagRequest("name", "#123456");

        // Act
        var response = await HttpClient.PostAsJsonAsync("operationTags", request);
        var result = await response.Content.ReadFromJsonOrDefaultAsync<CreateOperationTagResponse>();
        var operationTagInDb = DbContext.OperationTags.FirstOrDefault();

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
    public async Task CreateOperationTag_WithWrongRequest_ShouldReturn_ErrorResponse(string name, string color)
    {
        // Arrange
        var request = new CreateOperationTagRequest(name, color);

        // Act
        var response = await HttpClient.PostAsJsonAsync("operationTags", request);
        var result = await response.Content.ReadFromJsonOrDefaultAsync<ErrorResponse>();
        var operationTagInDb = DbContext.OperationTags.FirstOrDefault();

        // Assert
        Check.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
        Check.That(operationTagInDb).IsNull();
        Check.That(result).IsNotNull();
        Check.That(result?.Message).IsNotEmpty();
    }
}