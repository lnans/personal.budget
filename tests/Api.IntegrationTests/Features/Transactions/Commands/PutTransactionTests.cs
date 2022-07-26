using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Application.Features.Transactions.Commands.PutTransaction;
using Domain.Common;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using NFluent;
using NUnit.Framework;

namespace Api.IntegrationTests.Features.Transactions.Commands;

[TestFixture]
public class PutTransactionTests : TestBase
{
    [SetUp]
    public async Task SetupAccount()
    {
        var dbContext = GetDbContext();
        var account = new Account
        {
            Id = Guid.NewGuid().ToString(),
            Name = "ForTest",
            Bank = "bank",
            OwnerId = DefaultUser.Id,
            Balance = 50,
            InitialBalance = 0,
            CreationDate = DateTime.UtcNow,
            Type = AccountType.Expenses,
            Archived = false
        };
        var transaction = new Transaction
        {
            Id = Guid.Parse(TransactionId).ToString(),
            Account = account,
            Amount = -50,
            Description = "test",
            Type = TransactionType.Expense,
            CreatedById = DefaultUser.Id,
            CreationDate = DateTime.UtcNow,
            ExecutionDate = DateTime.UtcNow,
            Tag = new Tag
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Test",
                Color = "#000000",
                OwnerId = DefaultUser.Id
            }
        };
        await dbContext.Accounts.AddAsync(account);
        await dbContext.Transactions.AddAsync(transaction);
        await dbContext.SaveChangesAsync();
    }

    private const string TransactionId = "d32ac4ad-352a-4fb8-8455-04801d365ed2";

    [Test]
    public async Task PutTransaction_Should_UpdateTransactionAndAccount_WithValidRequest()
    {
        // Arrange
        var request = new PutTransactionRequest
        {
            Id = TransactionId, Description = "updated", TagId = null, Amount = 50, CreationDate = DateTime.Now, ExecutionDate = DateTime.Now
        };

        // Act
        var response = await HttpClient.PutAsJsonAsync($"transactions/{TransactionId}", request);
        var transactionInDb = await GetDbContext().Transactions.Include(o => o.Tag).FirstOrDefaultAsync();
        var accountInDb = await GetDbContext().Accounts.FirstOrDefaultAsync();

        // Assert
        Check.That(response.StatusCode).IsEqualTo(HttpStatusCode.NoContent);
        Check.That(transactionInDb).IsNotNull();
        Check.That(accountInDb).IsNotNull();
        Check.That(transactionInDb?.Description).IsEqualTo(request.Description);
        Check.That(transactionInDb?.Tag).IsNull();
        Check.That(transactionInDb?.Amount).IsEqualTo(request.Amount);
        Check.That(accountInDb?.Balance).IsEqualTo(150m);
    }

    [TestCase("", "unknown", "", 0, ExpectedResult = HttpStatusCode.BadRequest)]
    [TestCase("updated", "unknown", "", 0, ExpectedResult = HttpStatusCode.BadRequest)]
    [TestCase("updated", "unknown", "", 10, ExpectedResult = HttpStatusCode.NotFound)]
    [TestCase("updated", TransactionId, "unknown", 10, ExpectedResult = HttpStatusCode.NotFound)]
    public async Task<HttpStatusCode> PutTransaction_ShouldReturn_ErrorResponse_WithWrongRequest(string description, string transactionId, string tagId,
        decimal amount)
    {
        // Arrange
        var request = new PutTransactionRequest
        {
            Id = TransactionId, Description = description, TagId = tagId, Amount = amount, CreationDate = DateTime.Now
        };

        // Act
        var response = await HttpClient.PutAsJsonAsync($"transactions/{transactionId}", request);
        var result = await response.Content.ReadFromJsonOrDefaultAsync<ErrorResponse>();
        var transactionInDb = await GetDbContext().Transactions.Include(o => o.Tag).FirstOrDefaultAsync();
        var accountInDb = await GetDbContext().Accounts.FirstOrDefaultAsync();

        // Assert
        Check.That(result).IsNotNull();
        Check.That(transactionInDb).IsNotNull();
        Check.That(accountInDb).IsNotNull();
        Check.That(transactionInDb?.Description).IsNotEqualTo(request.Description);
        Check.That(transactionInDb?.Tag).IsNotNull();
        Check.That(transactionInDb?.Amount).IsNotEqualTo(request.Amount);
        Check.That(accountInDb?.Balance).IsEqualTo(50m);
        Check.That(result?.Message).IsNotEmpty();

        return response.StatusCode;
    }
}