using System.Net.Http.Json;
using Application.Features.Accounts.Commands.RenameAccountOperation;
using Domain.AccountOperations;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TestFixtures.Domain;

namespace Api.Tests.Accounts;

[Collection(ApiTestCollection.CollectionName)]
public class RenameAccountOperationTests : ApiTestBase
{
    private const string Endpoint = "/accounts";

    public RenameAccountOperationTests(ApiTestFixture factory)
        : base(factory) { }

    [Fact]
    public async Task RenameAccountOperation_WithValidData_ShouldUpdateOperationDescription()
    {
        // Arrange
        var account = AccountFixture.CreateValidAccount(User.Id, name: "Test Account", initialBalance: 100m);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        account.AddOperation("Original Description", 50m, DateTimeOffset.UtcNow);
        await DbContext.SaveChangesAsync(CancellationToken);

        var operation = account.Operations.First();
        var originalCreatedAt = operation.CreatedAt;

        var renameCommand = new RenameAccountOperationCommand
        {
            AccountId = account.Id,
            OperationId = operation.Id,
            Description = "Updated Description",
        };

        // Act
        var response = await ApiClient
            .LoggedAs(UserToken)
            .PatchAsJsonAsync($"{Endpoint}/{account.Id}/operations/{operation.Id}", renameCommand, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<RenameAccountOperationResponse>(CancellationToken);

        // Assert
        result.ShouldBeSuccessful();
        result.Response.ShouldNotBeNull();
        result.Response.Id.ShouldBe(operation.Id);
        result.Response.AccountId.ShouldBe(account.Id);
        result.Response.Description.ShouldBe(renameCommand.Description);
        result.Response.Amount.ShouldBe(50m);
        result.Response.PreviousBalance.ShouldBe(100m);
        result.Response.NextBalance.ShouldBe(150m);
        result.Response.CreatedAt.ShouldBe(originalCreatedAt);
        result.Response.UpdatedAt.ShouldBeGreaterThan(result.Response.CreatedAt);
        result.Response.UpdatedAt.ShouldBeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task RenameAccountOperation_WithEmptyDescription_ShouldReturnValidationError()
    {
        // Arrange
        var account = AccountFixture.CreateValidAccount(User.Id, name: "Test Account", initialBalance: 100m);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        account.AddOperation("Original Description", 50m, DateTimeOffset.UtcNow);
        await DbContext.SaveChangesAsync(CancellationToken);

        var operation = account.Operations.First();

        var renameCommand = new RenameAccountOperationCommand
        {
            AccountId = account.Id,
            OperationId = operation.Id,
            Description = "",
        };

        // Act
        var response = await ApiClient
            .LoggedAs(UserToken)
            .PatchAsJsonAsync($"{Endpoint}/{account.Id}/operations/{operation.Id}", renameCommand, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<RenameAccountOperationResponse>(CancellationToken);

        // Assert
        result.ShouldBeProblem();
        result.Problem.ShouldNotBeNull();
        result.Problem.Status.ShouldBe(StatusCodes.Status400BadRequest);
        result.Problem.ShouldHaveValidationError(
            "Description",
            AccountOperationErrors.AccountOperationDescriptionRequired.Code
        );
    }

    [Fact]
    public async Task RenameAccountOperation_WithWhitespaceDescription_ShouldReturnValidationError()
    {
        // Arrange
        var account = AccountFixture.CreateValidAccount(User.Id, name: "Test Account", initialBalance: 100m);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        account.AddOperation("Original Description", 50m, DateTimeOffset.UtcNow);
        await DbContext.SaveChangesAsync(CancellationToken);

        var operation = account.Operations.First();

        var renameCommand = new RenameAccountOperationCommand
        {
            AccountId = account.Id,
            OperationId = operation.Id,
            Description = "   ",
        };

        // Act
        var response = await ApiClient
            .LoggedAs(UserToken)
            .PatchAsJsonAsync($"{Endpoint}/{account.Id}/operations/{operation.Id}", renameCommand, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<RenameAccountOperationResponse>(CancellationToken);

        // Assert
        result.ShouldBeProblem();
        result.Problem.ShouldNotBeNull();
        result.Problem.Status.ShouldBe(StatusCodes.Status400BadRequest);
        result.Problem.ShouldHaveValidationError(
            "Description",
            AccountOperationErrors.AccountOperationDescriptionRequired.Code
        );
    }

    [Fact]
    public async Task RenameAccountOperation_WithTooLongDescription_ShouldReturnValidationError()
    {
        // Arrange
        var account = AccountFixture.CreateValidAccount(User.Id, name: "Test Account", initialBalance: 100m);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        account.AddOperation("Original Description", 50m, DateTimeOffset.UtcNow);
        await DbContext.SaveChangesAsync(CancellationToken);

        var operation = account.Operations.First();

        var renameCommand = new RenameAccountOperationCommand
        {
            AccountId = account.Id,
            OperationId = operation.Id,
            Description = new string('a', AccountOperationConstants.MaxDescriptionLength + 1),
        };

        // Act
        var response = await ApiClient
            .LoggedAs(UserToken)
            .PatchAsJsonAsync($"{Endpoint}/{account.Id}/operations/{operation.Id}", renameCommand, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<RenameAccountOperationResponse>(CancellationToken);

        // Assert
        result.ShouldBeProblem();
        result.Problem.ShouldNotBeNull();
        result.Problem.Status.ShouldBe(StatusCodes.Status400BadRequest);
        result.Problem.ShouldHaveValidationError(
            "Description",
            AccountOperationErrors.AccountOperationDescriptionTooLong.Code
        );
    }

    [Fact]
    public async Task RenameAccountOperation_WithNonExistentOperationId_ShouldReturnNotFound()
    {
        // Arrange
        var account = AccountFixture.CreateValidAccount(User.Id, name: "Test Account", initialBalance: 100m);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        var nonExistentOperationId = Guid.NewGuid();

        var renameCommand = new RenameAccountOperationCommand
        {
            AccountId = account.Id,
            OperationId = nonExistentOperationId,
            Description = "Updated Description",
        };

        // Act
        var response = await ApiClient
            .LoggedAs(UserToken)
            .PatchAsJsonAsync(
                $"{Endpoint}/{account.Id}/operations/{nonExistentOperationId}",
                renameCommand,
                CancellationToken
            );
        var result = await response.ReadResponseOrProblemAsync<RenameAccountOperationResponse>(CancellationToken);

        // Assert
        result.ShouldBeProblem();
        result.Problem.ShouldNotBeNull();
        result.Problem.Status.ShouldBe(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task RenameAccountOperation_WithNonExistentAccountId_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistentAccountId = Guid.NewGuid();
        var nonExistentOperationId = Guid.NewGuid();

        var renameCommand = new RenameAccountOperationCommand
        {
            AccountId = nonExistentAccountId,
            OperationId = nonExistentOperationId,
            Description = "Updated Description",
        };

        // Act
        var response = await ApiClient
            .LoggedAs(UserToken)
            .PatchAsJsonAsync(
                $"{Endpoint}/{nonExistentAccountId}/operations/{nonExistentOperationId}",
                renameCommand,
                CancellationToken
            );
        var result = await response.ReadResponseOrProblemAsync<RenameAccountOperationResponse>(CancellationToken);

        // Assert
        result.ShouldBeProblem();
        result.Problem.ShouldNotBeNull();
        result.Problem.Status.ShouldBe(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task RenameAccountOperation_WithMismatchedAccountId_ShouldReturnNotFound()
    {
        // Arrange
        var account1 = AccountFixture.CreateValidAccount(User.Id, name: "Account 1", initialBalance: 100m);
        var account2 = AccountFixture.CreateValidAccount(User.Id, name: "Account 2", initialBalance: 200m);
        DbContext.Accounts.Add(account1);
        DbContext.Accounts.Add(account2);
        await DbContext.SaveChangesAsync(CancellationToken);

        account1.AddOperation("Operation on Account 1", 50m, DateTimeOffset.UtcNow);
        await DbContext.SaveChangesAsync(CancellationToken);

        var operation = account1.Operations.First();

        var renameCommand = new RenameAccountOperationCommand
        {
            AccountId = account2.Id, // Different account
            OperationId = operation.Id,
            Description = "Updated Description",
        };

        // Act
        var response = await ApiClient
            .LoggedAs(UserToken)
            .PatchAsJsonAsync($"{Endpoint}/{account2.Id}/operations/{operation.Id}", renameCommand, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<RenameAccountOperationResponse>(CancellationToken);

        // Assert
        result.ShouldBeProblem();
        result.Problem.ShouldNotBeNull();
        result.Problem.Status.ShouldBe(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task RenameAccountOperation_ShouldPersistInDatabase()
    {
        // Arrange
        var account = AccountFixture.CreateValidAccount(User.Id, name: "Test Account", initialBalance: 100m);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        account.AddOperation("Original Description", 75m, DateTimeOffset.UtcNow);
        await DbContext.SaveChangesAsync(CancellationToken);

        var operation = account.Operations.First();
        var originalCreatedAt = operation.CreatedAt;

        var renameCommand = new RenameAccountOperationCommand
        {
            AccountId = account.Id,
            OperationId = operation.Id,
            Description = "Updated Description",
        };

        // Act
        var response = await ApiClient
            .LoggedAs(UserToken)
            .PatchAsJsonAsync($"{Endpoint}/{account.Id}/operations/{operation.Id}", renameCommand, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<RenameAccountOperationResponse>(CancellationToken);

        // Assert
        result.ShouldBeSuccessful();
        result.Response.ShouldNotBeNull();

        var operationInDb = await DbContext
            .AccountOperations.AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == operation.Id, CancellationToken);

        operationInDb.ShouldNotBeNull();
        operationInDb.Description.ShouldBe(renameCommand.Description);
        operationInDb.Amount.ShouldBe(75m);
        operationInDb.PreviousBalance.ShouldBe(100m);
        operationInDb.NextBalance.ShouldBe(175m);
        operationInDb.AccountId.ShouldBe(account.Id);
        operationInDb.CreatedAt.ShouldBe(originalCreatedAt);
        operationInDb.UpdatedAt.ShouldBeGreaterThan(operationInDb.CreatedAt);
    }

    [Fact]
    public async Task RenameAccountOperation_ShouldNotChangeAmountOrBalance()
    {
        // Arrange
        var account = AccountFixture.CreateValidAccount(User.Id, name: "Test Account", initialBalance: 100m);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        account.AddOperation("Original Description", 50m, DateTimeOffset.UtcNow);
        await DbContext.SaveChangesAsync(CancellationToken);

        var operation = account.Operations.First();

        var renameCommand = new RenameAccountOperationCommand
        {
            AccountId = account.Id,
            OperationId = operation.Id,
            Description = "New Description",
        };

        // Act
        var response = await ApiClient
            .LoggedAs(UserToken)
            .PatchAsJsonAsync($"{Endpoint}/{account.Id}/operations/{operation.Id}", renameCommand, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<RenameAccountOperationResponse>(CancellationToken);

        // Assert
        result.ShouldBeSuccessful();
        result.Response.ShouldNotBeNull();
        result.Response.Amount.ShouldBe(50m);
        result.Response.PreviousBalance.ShouldBe(100m);
        result.Response.NextBalance.ShouldBe(150m);

        var accountInDb = await DbContext
            .Accounts.AsNoTracking()
            .Include(a => a.Operations)
            .FirstOrDefaultAsync(a => a.Id == account.Id, CancellationToken);

        accountInDb.ShouldNotBeNull();
        accountInDb.Balance.ShouldBe(150m); // Should remain unchanged
    }

    [Fact]
    public async Task RenameAccountOperation_WithOperationFromOtherUser_ShouldReturnNotFound()
    {
        // Arrange
        var otherUser = UserFixture.CreateValidUser(login: "otheruser");
        DbContext.Users.Add(otherUser);
        await DbContext.SaveChangesAsync(CancellationToken);

        var otherUserAccount = AccountFixture.CreateValidAccount(
            otherUser.Id,
            name: "Other User Account",
            initialBalance: 100m
        );
        DbContext.Accounts.Add(otherUserAccount);
        await DbContext.SaveChangesAsync(CancellationToken);

        otherUserAccount.AddOperation("Other User Operation", 50m, DateTimeOffset.UtcNow);
        await DbContext.SaveChangesAsync(CancellationToken);

        var operation = otherUserAccount.Operations.First();

        var renameCommand = new RenameAccountOperationCommand
        {
            AccountId = otherUserAccount.Id,
            OperationId = operation.Id,
            Description = "Trying to update",
        };

        // Act
        var response = await ApiClient
            .LoggedAs(UserToken)
            .PatchAsJsonAsync(
                $"{Endpoint}/{otherUserAccount.Id}/operations/{operation.Id}",
                renameCommand,
                CancellationToken
            );
        var result = await response.ReadResponseOrProblemAsync<RenameAccountOperationResponse>(CancellationToken);

        // Assert
        result.ShouldBeProblem();
        result.Problem.ShouldNotBeNull();
        result.Problem.Status.ShouldBe(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task RenameAccountOperation_WithMultipleOperations_ShouldOnlyRenameSpecifiedOne()
    {
        // Arrange
        var account = AccountFixture.CreateValidAccount(User.Id, name: "Test Account", initialBalance: 100m);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        account.AddOperation("Operation 1", 10m, DateTimeOffset.UtcNow);
        account.AddOperation("Operation 2", 20m, DateTimeOffset.UtcNow);
        account.AddOperation("Operation 3", 30m, DateTimeOffset.UtcNow);
        await DbContext.SaveChangesAsync(CancellationToken);

        var targetOperation = account.Operations.ElementAt(1); // Second operation

        var renameCommand = new RenameAccountOperationCommand
        {
            AccountId = account.Id,
            OperationId = targetOperation.Id,
            Description = "Updated Operation 2",
        };

        // Act
        var response = await ApiClient
            .LoggedAs(UserToken)
            .PatchAsJsonAsync(
                $"{Endpoint}/{account.Id}/operations/{targetOperation.Id}",
                renameCommand,
                CancellationToken
            );
        var result = await response.ReadResponseOrProblemAsync<RenameAccountOperationResponse>(CancellationToken);

        // Assert
        result.ShouldBeSuccessful();

        var accountInDb = await DbContext
            .Accounts.AsNoTracking()
            .Include(a => a.Operations)
            .FirstOrDefaultAsync(a => a.Id == account.Id, CancellationToken);

        accountInDb.ShouldNotBeNull();
        accountInDb.Operations.Count.ShouldBe(3);

        var operationsInDb = accountInDb.Operations.OrderBy(o => o.CreatedAt).ToList();
        operationsInDb[0].Description.ShouldBe("Operation 1");
        operationsInDb[1].Description.ShouldBe("Updated Operation 2");
        operationsInDb[2].Description.ShouldBe("Operation 3");
    }
}
