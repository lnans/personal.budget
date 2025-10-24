using System.Net.Http.Json;
using Application.Features.Accounts.Commands.UpdateAccountOperationAmount;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TestFixtures.Domain;

namespace Api.Tests.Accounts;

[Collection(ApiTestCollection.CollectionName)]
public class UpdateAccountOperationAmountTests : ApiTestBase
{
    private const string BaseEndpoint = "/accounts";

    public UpdateAccountOperationAmountTests(ApiTestFixture factory)
        : base(factory) { }

    [Fact]
    public async Task UpdateAccountOperationAmount_WithValidAmount_ShouldUpdateOperationAndAccountBalance()
    {
        // Arrange
        var account = AccountFixture.CreateValidAccount(User.Id, name: "Test Account", initialBalance: 100m);
        account.AddOperation("Initial Operation", 50m, DateTimeOffset.UtcNow);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        var operationId = account.Operations.First().Id;
        var updateCommand = new UpdateAccountOperationAmountCommand { Amount = 75m };

        // Act
        var response = await ApiClient
            .LoggedAs(UserToken)
            .PutAsJsonAsync(
                $"{BaseEndpoint}/{account.Id}/operations/{operationId}/amount",
                updateCommand,
                CancellationToken
            );
        var result = await response.ReadResponseOrProblemAsync<UpdateAccountOperationAmountResponse>(CancellationToken);

        // Assert
        result.ShouldBeSuccessful();
        result.Response.ShouldNotBeNull();
        result.Response.Id.ShouldBe(operationId);
        result.Response.Amount.ShouldBe(75m);
        result.Response.PreviousBalance.ShouldBe(100m);
        result.Response.NextBalance.ShouldBe(175m); // 100 + 75

        // Verify account balance was updated
        var updatedAccount = await DbContext
            .Accounts.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == account.Id, CancellationToken);
        updatedAccount.ShouldNotBeNull();
        updatedAccount.Balance.ShouldBe(175m);
    }

    [Fact]
    public async Task UpdateAccountOperationAmount_WithNegativeAmount_ShouldUpdateCorrectly()
    {
        // Arrange
        var account = AccountFixture.CreateValidAccount(User.Id, name: "Test Account", initialBalance: 100m);
        account.AddOperation("Initial Operation", 50m, DateTimeOffset.UtcNow);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        var operationId = account.Operations.First().Id;
        var updateCommand = new UpdateAccountOperationAmountCommand { Amount = -30m };

        // Act
        var response = await ApiClient
            .LoggedAs(UserToken)
            .PutAsJsonAsync(
                $"{BaseEndpoint}/{account.Id}/operations/{operationId}/amount",
                updateCommand,
                CancellationToken
            );
        var result = await response.ReadResponseOrProblemAsync<UpdateAccountOperationAmountResponse>(CancellationToken);

        // Assert
        result.ShouldBeSuccessful();
        result.Response.ShouldNotBeNull();
        result.Response.Amount.ShouldBe(-30m);
        result.Response.NextBalance.ShouldBe(70m); // 100 - 30

        var updatedAccount = await DbContext
            .Accounts.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == account.Id, CancellationToken);
        updatedAccount!.Balance.ShouldBe(70m);
    }

    [Fact]
    public async Task UpdateAccountOperationAmount_WithMultipleOperations_ShouldCascadeBalanceChanges()
    {
        // Arrange
        var account = AccountFixture.CreateValidAccount(User.Id, name: "Test Account", initialBalance: 100m);
        var now = DateTimeOffset.UtcNow;
        account.AddOperation("First", 50m, now);
        account.AddOperation("Second", 30m, now.AddMilliseconds(10));
        account.AddOperation("Third", 20m, now.AddMilliseconds(20));
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        // Balance should be: 100 + 50 + 30 + 20 = 200
        account.Balance.ShouldBe(200m);

        // Update the first operation (should cascade to all subsequent operations)
        var firstOperationId = account.Operations.OrderBy(o => o.CreatedAt).First().Id;
        var updateCommand = new UpdateAccountOperationAmountCommand { Amount = 100m }; // Changed from 50 to 100

        // Act
        var response = await ApiClient
            .LoggedAs(UserToken)
            .PutAsJsonAsync(
                $"{BaseEndpoint}/{account.Id}/operations/{firstOperationId}/amount",
                updateCommand,
                CancellationToken
            );
        var result = await response.ReadResponseOrProblemAsync<UpdateAccountOperationAmountResponse>(CancellationToken);

        // Assert
        result.ShouldBeSuccessful();

        // Verify all operations have been updated correctly
        var updatedAccount = await DbContext
            .Accounts.AsNoTracking()
            .Include(a => a.Operations)
            .FirstOrDefaultAsync(a => a.Id == account.Id, CancellationToken);

        var updatedOperations = updatedAccount!.Operations.OrderBy(o => o.CreatedAt).ToList();

        // First operation: 100 + 100 = 200
        updatedOperations[0].Amount.ShouldBe(100m);
        updatedOperations[0].PreviousBalance.ShouldBe(100m);
        updatedOperations[0].NextBalance.ShouldBe(200m);

        // Second operation: 200 + 30 = 230 (previous balance cascaded)
        updatedOperations[1].Amount.ShouldBe(30m);
        updatedOperations[1].PreviousBalance.ShouldBe(200m); // Updated from first operation
        updatedOperations[1].NextBalance.ShouldBe(230m);

        // Third operation: 230 + 20 = 250 (previous balance cascaded)
        updatedOperations[2].Amount.ShouldBe(20m);
        updatedOperations[2].PreviousBalance.ShouldBe(230m); // Updated from second operation
        updatedOperations[2].NextBalance.ShouldBe(250m);

        // Account balance: 100 + 100 + 30 + 20 = 250
        updatedAccount.Balance.ShouldBe(250m);
    }

    [Fact]
    public async Task UpdateAccountOperationAmount_UpdateMiddleOperation_ShouldCascadeToSubsequentOperations()
    {
        // Arrange
        var account = AccountFixture.CreateValidAccount(User.Id, name: "Test Account", initialBalance: 100m);
        var now = DateTimeOffset.UtcNow;
        account.AddOperation("First", 50m, now);
        account.AddOperation("Second", 30m, now.AddMilliseconds(10));
        account.AddOperation("Third", 20m, now.AddMilliseconds(20));
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        var operations = account.Operations.OrderBy(o => o.CreatedAt).ToList();

        // Update the middle operation
        var middleOperationId = operations[1].Id;
        var updateCommand = new UpdateAccountOperationAmountCommand { Amount = 80m }; // Changed from 30 to 80

        // Act
        await ApiClient
            .LoggedAs(UserToken)
            .PutAsJsonAsync(
                $"{BaseEndpoint}/{account.Id}/operations/{middleOperationId}/amount",
                updateCommand,
                CancellationToken
            );

        // Assert
        var updatedAccount = await DbContext
            .Accounts.AsNoTracking()
            .Include(a => a.Operations)
            .FirstOrDefaultAsync(a => a.Id == account.Id, CancellationToken);

        var updatedOperations = updatedAccount!.Operations.OrderBy(o => o.CreatedAt).ToList();

        // First operation should remain unchanged
        updatedOperations[0].Amount.ShouldBe(50m);
        updatedOperations[0].PreviousBalance.ShouldBe(100m);
        updatedOperations[0].NextBalance.ShouldBe(150m);

        // Second operation should be updated
        updatedOperations[1].Amount.ShouldBe(80m);
        updatedOperations[1].PreviousBalance.ShouldBe(150m);
        updatedOperations[1].NextBalance.ShouldBe(230m);

        // Third operation should have cascaded balance update
        updatedOperations[2].Amount.ShouldBe(20m);
        updatedOperations[2].PreviousBalance.ShouldBe(230m); // Updated from second operation
        updatedOperations[2].NextBalance.ShouldBe(250m);

        // Account balance should reflect all changes
        updatedAccount.Balance.ShouldBe(250m); // 100 + 50 + 80 + 20
    }

    [Fact]
    public async Task UpdateAccountOperationAmount_UpdateLastOperation_ShouldNotAffectOthers()
    {
        // Arrange
        var account = AccountFixture.CreateValidAccount(User.Id, name: "Test Account", initialBalance: 100m);
        var now = DateTimeOffset.UtcNow;
        account.AddOperation("First", 50m, now);
        account.AddOperation("Second", 30m, now.AddMilliseconds(10));
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        var operations = account.Operations.OrderBy(o => o.CreatedAt).ToList();

        // Update the last operation
        var lastOperationId = operations[1].Id;
        var updateCommand = new UpdateAccountOperationAmountCommand { Amount = 100m }; // Changed from 30 to 100

        // Act
        await ApiClient
            .LoggedAs(UserToken)
            .PutAsJsonAsync(
                $"{BaseEndpoint}/{account.Id}/operations/{lastOperationId}/amount",
                updateCommand,
                CancellationToken
            );

        // Assert
        var updatedAccount = await DbContext
            .Accounts.AsNoTracking()
            .Include(a => a.Operations)
            .FirstOrDefaultAsync(a => a.Id == account.Id, CancellationToken);

        var updatedOperations = updatedAccount!.Operations.OrderBy(o => o.CreatedAt).ToList();

        // First operation should remain unchanged
        updatedOperations[0].Amount.ShouldBe(50m);
        updatedOperations[0].PreviousBalance.ShouldBe(100m);
        updatedOperations[0].NextBalance.ShouldBe(150m);

        // Last operation should be updated
        updatedOperations[1].Amount.ShouldBe(100m);
        updatedOperations[1].PreviousBalance.ShouldBe(150m);
        updatedOperations[1].NextBalance.ShouldBe(250m);

        // Account balance should reflect the change
        updatedAccount.Balance.ShouldBe(250m); // 100 + 50 + 100
    }

    [Fact]
    public async Task UpdateAccountOperationAmount_WithZeroAmount_ShouldUpdateCorrectly()
    {
        // Arrange
        var account = AccountFixture.CreateValidAccount(User.Id, name: "Test Account", initialBalance: 100m);
        account.AddOperation("Test Operation", 50m, DateTimeOffset.UtcNow);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        var operationId = account.Operations.First().Id;
        var updateCommand = new UpdateAccountOperationAmountCommand { Amount = 0m };

        // Act
        var response = await ApiClient
            .LoggedAs(UserToken)
            .PutAsJsonAsync(
                $"{BaseEndpoint}/{account.Id}/operations/{operationId}/amount",
                updateCommand,
                CancellationToken
            );
        var result = await response.ReadResponseOrProblemAsync<UpdateAccountOperationAmountResponse>(CancellationToken);

        // Assert
        result.ShouldBeSuccessful();
        result.Response.ShouldNotBeNull();
        result.Response.Amount.ShouldBe(0m);
        result.Response.NextBalance.ShouldBe(100m); // Previous balance + 0

        var updatedAccount = await DbContext
            .Accounts.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == account.Id, CancellationToken);
        updatedAccount!.Balance.ShouldBe(100m);
    }

    [Fact]
    public async Task UpdateAccountOperationAmount_WithNonExistentOperation_ShouldReturnNotFound()
    {
        // Arrange
        var account = AccountFixture.CreateValidAccount(User.Id, name: "Test Account", initialBalance: 100m);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        var nonExistentOperationId = Guid.NewGuid();
        var updateCommand = new UpdateAccountOperationAmountCommand { Amount = 100m };

        // Act
        var response = await ApiClient
            .LoggedAs(UserToken)
            .PutAsJsonAsync(
                $"{BaseEndpoint}/{account.Id}/operations/{nonExistentOperationId}/amount",
                updateCommand,
                CancellationToken
            );
        var result = await response.ReadResponseOrProblemAsync<UpdateAccountOperationAmountResponse>(CancellationToken);

        // Assert
        result.ShouldBeProblem();
        result.Problem.ShouldNotBeNull();
        result.Problem.Status.ShouldBe(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task UpdateAccountOperationAmount_WithNonExistentAccount_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistentAccountId = Guid.NewGuid();
        var nonExistentOperationId = Guid.NewGuid();
        var updateCommand = new UpdateAccountOperationAmountCommand { Amount = 100m };

        // Act
        var response = await ApiClient
            .LoggedAs(UserToken)
            .PutAsJsonAsync(
                $"{BaseEndpoint}/{nonExistentAccountId}/operations/{nonExistentOperationId}/amount",
                updateCommand,
                CancellationToken
            );
        var result = await response.ReadResponseOrProblemAsync<UpdateAccountOperationAmountResponse>(CancellationToken);

        // Assert
        result.ShouldBeProblem();
        result.Problem.ShouldNotBeNull();
        result.Problem.Status.ShouldBe(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task UpdateAccountOperationAmount_FromAnotherUserAccount_ShouldReturnNotFound()
    {
        // Arrange
        var otherUser = UserFixture.CreateValidUser(login: "otheruser");
        DbContext.Users.Add(otherUser);
        await DbContext.SaveChangesAsync(CancellationToken);

        var otherAccount = AccountFixture.CreateValidAccount(otherUser.Id, name: "Other Account", initialBalance: 100m);
        DbContext.Accounts.Add(otherAccount);
        await DbContext.SaveChangesAsync(CancellationToken);

        // Create an operation for the other user's account using direct DB manipulation
        var operationResult = otherAccount.AddOperation("Test", 50m, DateTimeOffset.UtcNow);
        await DbContext.SaveChangesAsync(CancellationToken);

        var operationId = otherAccount.Operations.First().Id;
        var updateCommand = new UpdateAccountOperationAmountCommand { Amount = 100m };

        // Act - Try to update operation from another user's account
        var response = await ApiClient
            .LoggedAs(UserToken)
            .PutAsJsonAsync(
                $"{BaseEndpoint}/{otherAccount.Id}/operations/{operationId}/amount",
                updateCommand,
                CancellationToken
            );
        var result = await response.ReadResponseOrProblemAsync<UpdateAccountOperationAmountResponse>(CancellationToken);

        // Assert
        result.ShouldBeProblem();
        result.Problem.ShouldNotBeNull();
        result.Problem.Status.ShouldBe(StatusCodes.Status404NotFound);
    }
}
