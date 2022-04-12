using System;
using System.Net;
using System.Net.Http.Json;
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
public class CreateOperationTests : TestBase
{
    [SetUp]
    public async Task SetupAccount()
    {
        var dbContext = GetDbContext();
        _account = new Account
        {
            Id = Guid.Parse(AccountId).ToString(),
            Name = "ForTest",
            OwnerId = DefaultUser.Id,
            Balance = 0,
            InitialBalance = 0,
            CreationDate = DateTime.UtcNow,
            Type = AccountType.Expenses,
            Archived = false
        };
        _tag = new OperationTag
        {
            Id = Guid.Parse(TagId).ToString(),
            Name = "Tag",
            OwnerId = DefaultUser.Id,
            Color = "#000000"
        };
        await dbContext.Accounts.AddAsync(_account);
        await dbContext.OperationTags.AddAsync(_tag);
        await dbContext.SaveChangesAsync();
    }

    private const string AccountId = "f12ac4ad-352a-4fb8-8455-04801d365ed2";
    private const string TagId = "d32ac4ad-352a-4fb8-8455-04801d365ed2";
    private Account _account = null!;
    private OperationTag _tag = null!;

    [Test]
    public async Task CreateOperation_Should_CreateOperationAndUpdateAccount_WithValidRequest()
    {
        // Arrange
        var request = new CreateOperationRequest(
            "desc",
            _account.Id,
            _tag.Id,
            OperationType.Expense,
            -50,
            DateTime.UtcNow,
            DateTime.UtcNow);

        // Act
        var response = await HttpClient.PostAsJsonAsync("operations", request);
        var result = await response.Content.ReadFromJsonOrDefaultAsync<CreateOperationResponse>();
        var operationInDb = await GetDbContext().Operations
            .Include(o => o.Account)
            .Include(o => o.Tag)
            .FirstOrDefaultAsync();
        var accountInDb = await GetDbContext().Accounts.FirstOrDefaultAsync();

        // Assert
        Check.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        Check.That(operationInDb).IsNotNull();
        Check.That(operationInDb?.Description).IsEqualTo(request.Description);
        Check.That(operationInDb?.Account?.Id).IsEqualTo(request.AccountId);
        Check.That(operationInDb?.Type).IsEqualTo(request.OperationType);
        Check.That(operationInDb?.Amount).IsEqualTo(-50);
        Check.That(operationInDb?.Tag?.Name).IsEqualTo(_tag.Name);
        Check.That(operationInDb?.Tag?.Color).IsEqualTo(_tag.Color);
        Check.That(operationInDb?.CreationDate).IsEqualTo(request.CreationDate);
        Check.That(operationInDb?.ExecutionDate).IsEqualTo(request.ExecutionDate);
        Check.That(accountInDb?.Balance).IsEqualTo(-50);
        Check.That(result).IsNotNull();
        Check.That(result?.Description).IsEqualTo(request.Description);
        Check.That(result?.AccountId).IsEqualTo(request.AccountId);
        Check.That(result?.AccountName).IsEqualTo(accountInDb?.Name);
        Check.That(result?.TagName).IsEqualTo(_tag.Name);
        Check.That(result?.TagColor).IsEqualTo(_tag.Color);
        Check.That(result?.OperationType).IsEqualTo(request.OperationType);
        Check.That(result?.Amount).IsEqualTo(request.Amount);
        Check.That(result?.CreationDate).IsEqualTo(request.CreationDate);
        Check.That(result?.ExecutionDate).IsEqualTo(request.ExecutionDate);
    }

    [TestCase("", "", "", 0, ExpectedResult = HttpStatusCode.BadRequest)]
    [TestCase("desc", "", "", 0, ExpectedResult = HttpStatusCode.BadRequest)]
    [TestCase("desc", AccountId, TagId, 0, ExpectedResult = HttpStatusCode.BadRequest)]
    [TestCase("desc", "none", "", -10, ExpectedResult = HttpStatusCode.NotFound)]
    [TestCase("desc", AccountId, "none", -10, ExpectedResult = HttpStatusCode.NotFound)]
    public async Task<HttpStatusCode> CreateOperation_ShouldReturn_ErrorResponse_WithWrongRequest(string description, string accountId, string tagId,
        decimal amount)
    {
        // Arrange
        var request = new CreateOperationRequest(
            description,
            accountId,
            tagId,
            OperationType.Expense,
            amount,
            DateTime.UtcNow,
            DateTime.UtcNow);

        // Act
        var response = await HttpClient.PostAsJsonAsync("operations", request);
        var result = await response.Content.ReadFromJsonOrDefaultAsync<ErrorResponse>();
        var operationInDb = await GetDbContext().Operations.FirstOrDefaultAsync();
        var accountInDb = await GetDbContext().Accounts.FirstOrDefaultAsync();

        // Assert
        Check.That(result).IsNotNull();
        Check.That(result?.Message).IsNotEmpty();
        Check.That(operationInDb).IsNull();
        Check.That(accountInDb?.Balance).IsEqualTo(0);

        return response.StatusCode;
    }
}