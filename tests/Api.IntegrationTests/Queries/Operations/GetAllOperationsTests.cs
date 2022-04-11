using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Application.Queries.Operations;
using Domain.Entities;
using Domain.Enums;
using NFluent;
using NUnit.Framework;

namespace Api.IntegrationTests.Queries.Operations;

[TestFixture]
public class GetAllOperationsTests : TestBase
{
    [SetUp]
    public async Task SetupOperations()
    {
        var dbContext = GetDbContext();
        var account1 = new Account
        {
            Id = Guid.Parse(AccountOneId).ToString(),
            Name = "ForTest 1",
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
            OwnerId = DefaultUser.Id,
            Balance = 50,
            InitialBalance = 0,
            CreationDate = DateTime.UtcNow,
            Type = AccountType.Expenses,
            Archived = false
        };
        var tag = new OperationTag
        {
            Id = Guid.Parse(TagId).ToString(),
            Name = "Tag",
            Color = "#000000",
            OwnerId = DefaultUser.Id
        };
        var operations = new List<Operation>();
        
        // account 1 :
        //      - 50 operations
        //      - 16 Expenses, 16 Income, 17 Fixed
        //      - 10 Not executed, 3 different groups of executed date
        Enumerable
            .Range(1, 100)
            .ToList()
            .ForEach(arg => operations.Add(new Operation
            {
                Id = Guid.NewGuid().ToString(),
                Account = arg % 2 == 0 ? account1 : account2,
                Amount = -10,
                Description = $"Op {arg}",
                Tag = arg % 3 == 0 ? tag : null,
                Type = (OperationType) (arg % 3),
                CreationDate = DateTime.UtcNow,
                ExecutionDate = arg % 10 != 0 ? DateTime.UtcNow.AddDays(arg % 3) : null,
                CreatedById = DefaultUser.Id
            }));
        await dbContext.OperationTags.AddAsync(tag);
        await dbContext.Accounts.AddRangeAsync(account1, account2);
        await dbContext.Operations.AddRangeAsync(operations);
        await dbContext.SaveChangesAsync();
    }

    private const string AccountOneId = "d32ac4ad-352a-4fb8-8455-04801d365ed2";
    private const string AccountTwoId = "132ac4ad-352a-4fb8-8455-04801d365ed2";
    private const string TagId = "f32ac4ad-352a-4fb8-8455-04801d365ed2";

    [TestCase(null, null, null, null, 100, 0, ExpectedResult = 100)]
    [TestCase(AccountOneId, null, null, null, 100, 0, ExpectedResult = 50)]
    [TestCase(AccountOneId, null, null, OperationType.Expense, 100, 0, ExpectedResult = 50)]
    public async Task<int> GetAllOperations_ShouldReturn_OperationsGroupedByDays_WithFilters(
        string accountId,
        string description,
        string[] tagsIds,
        OperationType? type,
        int pageSize,
        int skip)
    {
        // Arrange
        var request = new GetAllOperationsRequest(accountId, description, tagsIds, type, pageSize, skip);

        // Act
        var test = request.ToQueryString();
        var response = await HttpClient.GetAsync($"operations?{request.ToQueryString()}");
        var result = await response.Content.ReadFromJsonOrDefaultAsync<GetAllOperationsPaginatedResponse>();

        // Assert
        Check.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);


        return result?.Total ?? 0;
    }
}