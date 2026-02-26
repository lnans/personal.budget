using System.Net.Http.Json;
using Application.Features.Accounts.Commands.PatchAccount;
using Domain.Accounts;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TestFixtures.Domain;

namespace Api.Tests.Accounts;

[Collection(ApiTestCollection.CollectionName)]
public class PatchAccountTests : ApiTestBase
{
    private const string BaseEndpoint = "/accounts";

    public PatchAccountTests(ApiTestFixture factory)
        : base(factory) { }

    [Fact]
    public async Task PatchAccount_WithValidData_ShouldUpdateAccount()
    {
        // Arrange
        var account = AccountFixture.CreateValidAccount(User.Id, name: "Original Name", initialBalance: 100m);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        var originalCreatedAt = account.CreatedAt;
        var patchCommand = new PatchAccountCommand
        {
            Id = account.Id,
            Name = "Updated Name",
            Bank = "Updated Bank",
        };

        // Act
        var response = await ApiClient
            .LoggedAs(UserToken)
            .PatchAsJsonAsync($"{BaseEndpoint}/{account.Id}", patchCommand, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<PatchAccountResponse>(CancellationToken);

        // Assert
        result.ShouldBeSuccessful();
        result.Response.ShouldNotBeNull();
        result.Response.Id.ShouldBe(account.Id);
        result.Response.Name.ShouldBe(patchCommand.Name);
        result.Response.Bank.ShouldBe(patchCommand.Bank);
        result.Response.Type.ShouldBe(account.Type);
        result.Response.InitialBalance.ShouldBe(100m);
        result.Response.Balance.ShouldBe(100m);
        result.Response.CreatedAt.ShouldBeCloseTo(originalCreatedAt, TimeSpan.FromMilliseconds(1));
        result.Response.UpdatedAt.ShouldBeGreaterThan(result.Response.CreatedAt);
        result.Response.UpdatedAt.ShouldBeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task PatchAccount_WithEmptyName_ShouldReturnValidationError()
    {
        // Arrange
        var account = AccountFixture.CreateValidAccount(User.Id, name: "Test Account", initialBalance: 100m);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        var patchCommand = new PatchAccountCommand
        {
            Id = account.Id,
            Name = "",
            Bank = account.Bank,
        };

        // Act
        var response = await ApiClient
            .LoggedAs(UserToken)
            .PatchAsJsonAsync($"{BaseEndpoint}/{account.Id}", patchCommand, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<PatchAccountResponse>(CancellationToken);

        // Assert
        result.ShouldBeProblem();
        result.Problem.ShouldNotBeNull();
        result.Problem.Status.ShouldBe(StatusCodes.Status400BadRequest);
        result.Problem.ShouldHaveValidationError("Name", AccountErrors.AccountNameRequired.Code);
    }

    [Fact]
    public async Task PatchAccount_WithTooLongName_ShouldReturnValidationError()
    {
        // Arrange
        var account = AccountFixture.CreateValidAccount(User.Id, name: "Test Account", initialBalance: 100m);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        var patchCommand = new PatchAccountCommand
        {
            Id = account.Id,
            Name = AccountFixture.GenerateLongAccountName(),
            Bank = account.Bank,
        };

        // Act
        var response = await ApiClient
            .LoggedAs(UserToken)
            .PatchAsJsonAsync($"{BaseEndpoint}/{account.Id}", patchCommand, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<PatchAccountResponse>(CancellationToken);

        // Assert
        result.ShouldBeProblem();
        result.Problem.ShouldNotBeNull();
        result.Problem.Status.ShouldBe(StatusCodes.Status400BadRequest);
        result.Problem.ShouldHaveValidationError("Name", AccountErrors.AccountNameTooLong.Code);
    }

    [Fact]
    public async Task PatchAccount_WithEmptyBank_ShouldReturnValidationError()
    {
        // Arrange
        var account = AccountFixture.CreateValidAccount(User.Id, name: "Test Account", initialBalance: 100m);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        var patchCommand = new PatchAccountCommand
        {
            Id = account.Id,
            Name = "Updated Name",
            Bank = "",
        };

        // Act
        var response = await ApiClient
            .LoggedAs(UserToken)
            .PatchAsJsonAsync($"{BaseEndpoint}/{account.Id}", patchCommand, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<PatchAccountResponse>(CancellationToken);

        // Assert
        result.ShouldBeProblem();
        result.Problem.ShouldNotBeNull();
        result.Problem.Status.ShouldBe(StatusCodes.Status400BadRequest);
        result.Problem.ShouldHaveValidationError("Bank", AccountErrors.AccountBankRequired.Code);
    }

    [Fact]
    public async Task PatchAccount_WithTooLongBank_ShouldReturnValidationError()
    {
        // Arrange
        var account = AccountFixture.CreateValidAccount(User.Id, name: "Test Account", initialBalance: 100m);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        var patchCommand = new PatchAccountCommand
        {
            Id = account.Id,
            Name = "Updated Name",
            Bank = AccountFixture.GenerateLongAccountBank(),
        };

        // Act
        var response = await ApiClient
            .LoggedAs(UserToken)
            .PatchAsJsonAsync($"{BaseEndpoint}/{account.Id}", patchCommand, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<PatchAccountResponse>(CancellationToken);

        // Assert
        result.ShouldBeProblem();
        result.Problem.ShouldNotBeNull();
        result.Problem.Status.ShouldBe(StatusCodes.Status400BadRequest);
        result.Problem.ShouldHaveValidationError("Bank", AccountErrors.AccountBankTooLong.Code);
    }

    [Fact]
    public async Task PatchAccount_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var patchCommand = new PatchAccountCommand
        {
            Id = nonExistentId,
            Name = "Updated Name",
            Bank = "Updated Bank",
        };

        // Act
        var response = await ApiClient
            .LoggedAs(UserToken)
            .PatchAsJsonAsync($"{BaseEndpoint}/{nonExistentId}", patchCommand, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<PatchAccountResponse>(CancellationToken);

        // Assert
        result.ShouldBeProblem();
        result.Problem.ShouldNotBeNull();
        result.Problem.Status.ShouldBe(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task PatchAccount_ShouldPersistInDatabase()
    {
        // Arrange
        var account = AccountFixture.CreateValidAccount(User.Id, name: "Original Name", initialBalance: 200m);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        var originalCreatedAt = account.CreatedAt;
        var patchCommand = new PatchAccountCommand
        {
            Id = account.Id,
            Name = "Updated Name",
            Bank = "Updated Bank",
        };

        // Act
        var response = await ApiClient
            .LoggedAs(UserToken)
            .PatchAsJsonAsync($"{BaseEndpoint}/{account.Id}", patchCommand, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<PatchAccountResponse>(CancellationToken);

        // Assert
        result.ShouldBeSuccessful();
        result.Response.ShouldNotBeNull();

        var accountInDb = await DbContext
            .Accounts.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == account.Id, CancellationToken);
        accountInDb.ShouldNotBeNull();
        accountInDb.Name.ShouldBe(patchCommand.Name);
        accountInDb.Bank.ShouldBe(patchCommand.Bank);
        accountInDb.Type.ShouldBe(account.Type);
        accountInDb.Balance.ShouldBe(200m);
        accountInDb.UserId.ShouldBe(User.Id);
        accountInDb.CreatedAt.ShouldBeCloseTo(originalCreatedAt, TimeSpan.FromMilliseconds(1));
        accountInDb.UpdatedAt.ShouldBeGreaterThan(accountInDb.CreatedAt);
        accountInDb.UpdatedAt.ShouldBeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task PatchAccount_WithoutInitialBalance_ShouldNotUpdateBalance()
    {
        // Arrange
        var account = AccountFixture.CreateValidAccount(User.Id, name: "Test Account", initialBalance: 500m);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        var originalBalance = account.Balance;
        var patchCommand = new PatchAccountCommand
        {
            Id = account.Id,
            Name = "Updated Name",
            Bank = "Updated Bank",
        };

        // Act
        var response = await ApiClient
            .LoggedAs(UserToken)
            .PatchAsJsonAsync($"{BaseEndpoint}/{account.Id}", patchCommand, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<PatchAccountResponse>(CancellationToken);

        // Assert
        result.ShouldBeSuccessful();
        result.Response.ShouldNotBeNull();
        result.Response.Balance.ShouldBe(originalBalance);
        result.Response.InitialBalance.ShouldBe(500m);
    }

    [Fact]
    public async Task PatchAccount_WithNewInitialBalance_ShouldUpdateInitialBalanceAndBalance()
    {
        // Arrange
        var account = AccountFixture.CreateValidAccount(User.Id, name: "Test Account", initialBalance: 100m);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        var patchCommand = new PatchAccountCommand
        {
            Id = account.Id,
            Name = account.Name,
            Bank = account.Bank,
            InitialBalance = 500m,
        };

        // Act
        var response = await ApiClient
            .LoggedAs(UserToken)
            .PatchAsJsonAsync($"{BaseEndpoint}/{account.Id}", patchCommand, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<PatchAccountResponse>(CancellationToken);

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
    public async Task PatchAccount_WithNewInitialBalance_ShouldRecalculateOperationBalances()
    {
        // Arrange
        var account = AccountFixture.CreateValidAccount(User.Id, name: "Test Account", initialBalance: 100m);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        // Add operations: +50, -30, +20 => Balance should be 100 + 50 - 30 + 20 = 140
        account.AddOperation("Operation 1", 50m, DateTimeOffset.UtcNow.AddMinutes(1));
        account.AddOperation("Operation 2", -30m, DateTimeOffset.UtcNow.AddMinutes(2));
        account.AddOperation("Operation 3", 20m, DateTimeOffset.UtcNow.AddMinutes(3));
        await DbContext.SaveChangesAsync(CancellationToken);

        account.Balance.ShouldBe(140m);

        var patchCommand = new PatchAccountCommand
        {
            Id = account.Id,
            Name = account.Name,
            Bank = account.Bank,
            InitialBalance = 200m, // Change from 100 to 200
        };

        // Act
        var response = await ApiClient
            .LoggedAs(UserToken)
            .PatchAsJsonAsync($"{BaseEndpoint}/{account.Id}", patchCommand, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<PatchAccountResponse>(CancellationToken);

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
    public async Task PatchAccount_WithNegativeInitialBalance_ShouldUpdateCorrectly()
    {
        // Arrange
        var account = AccountFixture.CreateValidAccount(User.Id, name: "Test Account", initialBalance: 100m);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        account.AddOperation("Operation 1", 50m, DateTimeOffset.UtcNow.AddMinutes(1));
        await DbContext.SaveChangesAsync(CancellationToken);

        var patchCommand = new PatchAccountCommand
        {
            Id = account.Id,
            Name = account.Name,
            Bank = account.Bank,
            InitialBalance = -100m,
        };

        // Act
        var response = await ApiClient
            .LoggedAs(UserToken)
            .PatchAsJsonAsync($"{BaseEndpoint}/{account.Id}", patchCommand, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<PatchAccountResponse>(CancellationToken);

        // Assert
        result.ShouldBeSuccessful();
        result.Response.ShouldNotBeNull();
        result.Response.InitialBalance.ShouldBe(-100m);
        // Balance should be -100 + 50 = -50
        result.Response.Balance.ShouldBe(-50m);
    }

    [Fact]
    public async Task PatchAccount_WithZeroInitialBalance_ShouldUpdateCorrectly()
    {
        // Arrange
        var account = AccountFixture.CreateValidAccount(User.Id, name: "Test Account", initialBalance: 500m);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        account.AddOperation("Operation 1", 100m, DateTimeOffset.UtcNow.AddMinutes(1));
        await DbContext.SaveChangesAsync(CancellationToken);

        var patchCommand = new PatchAccountCommand
        {
            Id = account.Id,
            Name = account.Name,
            Bank = account.Bank,
            InitialBalance = 0m,
        };

        // Act
        var response = await ApiClient
            .LoggedAs(UserToken)
            .PatchAsJsonAsync($"{BaseEndpoint}/{account.Id}", patchCommand, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<PatchAccountResponse>(CancellationToken);

        // Assert
        result.ShouldBeSuccessful();
        result.Response.ShouldNotBeNull();
        result.Response.InitialBalance.ShouldBe(0m);
        // Balance should be 0 + 100 = 100
        result.Response.Balance.ShouldBe(100m);
    }

    [Fact]
    public async Task PatchAccount_WithSameInitialBalance_ShouldNotRecalculateOperations()
    {
        // Arrange
        var account = AccountFixture.CreateValidAccount(User.Id, name: "Test Account", initialBalance: 100m);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        account.AddOperation("Operation 1", 50m, DateTimeOffset.UtcNow.AddMinutes(1));
        await DbContext.SaveChangesAsync(CancellationToken);

        var operationUpdatedAt = account.Operations[0].UpdatedAt;

        var patchCommand = new PatchAccountCommand
        {
            Id = account.Id,
            Name = "New Name",
            Bank = "New Bank",
            InitialBalance = 100m, // Same as original
        };

        // Act
        var response = await ApiClient
            .LoggedAs(UserToken)
            .PatchAsJsonAsync($"{BaseEndpoint}/{account.Id}", patchCommand, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<PatchAccountResponse>(CancellationToken);

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
    public async Task PatchAccount_WithAllFields_ShouldUpdateAllFields()
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

        var patchCommand = new PatchAccountCommand
        {
            Id = account.Id,
            Name = "New Name",
            Bank = "New Bank",
            InitialBalance = 999m,
        };

        // Act
        var response = await ApiClient
            .LoggedAs(UserToken)
            .PatchAsJsonAsync($"{BaseEndpoint}/{account.Id}", patchCommand, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<PatchAccountResponse>(CancellationToken);

        // Assert
        result.ShouldBeSuccessful();
        result.Response.ShouldNotBeNull();
        result.Response.Name.ShouldBe("New Name");
        result.Response.Bank.ShouldBe("New Bank");
        result.Response.InitialBalance.ShouldBe(999m);
        result.Response.Balance.ShouldBe(999m);
    }
}
