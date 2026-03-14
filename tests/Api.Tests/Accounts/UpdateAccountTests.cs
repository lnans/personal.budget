using System.Net.Http.Json;
using Api.Contracts.Accounts;
using Application.Features.Accounts.Commands.UpdateAccount;
using Domain.Accounts;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TestFixtures.Domain;

namespace Api.Tests.Accounts;

[Collection(ApiTestCollection.CollectionName)]
public class UpdateAccountTests : ApiTestBase
{
    private const string BaseEndpoint = "/accounts";

    public UpdateAccountTests(ApiTestFixture factory)
        : base(factory) { }

    [Fact]
    public async Task UpdateAccount_WithValidData_ShouldUpdateAccount()
    {
        // Arrange
        var account = AccountFixture.CreateValidAccount(User.Id, name: "Original Name", initialBalance: 100m);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        var originalCreatedAt = account.CreatedAt;
        var updateAccountRequest = new UpdateAccountRequest("Updated Name", "Updated Bank");

        // Act
        var response = await ApiClient
            .LoggedAs(UserToken)
            .PutAsJsonAsync($"{BaseEndpoint}/{account.Id}", updateAccountRequest, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<UpdateAccountResponse>(CancellationToken);

        // Assert
        result.ShouldBeSuccessful();
        result.Response.ShouldNotBeNull();
        result.Response.Id.ShouldBe(account.Id);
        result.Response.Name.ShouldBe(updateAccountRequest.Name);
        result.Response.Bank.ShouldBe(updateAccountRequest.Bank);
        result.Response.Type.ShouldBe(account.Type);
        result.Response.InitialBalance.ShouldBe(100m);
        result.Response.Balance.ShouldBe(100m);
        result.Response.CreatedAt.ShouldBeCloseTo(originalCreatedAt, TimeSpan.FromMilliseconds(1));
        result.Response.UpdatedAt.ShouldBeGreaterThan(result.Response.CreatedAt);
        result.Response.UpdatedAt.ShouldBeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task UpdateAccount_WithEmptyName_ShouldReturnValidationError()
    {
        // Arrange
        var account = AccountFixture.CreateValidAccount(User.Id, name: "Test Account", initialBalance: 100m);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        var updateAccountRequest = new UpdateAccountRequest("", account.Bank);

        // Act
        var response = await ApiClient
            .LoggedAs(UserToken)
            .PutAsJsonAsync($"{BaseEndpoint}/{account.Id}", updateAccountRequest, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<UpdateAccountResponse>(CancellationToken);

        // Assert
        result.ShouldBeProblem();
        result.Problem.ShouldNotBeNull();
        result.Problem.Status.ShouldBe(StatusCodes.Status400BadRequest);
        result.Problem.ShouldHaveError(AccountErrors.AccountNameRequired.Code);
    }

    [Fact]
    public async Task UpdateAccount_WithTooLongName_ShouldReturnValidationError()
    {
        // Arrange
        var account = AccountFixture.CreateValidAccount(User.Id, name: "Test Account", initialBalance: 100m);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        var updateAccountRequest = new UpdateAccountRequest(AccountFixture.GenerateLongAccountName(), account.Bank);

        // Act
        var response = await ApiClient
            .LoggedAs(UserToken)
            .PutAsJsonAsync($"{BaseEndpoint}/{account.Id}", updateAccountRequest, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<UpdateAccountResponse>(CancellationToken);

        // Assert
        result.ShouldBeProblem();
        result.Problem.ShouldNotBeNull();
        result.Problem.Status.ShouldBe(StatusCodes.Status400BadRequest);
        result.Problem.ShouldHaveError(AccountErrors.AccountNameTooLong.Code);
    }

    [Fact]
    public async Task UpdateAccount_WithEmptyBank_ShouldReturnValidationError()
    {
        // Arrange
        var account = AccountFixture.CreateValidAccount(User.Id, name: "Test Account", initialBalance: 100m);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        var updateAccountRequest = new UpdateAccountRequest("Updated Name", "");

        // Act
        var response = await ApiClient
            .LoggedAs(UserToken)
            .PutAsJsonAsync($"{BaseEndpoint}/{account.Id}", updateAccountRequest, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<UpdateAccountResponse>(CancellationToken);

        // Assert
        result.ShouldBeProblem();
        result.Problem.ShouldNotBeNull();
        result.Problem.Status.ShouldBe(StatusCodes.Status400BadRequest);
        result.Problem.ShouldHaveError(AccountErrors.AccountBankRequired.Code);
    }

    [Fact]
    public async Task UpdateAccount_WithTooLongBank_ShouldReturnValidationError()
    {
        // Arrange
        var account = AccountFixture.CreateValidAccount(User.Id, name: "Test Account", initialBalance: 100m);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        var updateAccountRequest = new UpdateAccountRequest("Updated Name", AccountFixture.GenerateLongAccountBank());

        // Act
        var response = await ApiClient
            .LoggedAs(UserToken)
            .PutAsJsonAsync($"{BaseEndpoint}/{account.Id}", updateAccountRequest, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<UpdateAccountResponse>(CancellationToken);

        // Assert
        result.ShouldBeProblem();
        result.Problem.ShouldNotBeNull();
        result.Problem.Status.ShouldBe(StatusCodes.Status400BadRequest);
        result.Problem.ShouldHaveError(AccountErrors.AccountBankTooLong.Code);
    }

    [Fact]
    public async Task UpdateAccount_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var updateAccountRequest = new UpdateAccountRequest("Updated Name", "Updated Bank");

        // Act
        var response = await ApiClient
            .LoggedAs(UserToken)
            .PutAsJsonAsync($"{BaseEndpoint}/{nonExistentId}", updateAccountRequest, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<UpdateAccountResponse>(CancellationToken);

        // Assert
        result.ShouldBeProblem();
        result.Problem.ShouldNotBeNull();
        result.Problem.Status.ShouldBe(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task UpdateAccount_ShouldPersistInDatabase()
    {
        // Arrange
        var account = AccountFixture.CreateValidAccount(User.Id, name: "Original Name", initialBalance: 200m);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        var originalCreatedAt = account.CreatedAt;
        var updateAccountRequest = new UpdateAccountRequest("Updated Name", "Updated Bank");

        // Act
        var response = await ApiClient
            .LoggedAs(UserToken)
            .PutAsJsonAsync($"{BaseEndpoint}/{account.Id}", updateAccountRequest, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<UpdateAccountResponse>(CancellationToken);

        // Assert
        result.ShouldBeSuccessful();
        result.Response.ShouldNotBeNull();

        var accountInDb = await DbContext
            .Accounts.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == account.Id, CancellationToken);
        accountInDb.ShouldNotBeNull();
        accountInDb.Name.ShouldBe(updateAccountRequest.Name);
        accountInDb.Bank.ShouldBe(updateAccountRequest.Bank);
        accountInDb.Type.ShouldBe(account.Type);
        accountInDb.Balance.ShouldBe(200m);
        accountInDb.UserId.ShouldBe(User.Id);
        accountInDb.CreatedAt.ShouldBeCloseTo(originalCreatedAt, TimeSpan.FromMilliseconds(1));
        accountInDb.UpdatedAt.ShouldBeGreaterThan(accountInDb.CreatedAt);
        accountInDb.UpdatedAt.ShouldBeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task UpdateAccount_WithoutInitialBalance_ShouldNotUpdateBalance()
    {
        // Arrange
        var account = AccountFixture.CreateValidAccount(User.Id, name: "Test Account", initialBalance: 500m);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        var originalBalance = account.Balance;
        var updateAccountRequest = new UpdateAccountRequest("Updated Name", "Updated Bank");

        // Act
        var response = await ApiClient
            .LoggedAs(UserToken)
            .PutAsJsonAsync($"{BaseEndpoint}/{account.Id}", updateAccountRequest, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<UpdateAccountResponse>(CancellationToken);

        // Assert
        result.ShouldBeSuccessful();
        result.Response.ShouldNotBeNull();
        result.Response.Balance.ShouldBe(originalBalance);
        result.Response.InitialBalance.ShouldBe(500m);
    }

    [Fact]
    public async Task UpdateAccount_WithNewInitialBalance_ShouldUpdateInitialBalanceAndBalance()
    {
        // Arrange
        var account = AccountFixture.CreateValidAccount(User.Id, name: "Test Account", initialBalance: 100m);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        var updateAccountRequest = new UpdateAccountRequest(account.Name, account.Bank, 500m);

        // Act
        var response = await ApiClient
            .LoggedAs(UserToken)
            .PutAsJsonAsync($"{BaseEndpoint}/{account.Id}", updateAccountRequest, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<UpdateAccountResponse>(CancellationToken);

        // Assert
        result.ShouldBeSuccessful();
        result.Response.ShouldNotBeNull();
        result.Response.InitialBalance.ShouldBe(500m);
        result.Response.Balance.ShouldBe(500m);

        // Verify persistence
        var accountInDb = await DbContext
            .Accounts.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == account.Id, CancellationToken);
        accountInDb.ShouldNotBeNull();
        accountInDb.InitialBalance.ShouldBe(500m);
        accountInDb.Balance.ShouldBe(500m);
    }

    [Fact]
    public async Task UpdateAccount_WithNewInitialBalance_ShouldRecalculateOperationBalances()
    {
        // Arrange
        var account = AccountFixture.CreateValidAccount(User.Id, name: "Test Account", initialBalance: 100m);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        // Add operations: +50, -30, +20 => Balance should be 100 + 50 - 30 + 20 = 140
        account.AddOperation(
            "Operation 1",
            50m,
            false,
            DateTimeOffset.UtcNow.AddMinutes(1),
            DateTimeOffset.UtcNow.AddMinutes(1)
        );
        account.AddOperation(
            "Operation 2",
            -30m,
            false,
            DateTimeOffset.UtcNow.AddMinutes(2),
            DateTimeOffset.UtcNow.AddMinutes(2)
        );
        account.AddOperation(
            "Operation 3",
            20m,
            false,
            DateTimeOffset.UtcNow.AddMinutes(3),
            DateTimeOffset.UtcNow.AddMinutes(3)
        );
        await DbContext.SaveChangesAsync(CancellationToken);

        account.Balance.ShouldBe(140m);

        var updateAccountRequest = new UpdateAccountRequest(account.Name, account.Bank, 200m);

        // Act
        var response = await ApiClient
            .LoggedAs(UserToken)
            .PutAsJsonAsync($"{BaseEndpoint}/{account.Id}", updateAccountRequest, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<UpdateAccountResponse>(CancellationToken);

        // Assert
        result.ShouldBeSuccessful();
        result.Response.ShouldNotBeNull();
        result.Response.InitialBalance.ShouldBe(200m);
        // New balance should be 200 + 50 - 30 + 20 = 240
        result.Response.Balance.ShouldBe(240m);

        // Verify operation balances are recalculated in database
        var operations = await DbContext
            .AccountOperations.AsNoTracking()
            .Where(o => o.AccountId == account.Id)
            .OrderBy(o => o.CreatedAt)
            .ToListAsync(CancellationToken);

        operations.Count.ShouldBe(3);

        // Operation 1: +50, previous = 200, next = 250
        operations[0].PreviousBalance.ShouldBe(200m);
        operations[0].NextBalance.ShouldBe(250m);

        // Operation 2: -30, previous = 250, next = 220
        operations[1].PreviousBalance.ShouldBe(250m);
        operations[1].NextBalance.ShouldBe(220m);

        // Operation 3: +20, previous = 220, next = 240
        operations[2].PreviousBalance.ShouldBe(220m);
        operations[2].NextBalance.ShouldBe(240m);
    }

    [Fact]
    public async Task UpdateAccount_WithNegativeInitialBalance_ShouldUpdateCorrectly()
    {
        // Arrange
        var account = AccountFixture.CreateValidAccount(User.Id, name: "Test Account", initialBalance: 100m);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        account.AddOperation(
            "Operation 1",
            50m,
            false,
            DateTimeOffset.UtcNow.AddMinutes(1),
            DateTimeOffset.UtcNow.AddMinutes(1)
        );
        await DbContext.SaveChangesAsync(CancellationToken);

        var updateAccountRequest = new UpdateAccountRequest(account.Name, account.Bank, -100m);

        // Act
        var response = await ApiClient
            .LoggedAs(UserToken)
            .PutAsJsonAsync($"{BaseEndpoint}/{account.Id}", updateAccountRequest, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<UpdateAccountResponse>(CancellationToken);

        // Assert
        result.ShouldBeSuccessful();
        result.Response.ShouldNotBeNull();
        result.Response.InitialBalance.ShouldBe(-100m);
        // Balance should be -100 + 50 = -50
        result.Response.Balance.ShouldBe(-50m);
    }

    [Fact]
    public async Task UpdateAccount_WithZeroInitialBalance_ShouldUpdateCorrectly()
    {
        // Arrange
        var account = AccountFixture.CreateValidAccount(User.Id, name: "Test Account", initialBalance: 500m);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        account.AddOperation(
            "Operation 1",
            100m,
            false,
            DateTimeOffset.UtcNow.AddMinutes(1),
            DateTimeOffset.UtcNow.AddMinutes(1)
        );
        await DbContext.SaveChangesAsync(CancellationToken);

        var updateAccountRequest = new UpdateAccountRequest(account.Name, account.Bank, 0m);

        // Act
        var response = await ApiClient
            .LoggedAs(UserToken)
            .PutAsJsonAsync($"{BaseEndpoint}/{account.Id}", updateAccountRequest, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<UpdateAccountResponse>(CancellationToken);

        // Assert
        result.ShouldBeSuccessful();
        result.Response.ShouldNotBeNull();
        result.Response.InitialBalance.ShouldBe(0m);
        // Balance should be 0 + 100 = 100
        result.Response.Balance.ShouldBe(100m);
    }

    [Fact]
    public async Task UpdateAccount_WithSameInitialBalance_ShouldNotRecalculateOperations()
    {
        // Arrange
        var account = AccountFixture.CreateValidAccount(User.Id, name: "Test Account", initialBalance: 100m);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        account.AddOperation(
            "Operation 1",
            50m,
            false,
            DateTimeOffset.UtcNow.AddMinutes(1),
            DateTimeOffset.UtcNow.AddMinutes(1)
        );
        await DbContext.SaveChangesAsync(CancellationToken);

        var operationUpdatedAt = account.Operations[0].UpdatedAt;

        var updateAccountRequest = new UpdateAccountRequest("New Name", "New Bank", 100m);

        // Act
        var response = await ApiClient
            .LoggedAs(UserToken)
            .PutAsJsonAsync($"{BaseEndpoint}/{account.Id}", updateAccountRequest, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<UpdateAccountResponse>(CancellationToken);

        // Assert
        result.ShouldBeSuccessful();
        result.Response.ShouldNotBeNull();
        result.Response.Name.ShouldBe("New Name");
        result.Response.Bank.ShouldBe("New Bank");
        result.Response.InitialBalance.ShouldBe(100m);
        result.Response.Balance.ShouldBe(150m);

        // Verify operation was not updated
        var operation = await DbContext
            .AccountOperations.AsNoTracking()
            .FirstOrDefaultAsync(o => o.AccountId == account.Id, CancellationToken);
        operation.ShouldNotBeNull();
        operation.UpdatedAt.ShouldBeCloseTo(operationUpdatedAt, TimeSpan.FromMilliseconds(100));
    }

    [Fact]
    public async Task UpdateAccount_WithAllFields_ShouldUpdateAllFields()
    {
        // Arrange
        var account = AccountFixture.CreateValidAccount(
            User.Id,
            name: "Original Name",
            bank: "Original Bank",
            initialBalance: 100m
        );
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        var updateAccountRequest = new UpdateAccountRequest("New Name", "New Bank", 999m);

        // Act
        var response = await ApiClient
            .LoggedAs(UserToken)
            .PutAsJsonAsync($"{BaseEndpoint}/{account.Id}", updateAccountRequest, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<UpdateAccountResponse>(CancellationToken);

        // Assert
        result.ShouldBeSuccessful();
        result.Response.ShouldNotBeNull();
        result.Response.Name.ShouldBe("New Name");
        result.Response.Bank.ShouldBe("New Bank");
        result.Response.InitialBalance.ShouldBe(999m);
        result.Response.Balance.ShouldBe(999m);
    }
}
