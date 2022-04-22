using System;
using System.Net;
using System.Threading.Tasks;
using Domain.Common;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using NFluent;
using NUnit.Framework;

namespace Api.IntegrationTests.Commands.Operations;

[TestFixture]
public class DeleteOperationTests : TestBase
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
        _operation = new Operation
        {
            Id = Guid.NewGuid().ToString(),
            Account = _account,
            Amount = -50,
            Description = "test",
            Type = OperationType.Expense,
            CreatedById = DefaultUser.Id,
            CreationDate = DateTime.UtcNow,
            ExecutionDate = DateTime.UtcNow
        };
        await dbContext.Accounts.AddAsync(_account);
        await dbContext.Operations.AddAsync(_operation);
        await dbContext.SaveChangesAsync();
    }

    private Account _account = null!;
    private Operation _operation = null!;

    [Test]
    public async Task DeleteOperation_Should_DeleteOperation_WithKnownId()
    {
        // Act
        var response = await HttpClient.DeleteAsync($"Operations/{_operation.Id}");
        var operationInDb = await GetDbContext().Operations.FirstOrDefaultAsync();
        var accountInDb = await GetDbContext().Accounts.FirstOrDefaultAsync();

        // Assert
        Check.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        Check.That(operationInDb).IsNull();
        Check.That(accountInDb?.Balance).IsEqualTo(100);
    }

    [Test]
    public async Task DeleteOperation_ShouldReturn_ErrorResponse_WithUnknownId()
    {
        // Act
        var response = await HttpClient.DeleteAsync("Operations/unknown");
        var result = await response.Content.ReadFromJsonOrDefaultAsync<ErrorResponse>();
        var operationInDb = await GetDbContext().Operations.FirstOrDefaultAsync();
        var accountInDb = await GetDbContext().Accounts.FirstOrDefaultAsync();

        // Assert
        Check.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
        Check.That(operationInDb).IsNotNull();
        Check.That(accountInDb?.Balance).IsEqualTo(50);
        Check.That(result).IsNotNull();
        Check.That(result?.Message).IsNotEmpty();
    }
}