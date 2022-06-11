using System;
using System.Net;
using System.Threading.Tasks;
using Application.Commands.Operations;
using Domain.Common;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using NFluent;
using NUnit.Framework;

namespace Api.IntegrationTests.Commands.Operations;

[TestFixture]
public class UpdateOperationTests : TestBase
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
        var operation = new Operation
        {
            Id = Guid.Parse(OperationId).ToString(),
            Account = account,
            Amount = -50,
            Description = "test",
            Type = OperationType.Expense,
            CreatedById = DefaultUser.Id,
            CreationDate = DateTime.UtcNow,
            ExecutionDate = DateTime.UtcNow,
            Tag = new OperationTag
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Test",
                Color = "#000000",
                OwnerId = DefaultUser.Id
            }
        };
        await dbContext.Accounts.AddAsync(account);
        await dbContext.Operations.AddAsync(operation);
        await dbContext.SaveChangesAsync();
    }

    private const string OperationId = "d32ac4ad-352a-4fb8-8455-04801d365ed2";

    [Test]
    public async Task UpdateOperation_Should_UpdateOperationAndAccount_WithValidRequest()
    {
        // Arrange
        var request = new UpdateOperationRequest("updated", null, 50);

        // Act
        var response = await HttpClient.PatchAsJsonAsync($"operations/{OperationId}", request);
        var result = await response.Content.ReadFromJsonOrDefaultAsync<UpdateOperationResponse>();
        var operationInDb = await GetDbContext().Operations.Include(o => o.Tag).FirstOrDefaultAsync();
        var accountInDb = await GetDbContext().Accounts.FirstOrDefaultAsync();

        // Assert
        Check.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        Check.That(result).IsNotNull();
        Check.That(operationInDb).IsNotNull();
        Check.That(accountInDb).IsNotNull();
        Check.That(operationInDb?.Description).IsEqualTo(request.Description);
        Check.That(operationInDb?.Tag).IsNull();
        Check.That(operationInDb?.Amount).IsEqualTo(request.Amount);
        Check.That(accountInDb?.Balance).IsEqualTo(150m);
    }

    [TestCase("", "unknown", "", 0, ExpectedResult = HttpStatusCode.BadRequest)]
    [TestCase("updated", "unknown", "", 0, ExpectedResult = HttpStatusCode.BadRequest)]
    [TestCase("updated", "unknown", "", 10, ExpectedResult = HttpStatusCode.NotFound)]
    [TestCase("updated", OperationId, "unknown", 10, ExpectedResult = HttpStatusCode.NotFound)]
    public async Task<HttpStatusCode> UpdateOperation_ShouldReturn_ErrorResponse_WithWrongRequest(string description, string operationId, string tagId,
        decimal amount)
    {
        // Arrange
        var request = new UpdateOperationRequest(description, tagId, amount);

        // Act
        var response = await HttpClient.PatchAsJsonAsync($"operations/{operationId}", request);
        var result = await response.Content.ReadFromJsonOrDefaultAsync<ErrorResponse>();
        var operationInDb = await GetDbContext().Operations.Include(o => o.Tag).FirstOrDefaultAsync();
        var accountInDb = await GetDbContext().Accounts.FirstOrDefaultAsync();

        // Assert
        Check.That(result).IsNotNull();
        Check.That(operationInDb).IsNotNull();
        Check.That(accountInDb).IsNotNull();
        Check.That(operationInDb?.Description).IsNotEqualTo(request.Description);
        Check.That(operationInDb?.Tag).IsNotNull();
        Check.That(operationInDb?.Amount).IsNotEqualTo(request.Amount);
        Check.That(accountInDb?.Balance).IsEqualTo(50m);
        Check.That(result?.Message).IsNotEmpty();

        return response.StatusCode;
    }
}