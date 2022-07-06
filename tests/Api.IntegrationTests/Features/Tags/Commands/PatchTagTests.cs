using System;
using System.Net;
using System.Threading.Tasks;
using Application.Features.Tags.Commands.PatchTag;
using Domain.Common;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using NFluent;
using NUnit.Framework;

namespace Api.IntegrationTests.Features.Tags.Commands;

[TestFixture]
public class PatchTagTests : TestBase
{
    [Test]
    public async Task PatchTag_WithValidRequest_Should_UpdateTag()
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
        var request = new PatchTagRequest
        {
            Name = "updated", Color = "#000000"
        };

        // Act
        var response = await HttpClient.PatchAsJsonAsync($"tags/{tag.Id}", request);
        var tagInDb = await GetDbContext().Tags.FirstOrDefaultAsync();

        // Assert
        Check.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        Check.That(tagInDb).IsNotNull();
        Check.That(tagInDb?.Name).IsEqualTo(request.Name);
        Check.That(tagInDb?.Color).IsEqualTo(request.Color);
    }

    [TestCase("", "")]
    [TestCase("string", "")]
    [TestCase("", "#123456")]
    [TestCase("string", "456789564")]
    public async Task PatchTag_WithWrongRequest_ShouldReturn_ErrorResponse(string name, string color)
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
        var request = new PatchTagRequest
        {
            Name = name, Color = color
        };

        // Act
        var response = await HttpClient.PatchAsJsonAsync($"tags/{tag.Id}", request);
        var result = await response.Content.ReadFromJsonOrDefaultAsync<ErrorResponse>();
        var tagInDb = await GetDbContext().Tags.FirstOrDefaultAsync();

        // Assert
        Check.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
        Check.That(tagInDb).IsNotNull();
        Check.That(tagInDb?.Name).IsEqualTo(tag.Name);
        Check.That(tagInDb?.Color).IsEqualTo(tag.Color);
        Check.That(result).IsNotNull();
        Check.That(result?.Message).IsNotEmpty();
    }
}