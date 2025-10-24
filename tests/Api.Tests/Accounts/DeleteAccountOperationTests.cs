using Application.Features.Accounts.Commands.DeleteAccountOperation;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TestFixtures.Domain;

namespace Api.Tests.Accounts;

[Collection(ApiTestCollection.CollectionName)]
public class DeleteAccountOperationTests : ApiTestBase
{
    private const string BaseEndpoint = "/accounts";

    public DeleteAccountOperationTests(ApiTestFixture factory)
        : base(factory) { }

    [Fact]
    public async Task DeleteAccountOperation_WithSingleOperation_ShouldDeleteOperationAndResetAccountBalance()
    {
        // Arrange
        var account = AccountFixture.CreateValidAccount(User.Id, name: "Test Account", initialBalance: 100m);
        account.AddOperation("Test Operation", 50m, DateTimeOffset.UtcNow);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        var operationId = account.Operations.First().Id;

        // Act
        var response = await ApiClient
            .LoggedAs(UserToken)
            .DeleteAsync($"{BaseEndpoint}/{account.Id}/operations/{operationId}", CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<DeleteAccountOperationResponse>(CancellationToken);

        // Assert
        result.ShouldBeSuccessful();
        result.Response.ShouldNotBeNull();
        result.Response.Id.ShouldBe(operationId);
        result.Response.Amount.ShouldBe(50m);
        result.Response.DeletedAt.ShouldNotBe(default);

        // Verify operation was soft deleted
        var deletedOperation = await DbContext
            .AccountOperations.IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == operationId, CancellationToken);
        deletedOperation.ShouldNotBeNull();
        deletedOperation.DeletedAt.ShouldNotBeNull();
        deletedOperation.NextBalance.ShouldBe(deletedOperation.PreviousBalance); // Deleted operation no longer contributes

        // Verify account balance was reset to initial balance
        var updatedAccount = await DbContext
            .Accounts.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == account.Id, CancellationToken);
        updatedAccount.ShouldNotBeNull();
        updatedAccount.Balance.ShouldBe(100m); // Reset to initial balance
    }

    [Fact]
    public async Task DeleteAccountOperation_WithMultipleOperations_ShouldCascadeBalanceChanges()
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

        // Delete the first operation (should cascade to all subsequent operations)
        var firstOperationId = account.Operations.OrderBy(o => o.CreatedAt).First().Id;

        // Act
        var response = await ApiClient
            .LoggedAs(UserToken)
            .DeleteAsync($"{BaseEndpoint}/{account.Id}/operations/{firstOperationId}", CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<DeleteAccountOperationResponse>(CancellationToken);

        // Assert
        result.ShouldBeSuccessful();

        // Verify all operations have been updated correctly
        var updatedAccount = await DbContext
            .Accounts.IgnoreQueryFilters()
            .AsNoTracking()
            .Include(a => a.Operations)
            .FirstOrDefaultAsync(a => a.Id == account.Id, CancellationToken);

        var operations = updatedAccount!.Operations.OrderBy(o => o.CreatedAt).ToList();

        // First operation should be deleted
        operations[0].DeletedAt.ShouldNotBeNull();
        operations[0].NextBalance.ShouldBe(operations[0].PreviousBalance); // Deleted operation no longer contributes

        // Second operation: 100 + 30 = 130 (recalculated without first operation)
        operations[1].Amount.ShouldBe(30m);
        operations[1].PreviousBalance.ShouldBe(100m); // Updated to start from initial balance
        operations[1].NextBalance.ShouldBe(130m);
        operations[1].DeletedAt.ShouldBeNull();

        // Third operation: 130 + 20 = 150 (cascaded from second operation)
        operations[2].Amount.ShouldBe(20m);
        operations[2].PreviousBalance.ShouldBe(130m); // Updated from second operation
        operations[2].NextBalance.ShouldBe(150m);
        operations[2].DeletedAt.ShouldBeNull();

        // Account balance: 100 + 30 + 20 = 150 (without first operation)
        updatedAccount.Balance.ShouldBe(150m);
    }

    [Fact]
    public async Task DeleteAccountOperation_DeleteMiddleOperation_ShouldCascadeToSubsequentOperations()
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

        // Delete the middle operation
        var middleOperationId = operations[1].Id;

        // Act
        await ApiClient
            .LoggedAs(UserToken)
            .DeleteAsync($"{BaseEndpoint}/{account.Id}/operations/{middleOperationId}", CancellationToken);

        // Assert
        var updatedAccount = await DbContext
            .Accounts.IgnoreQueryFilters()
            .AsNoTracking()
            .Include(a => a.Operations)
            .FirstOrDefaultAsync(a => a.Id == account.Id, CancellationToken);

        var updatedOperations = updatedAccount!.Operations.OrderBy(o => o.CreatedAt).ToList();

        // First operation should remain unchanged
        updatedOperations[0].Amount.ShouldBe(50m);
        updatedOperations[0].PreviousBalance.ShouldBe(100m);
        updatedOperations[0].NextBalance.ShouldBe(150m);
        updatedOperations[0].DeletedAt.ShouldBeNull();

        // Second operation should be deleted
        updatedOperations[1].DeletedAt.ShouldNotBeNull();
        updatedOperations[1].NextBalance.ShouldBe(updatedOperations[1].PreviousBalance); // Deleted operation no longer contributes

        // Third operation should have cascaded balance update
        updatedOperations[2].Amount.ShouldBe(20m);
        updatedOperations[2].PreviousBalance.ShouldBe(150m); // Updated from first operation
        updatedOperations[2].NextBalance.ShouldBe(170m);
        updatedOperations[2].DeletedAt.ShouldBeNull();

        // Account balance should reflect deletion: 100 + 50 + 20 = 170
        updatedAccount.Balance.ShouldBe(170m);
    }

    [Fact]
    public async Task DeleteAccountOperation_DeleteLastOperation_ShouldNotAffectOthers()
    {
        // Arrange
        var account = AccountFixture.CreateValidAccount(User.Id, name: "Test Account", initialBalance: 100m);
        var now = DateTimeOffset.UtcNow;
        account.AddOperation("First", 50m, now);
        account.AddOperation("Second", 30m, now.AddMilliseconds(10));
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        var operations = account.Operations.OrderBy(o => o.CreatedAt).ToList();

        // Delete the last operation
        var lastOperationId = operations[1].Id;

        // Act
        await ApiClient
            .LoggedAs(UserToken)
            .DeleteAsync($"{BaseEndpoint}/{account.Id}/operations/{lastOperationId}", CancellationToken);

        // Assert
        var updatedAccount = await DbContext
            .Accounts.IgnoreQueryFilters()
            .AsNoTracking()
            .Include(a => a.Operations)
            .FirstOrDefaultAsync(a => a.Id == account.Id, CancellationToken);

        var updatedOperations = updatedAccount!.Operations.OrderBy(o => o.CreatedAt).ToList();

        // First operation should remain unchanged
        updatedOperations[0].Amount.ShouldBe(50m);
        updatedOperations[0].PreviousBalance.ShouldBe(100m);
        updatedOperations[0].NextBalance.ShouldBe(150m);
        updatedOperations[0].DeletedAt.ShouldBeNull();

        // Last operation should be deleted
        updatedOperations[1].DeletedAt.ShouldNotBeNull();
        updatedOperations[1].NextBalance.ShouldBe(updatedOperations[1].PreviousBalance); // Deleted operation no longer contributes

        // Account balance should reflect deletion: 100 + 50 = 150
        updatedAccount.Balance.ShouldBe(150m);
    }

    [Fact]
    public async Task DeleteAccountOperation_WithNonExistentOperation_ShouldReturnNotFound()
    {
        // Arrange
        var account = AccountFixture.CreateValidAccount(User.Id, name: "Test Account", initialBalance: 100m);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        var nonExistentOperationId = Guid.NewGuid();

        // Act
        var response = await ApiClient
            .LoggedAs(UserToken)
            .DeleteAsync($"{BaseEndpoint}/{account.Id}/operations/{nonExistentOperationId}", CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<DeleteAccountOperationResponse>(CancellationToken);

        // Assert
        result.ShouldBeProblem();
        result.Problem.ShouldNotBeNull();
        result.Problem.Status.ShouldBe(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task DeleteAccountOperation_WithNonExistentAccount_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistentAccountId = Guid.NewGuid();
        var nonExistentOperationId = Guid.NewGuid();

        // Act
        var response = await ApiClient
            .LoggedAs(UserToken)
            .DeleteAsync(
                $"{BaseEndpoint}/{nonExistentAccountId}/operations/{nonExistentOperationId}",
                CancellationToken
            );
        var result = await response.ReadResponseOrProblemAsync<DeleteAccountOperationResponse>(CancellationToken);

        // Assert
        result.ShouldBeProblem();
        result.Problem.ShouldNotBeNull();
        result.Problem.Status.ShouldBe(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task DeleteAccountOperation_FromAnotherUserAccount_ShouldReturnNotFound()
    {
        // Arrange
        var otherUser = UserFixture.CreateValidUser(login: "otheruser");
        DbContext.Users.Add(otherUser);
        await DbContext.SaveChangesAsync(CancellationToken);

        var otherAccount = AccountFixture.CreateValidAccount(otherUser.Id, name: "Other Account", initialBalance: 100m);
        DbContext.Accounts.Add(otherAccount);
        await DbContext.SaveChangesAsync(CancellationToken);

        // Create an operation for the other user's account
        otherAccount.AddOperation("Test", 50m, DateTimeOffset.UtcNow);
        await DbContext.SaveChangesAsync(CancellationToken);

        var operationId = otherAccount.Operations.First().Id;

        // Act - Try to delete operation from another user's account
        var response = await ApiClient
            .LoggedAs(UserToken)
            .DeleteAsync($"{BaseEndpoint}/{otherAccount.Id}/operations/{operationId}", CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<DeleteAccountOperationResponse>(CancellationToken);

        // Assert
        result.ShouldBeProblem();
        result.Problem.ShouldNotBeNull();
        result.Problem.Status.ShouldBe(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task DeleteAccountOperation_WithNegativeAmountOperation_ShouldUpdateCorrectly()
    {
        // Arrange
        var account = AccountFixture.CreateValidAccount(User.Id, name: "Test Account", initialBalance: 100m);
        var now = DateTimeOffset.UtcNow;
        account.AddOperation("Deposit", 50m, now);
        account.AddOperation("Withdrawal", -30m, now.AddMilliseconds(10));
        account.AddOperation("Another Deposit", 20m, now.AddMilliseconds(20));
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        // Balance should be: 100 + 50 - 30 + 20 = 140
        account.Balance.ShouldBe(140m);

        // Delete the withdrawal operation
        var operations = account.Operations.OrderBy(o => o.CreatedAt).ToList();
        var withdrawalOperationId = operations[1].Id;

        // Act
        await ApiClient
            .LoggedAs(UserToken)
            .DeleteAsync($"{BaseEndpoint}/{account.Id}/operations/{withdrawalOperationId}", CancellationToken);

        // Assert
        var updatedAccount = await DbContext
            .Accounts.IgnoreQueryFilters()
            .AsNoTracking()
            .Include(a => a.Operations)
            .FirstOrDefaultAsync(a => a.Id == account.Id, CancellationToken);

        var updatedOperations = updatedAccount!.Operations.OrderBy(o => o.CreatedAt).ToList();

        // First operation unchanged
        updatedOperations[0].Amount.ShouldBe(50m);
        updatedOperations[0].NextBalance.ShouldBe(150m);
        updatedOperations[0].DeletedAt.ShouldBeNull();

        // Second operation deleted
        updatedOperations[1].DeletedAt.ShouldNotBeNull();
        updatedOperations[1].NextBalance.ShouldBe(updatedOperations[1].PreviousBalance); // Deleted operation no longer contributes

        // Third operation recalculated: 150 + 20 = 170
        updatedOperations[2].PreviousBalance.ShouldBe(150m);
        updatedOperations[2].NextBalance.ShouldBe(170m);
        updatedOperations[2].DeletedAt.ShouldBeNull();

        // Account balance: 100 + 50 + 20 = 170 (without withdrawal)
        updatedAccount.Balance.ShouldBe(170m);
    }
}
