using System;
using System.Net;
using System.Threading.Tasks;
using Application.Commands.Accounts;
using Domain.Common;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using NFluent;
using NUnit.Framework;

namespace Api.IntegrationTests.Commands.Accounts;

[TestFixture]
public class UpdateAccountTests : TestBase
{
    [Test]
    public async Task UpdateAccount_Should_UpdateAccount_WithValidRequest()
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
        var request = new UpdateAccountRequest("updated", "updatedIcon");

        // Act
        var response = await HttpClient.PatchAsJsonAsync($"accounts/{account.Id}", request);
        var result = await response.Content.ReadFromJsonOrDefaultAsync<UpdateAccountResponse>();
        var accountInDb = await GetDbContext().Accounts.FirstOrDefaultAsync(a => a.Id == account.Id);

        // Assert
        Check.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        Check.That(accountInDb).IsNotNull();
        Check.That(accountInDb?.Name).IsEqualTo(request.Name);
        Check.That(accountInDb?.Icon).IsEqualTo(request.Icon);
        Check.That(result).IsNotNull();
        Check.That(result?.Name).IsEqualTo(request.Name);
        Check.That(result?.Icon).IsEqualTo(request.Icon);
    }

    [TestCase("")]
    [TestCase(null)]
    public async Task UpdateAccount_ShouldReturn_ErrorResponse_WithWrongRequest(string name)
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
        var request = new UpdateAccountRequest(name, "updatedIcon");

        // Act
        var response = await HttpClient.PatchAsJsonAsync($"accounts/{account.Id}", request);
        var result = await response.Content.ReadFromJsonOrDefaultAsync<ErrorResponse>();
        var accountInDb = await GetDbContext().Accounts.FirstOrDefaultAsync(a => a.Id == account.Id);

        // Assert
        Check.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
        Check.That(accountInDb).IsNotNull();
        Check.That(accountInDb?.Name).IsEqualTo(account.Name);
        Check.That(accountInDb?.Icon).IsEqualTo(account.Icon);
        Check.That(result).IsNotNull();
        Check.That(result?.Message).IsNotEmpty();
    }

    [Test]
    public async Task UpdateAccount_ShouldReturn_ErrorResponse_WithUnknownAccount()
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
        var request = new UpdateAccountRequest("updated", "updatedIcon");

        // Act
        var response = await HttpClient.PatchAsJsonAsync("Accounts/1", request);
        var result = await response.Content.ReadFromJsonOrDefaultAsync<ErrorResponse>();
        var accountInDb = await GetDbContext().Accounts.FirstOrDefaultAsync(a => a.Id == account.Id);

        // Assert
        Check.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
        Check.That(accountInDb).IsNotNull();
        Check.That(accountInDb?.Name).IsEqualTo(account.Name);
        Check.That(accountInDb?.Icon).IsEqualTo(account.Icon);
        Check.That(result).IsNotNull();
        Check.That(result?.Message).IsNotEmpty();
    }
}