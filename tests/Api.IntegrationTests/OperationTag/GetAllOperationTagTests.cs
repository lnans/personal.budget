using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Application.Queries.OperationTag;
using NFluent;
using NUnit.Framework;

namespace Api.IntegrationTests.OperationTag;

public class GetAllOperationTagTests : TestBase
{
    [Test]
    public async Task Get_ShouldReturn_OperationTags()
    {
        // Arrange
        for (var i = 0; i < 10; i++)
            await DbContext.OperationTags.AddAsync(new Domain.Entities.OperationTag {Id = Guid.NewGuid().ToString()});
        await DbContext.SaveChangesAsync();

        // Act
        var response = await HttpClient.GetAsync("/OperationTags");
        var result = await response.Content.ReadFromJsonAsync<IEnumerable<GetAllOperationTagResponse>>();

        // Assert
        Check.That(result).IsNotNull();
        Check.That(result).WhoseSize().IsEqualTo(10);
    }
}