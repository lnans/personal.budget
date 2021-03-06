using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Application.Commands.Accounts;
using Domain.Common;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using NFluent;
using NUnit.Framework;

namespace Api.IntegrationTests.Commands.Accounts;

[TestFixture]
public class CreateAccountTests : TestBase
{
    [Test]
    public async Task CreateAccount_WithValidRequest_Should_CreateAccount()
    {
        // Arrange
        var request = new CreateAccountRequest("Account", "Bank", "Icon", AccountType.Expenses, 0);

        // Act
        var response = await HttpClient.PostAsJsonAsync("accounts", request);
        var result = await response.Content.ReadFromJsonOrDefaultAsync<CreateAccountResponse>();
        var accountInDb = await GetDbContext().Accounts.FirstOrDefaultAsync();

        // Assert
        Check.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        Check.That(accountInDb).IsNotNull();
        Check.That(accountInDb?.Name).IsEqualTo(request.Name);
        Check.That(accountInDb?.Bank).IsEqualTo(request.Bank);
        Check.That(accountInDb?.Icon).IsEqualTo(request.Icon);
        Check.That(accountInDb?.Type).IsEqualTo(request.Type);
        Check.That(accountInDb?.InitialBalance).IsEqualTo(request.InitialBalance);
        Check.That(accountInDb?.Balance).IsEqualTo(request.InitialBalance);
        Check.That(accountInDb?.OwnerId).IsEqualTo(DefaultUser.Id);
        Check.That(result).IsNotNull();
        Check.That(result?.Name).IsEqualTo(request.Name);
        Check.That(result?.Bank).IsEqualTo(request.Bank);
        Check.That(result?.Icon).IsEqualTo(request.Icon);
        Check.That(result?.Type).IsEqualTo(request.Type);
        Check.That(result?.Balance).IsEqualTo(request.InitialBalance);
    }

    [TestCase("", "")]
    [TestCase(null, null)]
    [TestCase("name", null)]
    [TestCase(null, "bank")]
    public async Task CreateAccount_WithWrongRequest_ShouldReturn_ErrorResponse(string name, string bank)
    {
        // Arrange
        var request = new CreateAccountRequest(name, bank, "Icon", AccountType.Expenses, 0);

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