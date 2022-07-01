using System;
using System.Net;
using System.Threading.Tasks;
using Application.Features.Accounts.Commands.ArchivedAccount;
using Domain.Common;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using NFluent;
using NUnit.Framework;

namespace Api.IntegrationTests.Commands.Accounts;

[TestFixture]
public class ArchivedAccountTests : TestBase
{
    [TestCase(true)]
    [TestCase(false)]
    public async Task ArchiveAccount_Should_ChangeAccountState_WithExistingAccount(bool archived)
    {
        // Arrange
        var dbContext = GetDbContext();
        var account = new Account
        {
            Id = Guid.NewGuid().ToString(),
            Name = "test",
            Bank = "bank",
            Balance = 0,
            InitialBalance = 0,
            Icon = "",
            OwnerId = DefaultUser.Id,
            Type = AccountType.Expenses,
            CreationDate = DateTime.UtcNow,
            Archived = !archived
        };
        await dbContext.Accounts.AddAsync(account);
        await dbContext.SaveChangesAsync();
        var request = new ArchivedAccountRequest
        {
            Id = account.Id,
            Archived = archived
        };

        // Act
        var response = await HttpClient.PatchAsJsonAsync($"accounts/{account.Id}/archive", request);
        var accountInDb = await GetDbContext().Accounts.FirstOrDefaultAsync(a => a.Id == account.Id);

        // Assert
        Check.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        Check.That(accountInDb).IsNotNull();
        Check.That(accountInDb?.Archived).IsEqualTo(archived);
    }

    [Test]
    public async Task ArchiveAccount_ShouldReturn_ErrorResponse_WithUnknownAccount()
    {
        // Arrange
        var dbContext = GetDbContext();
        var account = new Account
        {
            Id = Guid.NewGuid().ToString(),
            Name = "test",
            Bank = "bank",
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
        var request = new ArchivedAccountRequest();

        // Act
        var response = await HttpClient.PatchAsJsonAsync("accounts/1/archive", request);
        var result = await response.Content.ReadFromJsonOrDefaultAsync<ErrorResponse>();
        var accountInDb = await GetDbContext().Accounts.FirstOrDefaultAsync(a => a.Id == account.Id);

        // Assert
        Check.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
        Check.That(accountInDb).IsNotNull();
        Check.That(accountInDb?.Archived).IsEqualTo(false);
        Check.That(result).IsNotNull();
        Check.That(result?.Message).IsNotEmpty();
    }
}