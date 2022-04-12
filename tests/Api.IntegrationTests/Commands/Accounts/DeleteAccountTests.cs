using System;
using System.Net;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using NFluent;
using NUnit.Framework;

namespace Api.IntegrationTests.Commands.Accounts;

[TestFixture]
public class DeleteAccountTests : TestBase
{
    [Test]
    public async Task DeleteAccount_Should_DeleteAccount_WithExistingAccount()
    {
        // Arrange
        var dbContext = GetDbContext();
        var account = new Account
        {
            Id = Guid.NewGuid().ToString(),
            Name = "test",
            Balance = 0,
            InitialBalance = 0,
            Icon = "",
            OwnerId = DefaultUser.Id,
            Type = AccountType.Expenses,
            CreationDate = DateTime.UtcNow,
            Archived = false
        };
        await dbContext.Accounts.AddAsync(account);
        await dbContext.SaveChangesAsync();

        // Act
        var response = await HttpClient.DeleteAsync($"Accounts/{account.Id}");
        var accountInDb = await GetDbContext().Accounts.FirstOrDefaultAsync(a => a.Id == account.Id);

        // Assert
        Check.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        Check.That(accountInDb).IsNull();
    }

    [Test]
    public async Task DeleteAccount_ShouldReturn_ErrorResponse_WithUnknownAccount()
    {
        // Arrange
        var dbContext = GetDbContext();
        var account = new Account
        {
            Id = Guid.NewGuid().ToString(),
            Name = "test",
            Balance = 0,
            InitialBalance = 0,
            Icon = "",
            OwnerId = DefaultUser.Id,
            Type = AccountType.Expenses,
            CreationDate = DateTime.UtcNow,
            Archived = false
        };
        await dbContext.Accounts.AddAsync(account);
        await dbContext.SaveChangesAsync();

        // Act
        var response = await HttpClient.DeleteAsync("Accounts/1");
        var accountInDb = await GetDbContext().Accounts.FirstOrDefaultAsync(a => a.Id == account.Id);

        // Assert
        Check.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
        Check.That(accountInDb).IsNotNull();
    }
}