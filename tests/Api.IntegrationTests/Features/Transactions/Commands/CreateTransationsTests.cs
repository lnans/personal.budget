using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Application.Features.Transactions.Commands.CreateTransactions;
using Domain.Common;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using NFluent;
using NUnit.Framework;

namespace Api.IntegrationTests.Features.Transactions.Commands;

[TestFixture]
public class CreateTransactionsTests : TestBase
{
    [SetUp]
    public async Task SetupAccount()
    {
        var dbContext = GetDbContext();
        _account = new Account
        {
            Id = Guid.Parse(AccountId).ToString(),
            Name = "ForTest",
            Bank = "bank",
            OwnerId = DefaultUser.Id,
            Balance = 0,
            InitialBalance = 0,
            CreationDate = DateTime.UtcNow,
            Type = AccountType.Expenses,
            Archived = false
        };
        _tag = new Tag
        {
            Id = Guid.Parse(TagId).ToString(),
            Name = "Tag",
            OwnerId = DefaultUser.Id,
            Color = "#000000"
        };
        await dbContext.Accounts.AddAsync(_account);
        await dbContext.Tags.AddAsync(_tag);
        await dbContext.SaveChangesAsync();
    }

    private const string AccountId = "f12ac4ad-352a-4fb8-8455-04801d365ed2";
    private const string TagId = "d32ac4ad-352a-4fb8-8455-04801d365ed2";
    private Account _account = null!;
    private Tag _tag = null!;

    [Test]
    public async Task CreateTransaction_Should_CreateTransactionAndUpdateAccount_WithValidRequest()
    {
        // Arrange
        var request = new CreateTransactionsRequest
        {
            AccountId = _account.Id,
            Transactions = new[]
            {
                new CreateTransactionDto
                {
                    Description = "desc",
                    TagId = _tag.Id,
                    Type = TransactionType.Expense,
                    Amount = -50,
                    CreationDate = DateTime.UtcNow,
                    ExecutionDate = DateTime.UtcNow
                }
            }
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync("transactions", request);
        var transactionInDb = await GetDbContext().Transactions
            .Include(o => o.Account)
            .Include(o => o.Tag)
            .FirstOrDefaultAsync();
        var accountInDb = await GetDbContext().Accounts.FirstOrDefaultAsync();

        // Assert
        Check.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        Check.That(transactionInDb).IsNotNull();
        Check.That(transactionInDb?.Description).IsEqualTo(request.Transactions[0].Description);
        Check.That(transactionInDb?.Account?.Id).IsEqualTo(request.AccountId);
        Check.That(transactionInDb?.Type).IsEqualTo(request.Transactions[0].Type);
        Check.That(transactionInDb?.Amount).IsEqualTo(-50);
        Check.That(transactionInDb?.Tag?.Name).IsEqualTo(_tag.Name);
        Check.That(transactionInDb?.Tag?.Color).IsEqualTo(_tag.Color);
        Check.That(transactionInDb?.CreationDate).IsEqualTo(request.Transactions[0].CreationDate);
        Check.That(transactionInDb?.ExecutionDate).IsEqualTo(request.Transactions[0].ExecutionDate);
        Check.That(accountInDb?.Balance).IsEqualTo(-50);
    }

    [TestCase("", "", "", 0, ExpectedResult = HttpStatusCode.BadRequest)]
    [TestCase("desc", "", "", 0, ExpectedResult = HttpStatusCode.BadRequest)]
    [TestCase("desc", AccountId, TagId, 0, ExpectedResult = HttpStatusCode.BadRequest)]
    [TestCase("desc", "none", "", -10, ExpectedResult = HttpStatusCode.NotFound)]
    [TestCase("desc", AccountId, "none", -10, ExpectedResult = HttpStatusCode.NotFound)]
    public async Task<HttpStatusCode> CreateTransaction_ShouldReturn_ErrorResponse_WithWrongRequest(string description, string accountId, string tagId,
        decimal amount)
    {
        // Arrange
        var request = new CreateTransactionsRequest
        {
            AccountId = accountId, Transactions = new[]
            {
                new CreateTransactionDto
                {
                    Description = description,
                    TagId = tagId,
                    Type = TransactionType.Expense,
                    Amount = amount,
                    CreationDate = DateTime.UtcNow,
                    ExecutionDate = DateTime.UtcNow
                }
            }
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync("transactions", request);
        var result = await response.Content.ReadFromJsonOrDefaultAsync<ErrorResponse>();
        var transactionInDb = await GetDbContext().Transactions.FirstOrDefaultAsync();
        var accountInDb = await GetDbContext().Accounts.FirstOrDefaultAsync();

        // Assert
        Check.That(result).IsNotNull();
        Check.That(result?.Message).IsNotEmpty();
        Check.That(transactionInDb).IsNull();
        Check.That(accountInDb?.Balance).IsEqualTo(0);

        return response.StatusCode;
    }
}