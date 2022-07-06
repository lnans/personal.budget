using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Application.Features.Transactions.Queries.GetPaginatedTransactions;
using Domain.Common;
using Domain.Entities;
using Domain.Enums;
using NFluent;
using NUnit.Framework;

namespace Api.IntegrationTests.Features.Transactions.Queries;

[TestFixture]
public class GetAllTransactionsTests : TestBase
{
    [SetUp]
    public async Task SetupTransactions()
    {
        var dbContext = GetDbContext();
        var account1 = new Account
        {
            Id = Guid.Parse(AccountOneId).ToString(),
            Name = "ForTest 1",
            Bank = "bank",
            OwnerId = DefaultUser.Id,
            Balance = 50,
            InitialBalance = 0,
            CreationDate = DateTime.UtcNow,
            Type = AccountType.Expenses,
            Archived = false
        };
        var account2 = new Account
        {
            Id = Guid.Parse(AccountTwoId).ToString(),
            Name = "ForTest 2",
            Bank = "bank",
            OwnerId = DefaultUser.Id,
            Balance = 50,
            InitialBalance = 0,
            CreationDate = DateTime.UtcNow,
            Type = AccountType.Expenses,
            Archived = false
        };
        var tag = new Tag
        {
            Id = Guid.Parse(TagId).ToString(),
            Name = "Tag",
            Color = "#000000",
            OwnerId = DefaultUser.Id
        };
        var transactions = new List<Transaction>();
        Enumerable
            .Range(1, 100)
            .ToList()
            .ForEach(arg => transactions.Add(new Transaction
            {
                Id = Guid.NewGuid().ToString(),
                Account = arg % 2 == 0 ? account1 : account2,
                Amount = -10,
                Description = $"Op {arg}",
                Tag = arg % 3 == 0 ? tag : null,
                Type = (TransactionType) (arg % 3),
                CreationDate = DateTime.UtcNow,
                ExecutionDate = arg % 10 != 0 ? DateTime.UtcNow.AddDays(arg % 3) : null,
                CreatedById = DefaultUser.Id
            }));
        await dbContext.Tags.AddAsync(tag);
        await dbContext.Accounts.AddRangeAsync(account1, account2);
        await dbContext.Transactions.AddRangeAsync(transactions);
        await dbContext.SaveChangesAsync();
    }

    private const string AccountOneId = "d32ac4ad-352a-4fb8-8455-04801d365ed2";
    private const string AccountTwoId = "132ac4ad-352a-4fb8-8455-04801d365ed2";
    private const string TagId = "f32ac4ad-352a-4fb8-8455-04801d365ed2";

    /// <summary>
    ///     account 1 :
    ///     - desc {N} with N is even
    ///     - 50 transactions
    ///     - 16 Expenses, 17 Income, 17 Fixed
    ///     - groups: 1 Not executed (10 op) + 3 different groups of executed date
    /// </summary>
    /// <param name="accountId"></param>
    /// <param name="description"></param>
    /// <param name="tagsIds"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    [TestCase(null, null, null, null, ExpectedResult = 100)]
    [TestCase(null, null, new[] {TagId}, null, ExpectedResult = 33)]
    [TestCase(AccountOneId, null, null, null, ExpectedResult = 50)]
    [TestCase(AccountOneId, "1", null, null, ExpectedResult = 6)]
    [TestCase(AccountOneId, null, null, TransactionType.Expense, ExpectedResult = 16)]
    [TestCase(AccountOneId, null, null, TransactionType.Income, ExpectedResult = 17)]
    [TestCase(AccountOneId, null, null, TransactionType.Fixed, ExpectedResult = 17)]
    public async Task<int> GetAllTransactions_ShouldReturn_TransactionsGroupedByDays_WithFilters(
        string accountId,
        string description,
        string[] tagsIds,
        TransactionType? type)
    {
        // Arrange
        var request = new GetPaginatedTransactionsRequest
        {
            AccountId = accountId, Description = description, TagIds = tagsIds, Type = type, Cursor = 0, PageSize = 100
        };

        // Act
        var response = await HttpClient.GetAsync($"transactions?{request.ToQueryString()}");
        var result = await response.Content.ReadFromJsonOrDefaultAsync<InfiniteData<TransactionDto>>();

        // Assert
        Check.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        Check.That(result).IsNotNull();

        return result?.Data.Count() ?? 0;
    }
}