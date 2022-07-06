using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Application.Features.Tags.Commands.CreateTag;
using Domain.Common;
using NFluent;
using NUnit.Framework;

namespace Api.IntegrationTests.Features.Tags.Commands;

[TestFixture]
public class CreateTagTests : TestBase
{
    [Test]
    public async Task CreateTag_WithValidRequest_Should_CreateTag()
    {
        // Arrange
        var request = new CreateTagRequest
        {
            Name = "name", Color = "#123456"
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync("tags", request);
        var tagInDb = GetDbContext().Tags.FirstOrDefault();

        // Assert
        Check.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        Check.That(tagInDb).IsNotNull();
        Check.That(tagInDb?.Name).IsEqualTo(request.Name);
        Check.That(tagInDb?.Color).IsEqualTo(request.Color);
        Check.That(tagInDb?.OwnerId).IsEqualTo(DefaultUser.Id);
    }

    [TestCase("", "")]
    [TestCase("string", "")]
    [TestCase("", "#123456")]
    [TestCase("string", "456789564")]
    public async Task CreateTag_WithWrongRequest_ShouldReturn_ErrorResponse(string name, string color)
    {
        // Arrange
        var request = new CreateTagRequest
        {
            Name = name, Color = color
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync("tags", request);
        var result = await response.Content.ReadFromJsonOrDefaultAsync<ErrorResponse>();
        var tagInDb = GetDbContext().Tags.FirstOrDefault();

        // Assert
        Check.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
        Check.That(tagInDb).IsNull();
        Check.That(result).IsNotNull();
        Check.That(result?.Message).IsNotEmpty();
    }
}