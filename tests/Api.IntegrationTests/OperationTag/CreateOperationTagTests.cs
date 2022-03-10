using System.Net.Http.Json;
using System.Threading.Tasks;
using Application.Commands.OperationTag;
using Domain.Common;
using NFluent;
using NUnit.Framework;

namespace Api.IntegrationTests.OperationTag;

public class CreateOperationTagTests : TestBase
{
    [Test]
    public async Task Post_WithValidCommand_ShouldReturn_OperationTag_Created()
    {
        // Arrange
        var command = new CreateOperationTagCommand("tag", "#123456");

        // Act
        var response = await HttpClient.PostAsJsonAsync("/operationTags", command);
        var result = await response.Content.ReadFromJsonAsync<CreateOperationTagResponse>();

        // Assert
        Check.That(result).IsNotNull();
        Check.That(result?.Name).IsEqualTo("tag");
        Check.That(result?.Color).IsEqualTo("#123456");
    }
    
    [TestCase("", "")]
    [TestCase("name", "")]
    [TestCase("name", "123")]
    [TestCase("", "123")]
    [TestCase("", "#123")]
    public async Task Post_NotValidCommand_ShouldReturn_ErrorResponse(string name, string color)
    {
        // Arrange
        var command = new CreateOperationTagCommand(name, color);

        // Act
        var response = await HttpClient.PostAsJsonAsync("/operationTags", command);
        var result = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        // Assert
        Check.That(result).IsNotNull();
        Check.That(result?.Message).IsNotEmpty();
    }
}