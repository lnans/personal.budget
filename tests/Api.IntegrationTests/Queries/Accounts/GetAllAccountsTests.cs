using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Application.Queries.Accounts;
using Domain.Entities;
using Domain.Enums;
using NFluent;
using NUnit.Framework;

namespace Api.IntegrationTests.Queries.Accounts;

[TestFixture]
public class GetAllAccountsTests : TestBase
{
    [TestCase(null, false, ExpectedResult = 10)]
    [TestCase(null, true, ExpectedResult = 10)]
    [TestCase("string", true, ExpectedResult = 10)]
    [TestCase("string", false, ExpectedResult = 10)]
    [TestCase("1", true, ExpectedResult = 5)]
    [TestCase("1", false, ExpectedResult = 6)]
    [TestCase("none", true, ExpectedResult = 0)]
    [TestCase("none", false, ExpectedResult = 0)]
    public async Task<int> GetAllAccounts_ShouldReturn_AccountsList(string name, bool archived)
    {
        // Arrange
        var dbContext = GetDbContext();
        var accounts = new List<Account>();
        Enumerable
            .Range(0, 20)
            .ToList()
            .ForEach(arg => accounts.Add(new Account()
            {
                Id = Guid.NewGuid().ToString(),
                Name = $"string{arg}",
                Balance = 0,
                InitialBalance = 0,
                Icon = "",
                OwnerId = DefaultUser.Id,
                Type = AccountType.Expenses,
                CreationDate = DateTime.UtcNow,
                Archived = arg % 2 == 0
            }));
        await dbContext.Accounts.AddRangeAsync(accounts);
        await dbContext.SaveChangesAsync();

        // Act
        var response = await HttpClient.GetAsync($"Accounts?name={name}&archived={archived}");
        var result = await response.Content.ReadFromJsonOrDefaultAsync<IEnumerable<GetAllAccountsResponse>>();

        // Assert
        Check.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

        return result?.Count() ?? 0;
    }
}