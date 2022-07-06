using System;
using System.Net;
using System.Threading.Tasks;
using Application.Features.Accounts.Commands.PatchAccount;
using Domain.Common;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using NFluent;
using NUnit.Framework;

namespace Api.IntegrationTests.Features.Accounts.Commands;

[TestFixture]
public class PatchAccountTests : TestBase
{
    [Test]
    public async Task PatchAccount_Should_UpdateAccount_WithValidRequest()
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
        var request = new PatchAccountRequest
        {
            Id = account.Id, Bank = "updated", Name = "updated", Icon = "updatedIcon"
        };

        // Act
        var response = await HttpClient.PatchAsJsonAsync($"accounts/{account.Id}", request);
        var accountInDb = await GetDbContext().Accounts.FirstOrDefaultAsync(a => a.Id == account.Id);

        // Assert
        Check.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        Check.That(accountInDb).IsNotNull();
        Check.That(accountInDb?.Name).IsEqualTo(request.Name);
        Check.That(accountInDb?.Bank).IsEqualTo(request.Bank);
        Check.That(accountInDb?.Icon).IsEqualTo(request.Icon);
    }

    [TestCase("", "")]
    [TestCase(null, null)]
    [TestCase("name", null)]
    [TestCase(null, "bank")]
    public async Task PatchAccount_ShouldReturn_ErrorResponse_WithWrongRequest(string name, string bank)
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
        var request = new PatchAccountRequest
        {
            Id = account.Id, Name = name, Bank = bank, Icon = "updatedIcon"
        };

        // Act
        var response = await HttpClient.PatchAsJsonAsync($"accounts/{account.Id}", request);
        var result = await response.Content.ReadFromJsonOrDefaultAsync<ErrorResponse>();
        var accountInDb = await GetDbContext().Accounts.FirstOrDefaultAsync(a => a.Id == account.Id);

        // Assert
        Check.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
        Check.That(accountInDb).IsNotNull();
        Check.That(accountInDb?.Name).IsEqualTo(account.Name);
        Check.That(accountInDb?.Bank).IsEqualTo(account.Bank);
        Check.That(accountInDb?.Icon).IsEqualTo(account.Icon);
        Check.That(result).IsNotNull();
        Check.That(result?.Message).IsNotEmpty();
    }

    [Test]
    public async Task PatchAccount_ShouldReturn_ErrorResponse_WithUnknownAccount()
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
        var request = new PatchAccountRequest
        {
            Id = "1", Name = "updated", Bank = "updated", Icon = "updatedIcon"
        };

        // Act
        var response = await HttpClient.PatchAsJsonAsync("Accounts/1", request);
        var result = await response.Content.ReadFromJsonOrDefaultAsync<ErrorResponse>();
        var accountInDb = await GetDbContext().Accounts.FirstOrDefaultAsync(a => a.Id == account.Id);

        // Assert
        Check.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
        Check.That(accountInDb).IsNotNull();
        Check.That(accountInDb?.Name).IsEqualTo(account.Name);
        Check.That(accountInDb?.Bank).IsEqualTo(account.Bank);
        Check.That(accountInDb?.Icon).IsEqualTo(account.Icon);
        Check.That(result).IsNotNull();
        Check.That(result?.Message).IsNotEmpty();
    }
}