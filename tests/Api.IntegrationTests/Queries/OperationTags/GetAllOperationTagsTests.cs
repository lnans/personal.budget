using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Application.Queries.OperationTags;
using Domain.Entities;
using NFluent;
using NUnit.Framework;

namespace Api.IntegrationTests.Queries.OperationTags;

[TestFixture]
public class GetAllOperationTagsTests : TestBase
{
    [TestCase(null, ExpectedResult = 20)]
    [TestCase("string", ExpectedResult = 20)]
    [TestCase("string10", ExpectedResult = 1)]
    [TestCase("1", ExpectedResult = 11)]
    [TestCase("none", ExpectedResult = 0)]
    public async Task<int> GetAllOperationTags_ShouldReturn_OperationTagsList(string name)
    {
        // Arrange
        var dbContext = GetDbContext();
        var operationTags = new List<OperationTag>();
        Enumerable
            .Range(0, 20)
            .ToList()
            .ForEach(arg => operationTags.Add(new OperationTag
                {Id = Guid.NewGuid().ToString(), Color = "#000000", OwnerId = DefaultUser.Id, Name = $"string{arg}"}));
        await dbContext.OperationTags.AddRangeAsync(operationTags);
        await dbContext.SaveChangesAsync();
        
        // Act
        var response = await HttpClient.GetAsync($"OperationTags?name={name}");
        var result = await response.Content.ReadFromJsonOrDefaultAsync<IEnumerable<GetAllOperationTagsResponse>>();
        
        // Assert
        Check.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

        return result?.Count() ?? 0;
    }
}