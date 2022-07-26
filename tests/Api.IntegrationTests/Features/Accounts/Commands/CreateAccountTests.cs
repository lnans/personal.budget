using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Application.Features.Accounts.Commands.CreateAccount;
using Domain.Common;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using NFluent;
using NUnit.Framework;

namespace Api.IntegrationTests.Features.Accounts.Commands;

[TestFixture]
public class CreateAccountTests : TestBase
{
    [Test]
    public async Task CreateAccount_WithValidRequest_Should_CreateAccount()
    {
        // Arrange
        var request = new CreateAccountRequest
        {
            Name = "Account", Bank = "Bank", Icon = "Icon", Type = AccountType.Expenses, InitialBalance = 0
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync("accounts", request);
        var accountInDb = await GetDbContext().Accounts.FirstOrDefaultAsync();

        // Assert
        Check.That(response.StatusCode).IsEqualTo(HttpStatusCode.NoContent);
        Check.That(accountInDb).IsNotNull();
        Check.That(accountInDb?.Name).IsEqualTo(request.Name);
        Check.That(accountInDb?.Bank).IsEqualTo(request.Bank);
        Check.That(accountInDb?.Icon).IsEqualTo(request.Icon);
        Check.That(accountInDb?.Type).IsEqualTo(request.Type);
        Check.That(accountInDb?.InitialBalance).IsEqualTo(request.InitialBalance);
        Check.That(accountInDb?.Balance).IsEqualTo(request.InitialBalance);
        Check.That(accountInDb?.OwnerId).IsEqualTo(DefaultUser.Id);
    }

    [TestCase("", "")]
    [TestCase(null, null)]
    [TestCase("name", null)]
    [TestCase(null, "bank")]
    public async Task CreateAccount_WithWrongRequest_ShouldReturn_ErrorResponse(string name, string bank)
    {
        // Arrange
        var request = new CreateAccountRequest
        {
            Name = name, Bank = bank, Icon = "Icon", Type = AccountType.Expenses, InitialBalance = 0
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync("accounts", request);
        var result = await response.Content.ReadFromJsonOrDefaultAsync<ErrorResponse>();
        var accountInDb = await GetDbContext().Accounts.FirstOrDefaultAsync();

        // Assert
        Check.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
        Check.That(accountInDb).IsNull();
        Check.That(result).IsNotNull();
        Check.That(result?.Message).IsNotEmpty();
    }
}