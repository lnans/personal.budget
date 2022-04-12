using System;
using System.Net;
using System.Threading.Tasks;
using Application.Commands.Operations;
using Domain;
using Domain.Common;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using NFluent;
using NUnit.Framework;

namespace Api.IntegrationTests.Commands.Operations;

[TestFixture]
public class ExecuteOperationTests : TestBase
{
    [SetUp]
    public async Task SetupAccount()
    {
        var dbContext = GetDbContext();
        var account = new Account
        {
            Id = Guid.NewGuid().ToString(),
            Name = "ForTest",
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
    public async Task ExecuteOperation_Should_UpdateExecutionDate_WithValidRequest()
    {
        // Arrange
        var request = new ExecuteOperationRequest(DateTime.UtcNow);

        // Act
        var response = await HttpClient.PatchAsJsonAsync($"operations/{OperationId}/execute", request);
        var operationInDb = await GetDbContext().Operations.FirstOrDefaultAsync();

        // Assert
        Check.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        Check.That(operationInDb).IsNotNull();
        Check.That(operationInDb?.ExecutionDate).IsEqualTo(request.ExecutionDate);
    }

    [Test]
    public async Task ExecuteOperation_ShouldReturn_ErrorResponse_WithUnknownId()
    {
        // Arrange
        var request = new ExecuteOperationRequest(DateTime.UtcNow);

        // Act
        var response = await HttpClient.PatchAsJsonAsync("operations/none/execute", request);
        var result = await response.Content.ReadFromJsonOrDefaultAsync<ErrorResponse>();
        var operationInDb = await GetDbContext().Operations.FirstOrDefaultAsync();

        // Assert
        Check.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
        Check.That(operationInDb).IsNotNull();
        Check.That(operationInDb?.ExecutionDate).IsNull();
        Check.That(result).IsNotNull();
        Check.That(result?.Message).IsEqualTo(Errors.OperationNotFound);
    }

    [Test]
    public async Task ExecuteOperation_ShouldReturn_ErrorResponse_WithWrongRequest()
    {
        // Arrange
        var request = new object();

        // Act
        var response = await HttpClient.PatchAsJsonAsync($"operations/{OperationId}/execute", request);
        var result = await response.Content.ReadFromJsonOrDefaultAsync<ErrorResponse>();
        var operationInDb = await GetDbContext().Operations.FirstOrDefaultAsync();

        // Assert
        Check.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
        Check.That(operationInDb).IsNotNull();
        Check.That(operationInDb?.ExecutionDate).IsNull();
        Check.That(result).IsNotNull();
        Check.That(result?.Message).IsEqualTo(Errors.OperationExecutionDateRequired);
    }
}