using System;
using System.Net;
using System.Threading.Tasks;
using Domain.Common;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using NFluent;
using NUnit.Framework;

namespace Api.IntegrationTests.Features.Transactions.Commands;

[TestFixture]
public class DeleteTransactionTests : TestBase
{
    [SetUp]
    public async Task SetupAccount()
    {
        var dbContext = GetDbContext();
        _account = new Account
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
        _transaction = new Transaction
        {
            Id = Guid.NewGuid().ToString(),
            Account = _account,
            Amount = -50,
            Description = "test",
            Type = TransactionType.Expense,
            CreatedById = DefaultUser.Id,
            CreationDate = DateTime.UtcNow,
            ExecutionDate = DateTime.UtcNow
        };
        await dbContext.Accounts.AddAsync(_account);
        await dbContext.Transactions.AddAsync(_transaction);
        await dbContext.SaveChangesAsync();
    }

    private Account _account = null!;
    private Transaction _transaction = null!;

    [Test]
    public async Task DeleteTransaction_Should_DeleteTransaction_WithKnownId()
    {
        // Act
        var response = await HttpClient.DeleteAsync($"Transactions/{_transaction.Id}");
        var transactionInDb = await GetDbContext().Transactions.FirstOrDefaultAsync();
        var accountInDb = await GetDbContext().Accounts.FirstOrDefaultAsync();

        // Assert
        Check.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        Check.That(transactionInDb).IsNull();
        Check.That(accountInDb?.Balance).IsEqualTo(100);
    }

    [Test]
    public async Task DeleteTransaction_ShouldReturn_ErrorResponse_WithUnknownId()
    {
        // Act
        var response = await HttpClient.DeleteAsync("Transactions/unknown");
        var result = await response.Content.ReadFromJsonOrDefaultAsync<ErrorResponse>();
        var transactionInDb = await GetDbContext().Transactions.FirstOrDefaultAsync();
        var accountInDb = await GetDbContext().Accounts.FirstOrDefaultAsync();

        // Assert
        Check.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
        Check.That(transactionInDb).IsNotNull();
        Check.That(accountInDb?.Balance).IsEqualTo(50);
        Check.That(result).IsNotNull();
        Check.That(result?.Message).IsNotEmpty();
    }
}