using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Application.Features.Tags.Queries.GetAllTags;
using Domain.Entities;
using NFluent;
using NUnit.Framework;

namespace Api.IntegrationTests.Features.Tags.Queries;

[TestFixture]
public class GetAllTagsTests : TestBase
{
    [TestCase(null, ExpectedResult = 20)]
    [TestCase("string", ExpectedResult = 20)]
    [TestCase("string10", ExpectedResult = 1)]
    [TestCase("1", ExpectedResult = 11)]
    [TestCase("none", ExpectedResult = 0)]
    public async Task<int> GetAllTags_ShouldReturn_TagsList(string name)
    {
        // Arrange
        var dbContext = GetDbContext();
        var tags = new List<Tag>();
        Enumerable
            .Range(0, 20)
            .ToList()
            .ForEach(arg => tags.Add(new Tag
                {Id = Guid.NewGuid().ToString(), Color = "#000000", OwnerId = DefaultUser.Id, Name = $"string{arg}"}));
        await dbContext.Tags.AddRangeAsync(tags);
        await dbContext.SaveChangesAsync();

        // Act
        var response = await HttpClient.GetAsync($"Tags?name={name}");
        var result = await response.Content.ReadFromJsonOrDefaultAsync<IEnumerable<TagDto>>();

        // Assert
        Check.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

        return result?.Count() ?? 0;
    }
}