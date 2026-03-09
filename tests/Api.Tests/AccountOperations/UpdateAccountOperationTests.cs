using System.Net.Http.Json;
using Api.Contracts.AccountOperations;
using Application.Features.AccountOperations.Commands.UpdateAccountOperation;
using Domain.AccountOperations;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TestFixtures.Domain;

namespace Api.Tests.AccountOperations;

[Collection(ApiTestCollection.CollectionName)]
public class UpdateAccountOperationTests : ApiTestBase
{
    private const string BaseEndpoint = "/accounts";

    public UpdateAccountOperationTests(ApiTestFixture factory)
        : base(factory) { }

    [Fact]
    public async Task UpdateAccountOperation_WithValidData_ShouldUpdateDescriptionAndAmount()
    {
        var account = AccountFixture.CreateValidAccount(User.Id, name: "Test Account", initialBalance: 100m);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        account.AddOperation("Original Description", 50m, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);
        await DbContext.SaveChangesAsync(CancellationToken);

        var operation = account.Operations[0];
        var originalCreatedAt = operation.CreatedAt;

        var updateRequest = new UpdateAccountOperationRequest(75m, "Updated Description");

        var response = await ApiClient
            .LoggedAs(UserToken)
            .PutAsJsonAsync($"{BaseEndpoint}/{account.Id}/operations/{operation.Id}", updateRequest, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<UpdateAccountOperationResponse>(CancellationToken);

        result.ShouldBeSuccessful();
        result.Response.ShouldNotBeNull();
        result.Response.Id.ShouldBe(operation.Id);
        result.Response.AccountId.ShouldBe(account.Id);
        result.Response.AccountName.ShouldBe("Test Account");
        result.Response.Description.ShouldBe("Updated Description");
        result.Response.Amount.ShouldBe(75m);
        result.Response.PreviousBalance.ShouldBe(100m);
        result.Response.NextBalance.ShouldBe(175m);
        result.Response.CreatedAt.ShouldBeCloseTo(originalCreatedAt, TimeSpan.FromMilliseconds(1));
        result.Response.UpdatedAt.ShouldBeGreaterThan(result.Response.CreatedAt);
        result.Response.UpdatedAt.ShouldBeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));

        var updatedAccount = await DbContext
            .Accounts.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == account.Id, CancellationToken);
        updatedAccount.ShouldNotBeNull();
        updatedAccount.Balance.ShouldBe(175m);
    }

    [Fact]
    public async Task UpdateAccountOperation_WithDescriptionOnlyChange_ShouldNotChangeAmountOrBalance()
    {
        var account = AccountFixture.CreateValidAccount(User.Id, name: "Test Account", initialBalance: 100m);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        account.AddOperation("Original Description", 50m, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);
        await DbContext.SaveChangesAsync(CancellationToken);

        var operation = account.Operations[0];

        var updateRequest = new UpdateAccountOperationRequest(50m, "New Description");

        var response = await ApiClient
            .LoggedAs(UserToken)
            .PutAsJsonAsync($"{BaseEndpoint}/{account.Id}/operations/{operation.Id}", updateRequest, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<UpdateAccountOperationResponse>(CancellationToken);

        result.ShouldBeSuccessful();
        result.Response.ShouldNotBeNull();
        result.Response.AccountName.ShouldBe("Test Account");
        result.Response.Description.ShouldBe("New Description");
        result.Response.Amount.ShouldBe(50m);
        result.Response.PreviousBalance.ShouldBe(100m);
        result.Response.NextBalance.ShouldBe(150m);

        var accountInDb = await DbContext
            .Accounts.AsNoTracking()
            .Include(a => a.Operations)
            .FirstOrDefaultAsync(a => a.Id == account.Id, CancellationToken);

        accountInDb.ShouldNotBeNull();
        accountInDb.Balance.ShouldBe(150m);
    }

    [Fact]
    public async Task UpdateAccountOperation_WithNegativeAmount_ShouldUpdateCorrectly()
    {
        var account = AccountFixture.CreateValidAccount(User.Id, name: "Test Account", initialBalance: 100m);
        account.AddOperation("Initial Operation", 50m, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        var operationId = account.Operations[0].Id;
        var updateRequest = new UpdateAccountOperationRequest(-30m, "Initial Operation");

        var response = await ApiClient
            .LoggedAs(UserToken)
            .PutAsJsonAsync($"{BaseEndpoint}/{account.Id}/operations/{operationId}", updateRequest, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<UpdateAccountOperationResponse>(CancellationToken);

        result.ShouldBeSuccessful();
        result.Response.ShouldNotBeNull();
        result.Response.AccountName.ShouldBe("Test Account");
        result.Response.Amount.ShouldBe(-30m);
        result.Response.NextBalance.ShouldBe(70m);

        var updatedAccount = await DbContext
            .Accounts.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == account.Id, CancellationToken);
        updatedAccount!.Balance.ShouldBe(70m);
    }

    [Fact]
    public async Task UpdateAccountOperation_WithMultipleOperations_ShouldCascadeBalanceChanges()
    {
        var account = AccountFixture.CreateValidAccount(User.Id, name: "Test Account", initialBalance: 100m);
        var now = DateTimeOffset.UtcNow;
        account.AddOperation("First", 50m, now, now);
        account.AddOperation("Second", 30m, now.AddMilliseconds(10), now.AddMilliseconds(10));
        account.AddOperation("Third", 20m, now.AddMilliseconds(20), now.AddMilliseconds(20));
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        account.Balance.ShouldBe(200m);

        var firstOperationId = account.Operations.OrderBy(o => o.CreatedAt).First().Id;
        var updateRequest = new UpdateAccountOperationRequest(100m, "First");

        var response = await ApiClient
            .LoggedAs(UserToken)
            .PutAsJsonAsync(
                $"{BaseEndpoint}/{account.Id}/operations/{firstOperationId}",
                updateRequest,
                CancellationToken
            );
        var result = await response.ReadResponseOrProblemAsync<UpdateAccountOperationResponse>(CancellationToken);

        result.ShouldBeSuccessful();

        var updatedAccount = await DbContext
            .Accounts.AsNoTracking()
            .Include(a => a.Operations)
            .FirstOrDefaultAsync(a => a.Id == account.Id, CancellationToken);

        var updatedOperations = updatedAccount!.Operations.OrderBy(o => o.CreatedAt).ToList();

        updatedOperations[0].Amount.ShouldBe(100m);
        updatedOperations[0].PreviousBalance.ShouldBe(100m);
        updatedOperations[0].NextBalance.ShouldBe(200m);

        updatedOperations[1].Amount.ShouldBe(30m);
        updatedOperations[1].PreviousBalance.ShouldBe(200m);
        updatedOperations[1].NextBalance.ShouldBe(230m);

        updatedOperations[2].Amount.ShouldBe(20m);
        updatedOperations[2].PreviousBalance.ShouldBe(230m);
        updatedOperations[2].NextBalance.ShouldBe(250m);

        updatedAccount.Balance.ShouldBe(250m);
    }

    [Fact]
    public async Task UpdateAccountOperation_UpdateMiddleOperation_ShouldCascadeToSubsequentOperations()
    {
        var account = AccountFixture.CreateValidAccount(User.Id, name: "Test Account", initialBalance: 100m);
        var now = DateTimeOffset.UtcNow;
        account.AddOperation("First", 50m, now, now);
        account.AddOperation("Second", 30m, now.AddMilliseconds(10), now.AddMilliseconds(10));
        account.AddOperation("Third", 20m, now.AddMilliseconds(20), now.AddMilliseconds(20));
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        var operations = account.Operations.OrderBy(o => o.CreatedAt).ToList();

        var middleOperationId = operations[1].Id;
        var updateRequest = new UpdateAccountOperationRequest(80m, "Second");

        await ApiClient
            .LoggedAs(UserToken)
            .PutAsJsonAsync(
                $"{BaseEndpoint}/{account.Id}/operations/{middleOperationId}",
                updateRequest,
                CancellationToken
            );

        var updatedAccount = await DbContext
            .Accounts.AsNoTracking()
            .Include(a => a.Operations)
            .FirstOrDefaultAsync(a => a.Id == account.Id, CancellationToken);

        var updatedOperations = updatedAccount!.Operations.OrderBy(o => o.CreatedAt).ToList();

        updatedOperations[0].Amount.ShouldBe(50m);
        updatedOperations[0].PreviousBalance.ShouldBe(100m);
        updatedOperations[0].NextBalance.ShouldBe(150m);

        updatedOperations[1].Amount.ShouldBe(80m);
        updatedOperations[1].PreviousBalance.ShouldBe(150m);
        updatedOperations[1].NextBalance.ShouldBe(230m);

        updatedOperations[2].Amount.ShouldBe(20m);
        updatedOperations[2].PreviousBalance.ShouldBe(230m);
        updatedOperations[2].NextBalance.ShouldBe(250m);

        updatedAccount.Balance.ShouldBe(250m);
    }

    [Fact]
    public async Task UpdateAccountOperation_UpdateLastOperation_ShouldNotAffectOthers()
    {
        var account = AccountFixture.CreateValidAccount(User.Id, name: "Test Account", initialBalance: 100m);
        var now = DateTimeOffset.UtcNow;
        account.AddOperation("First", 50m, now, now);
        account.AddOperation("Second", 30m, now.AddMilliseconds(10), now.AddMilliseconds(10));
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        var operations = account.Operations.OrderBy(o => o.CreatedAt).ToList();

        var lastOperationId = operations[1].Id;
        var updateRequest = new UpdateAccountOperationRequest(100m, "Second");

        await ApiClient
            .LoggedAs(UserToken)
            .PutAsJsonAsync(
                $"{BaseEndpoint}/{account.Id}/operations/{lastOperationId}",
                updateRequest,
                CancellationToken
            );

        var updatedAccount = await DbContext
            .Accounts.AsNoTracking()
            .Include(a => a.Operations)
            .FirstOrDefaultAsync(a => a.Id == account.Id, CancellationToken);

        var updatedOperations = updatedAccount!.Operations.OrderBy(o => o.CreatedAt).ToList();

        updatedOperations[0].Amount.ShouldBe(50m);
        updatedOperations[0].PreviousBalance.ShouldBe(100m);
        updatedOperations[0].NextBalance.ShouldBe(150m);

        updatedOperations[1].Amount.ShouldBe(100m);
        updatedOperations[1].PreviousBalance.ShouldBe(150m);
        updatedOperations[1].NextBalance.ShouldBe(250m);

        updatedAccount.Balance.ShouldBe(250m);
    }

    [Fact]
    public async Task UpdateAccountOperation_WithZeroAmount_ShouldUpdateCorrectly()
    {
        var account = AccountFixture.CreateValidAccount(User.Id, name: "Test Account", initialBalance: 100m);
        account.AddOperation("Test Operation", 50m, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        var operationId = account.Operations[0].Id;
        var updateRequest = new UpdateAccountOperationRequest(0m, "Test Operation");

        var response = await ApiClient
            .LoggedAs(UserToken)
            .PutAsJsonAsync($"{BaseEndpoint}/{account.Id}/operations/{operationId}", updateRequest, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<UpdateAccountOperationResponse>(CancellationToken);

        result.ShouldBeSuccessful();
        result.Response.ShouldNotBeNull();
        result.Response.AccountName.ShouldBe("Test Account");
        result.Response.Amount.ShouldBe(0m);
        result.Response.NextBalance.ShouldBe(100m);

        var updatedAccount = await DbContext
            .Accounts.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == account.Id, CancellationToken);
        updatedAccount!.Balance.ShouldBe(100m);
    }

    [Fact]
    public async Task UpdateAccountOperation_WithEmptyDescription_ShouldReturnValidationError()
    {
        var account = AccountFixture.CreateValidAccount(User.Id, name: "Test Account", initialBalance: 100m);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        account.AddOperation("Original Description", 50m, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);
        await DbContext.SaveChangesAsync(CancellationToken);

        var operation = account.Operations[0];

        var updateRequest = new UpdateAccountOperationRequest(50m, "");

        var response = await ApiClient
            .LoggedAs(UserToken)
            .PutAsJsonAsync($"{BaseEndpoint}/{account.Id}/operations/{operation.Id}", updateRequest, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<UpdateAccountOperationResponse>(CancellationToken);

        result.ShouldBeProblem();
        result.Problem.ShouldNotBeNull();
        result.Problem.Status.ShouldBe(StatusCodes.Status400BadRequest);
        result.Problem.ShouldHaveError(AccountOperationErrors.AccountOperationDescriptionRequired.Code);
    }

    [Fact]
    public async Task UpdateAccountOperation_WithWhitespaceDescription_ShouldReturnValidationError()
    {
        var account = AccountFixture.CreateValidAccount(User.Id, name: "Test Account", initialBalance: 100m);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        account.AddOperation("Original Description", 50m, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);
        await DbContext.SaveChangesAsync(CancellationToken);

        var operation = account.Operations[0];

        var updateRequest = new UpdateAccountOperationRequest(50m, "   ");

        var response = await ApiClient
            .LoggedAs(UserToken)
            .PutAsJsonAsync($"{BaseEndpoint}/{account.Id}/operations/{operation.Id}", updateRequest, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<UpdateAccountOperationResponse>(CancellationToken);

        result.ShouldBeProblem();
        result.Problem.ShouldNotBeNull();
        result.Problem.Status.ShouldBe(StatusCodes.Status400BadRequest);
        result.Problem.ShouldHaveError(AccountOperationErrors.AccountOperationDescriptionRequired.Code);
    }

    [Fact]
    public async Task UpdateAccountOperation_WithTooLongDescription_ShouldReturnValidationError()
    {
        var account = AccountFixture.CreateValidAccount(User.Id, name: "Test Account", initialBalance: 100m);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        account.AddOperation("Original Description", 50m, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);
        await DbContext.SaveChangesAsync(CancellationToken);

        var operation = account.Operations[0];

        var updateRequest = new UpdateAccountOperationRequest(
            50m,
            new string('a', AccountOperationConstants.MaxDescriptionLength + 1)
        );

        var response = await ApiClient
            .LoggedAs(UserToken)
            .PutAsJsonAsync($"{BaseEndpoint}/{account.Id}/operations/{operation.Id}", updateRequest, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<UpdateAccountOperationResponse>(CancellationToken);

        result.ShouldBeProblem();
        result.Problem.ShouldNotBeNull();
        result.Problem.Status.ShouldBe(StatusCodes.Status400BadRequest);
        result.Problem.ShouldHaveError(AccountOperationErrors.AccountOperationDescriptionTooLong.Code);
    }

    [Fact]
    public async Task UpdateAccountOperation_WithNonExistentOperation_ShouldReturnNotFound()
    {
        var account = AccountFixture.CreateValidAccount(User.Id, name: "Test Account", initialBalance: 100m);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        var nonExistentOperationId = Guid.NewGuid();
        var updateRequest = new UpdateAccountOperationRequest(100m, "Updated Description");

        var response = await ApiClient
            .LoggedAs(UserToken)
            .PutAsJsonAsync(
                $"{BaseEndpoint}/{account.Id}/operations/{nonExistentOperationId}",
                updateRequest,
                CancellationToken
            );
        var result = await response.ReadResponseOrProblemAsync<UpdateAccountOperationResponse>(CancellationToken);

        result.ShouldBeProblem();
        result.Problem.ShouldNotBeNull();
        result.Problem.Status.ShouldBe(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task UpdateAccountOperation_WithNonExistentAccount_ShouldReturnNotFound()
    {
        var nonExistentAccountId = Guid.NewGuid();
        var nonExistentOperationId = Guid.NewGuid();
        var updateRequest = new UpdateAccountOperationRequest(100m, "Updated Description");

        var response = await ApiClient
            .LoggedAs(UserToken)
            .PutAsJsonAsync(
                $"{BaseEndpoint}/{nonExistentAccountId}/operations/{nonExistentOperationId}",
                updateRequest,
                CancellationToken
            );
        var result = await response.ReadResponseOrProblemAsync<UpdateAccountOperationResponse>(CancellationToken);

        result.ShouldBeProblem();
        result.Problem.ShouldNotBeNull();
        result.Problem.Status.ShouldBe(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task UpdateAccountOperation_WithMismatchedAccountId_ShouldReturnNotFound()
    {
        var account1 = AccountFixture.CreateValidAccount(User.Id, name: "Account 1", initialBalance: 100m);
        var account2 = AccountFixture.CreateValidAccount(User.Id, name: "Account 2", initialBalance: 200m);
        DbContext.Accounts.Add(account1);
        DbContext.Accounts.Add(account2);
        await DbContext.SaveChangesAsync(CancellationToken);

        account1.AddOperation("Operation on Account 1", 50m, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);
        await DbContext.SaveChangesAsync(CancellationToken);

        var operation = account1.Operations[0];

        var updateRequest = new UpdateAccountOperationRequest(50m, "Updated Description");

        var response = await ApiClient
            .LoggedAs(UserToken)
            .PutAsJsonAsync(
                $"{BaseEndpoint}/{account2.Id}/operations/{operation.Id}",
                updateRequest,
                CancellationToken
            );
        var result = await response.ReadResponseOrProblemAsync<UpdateAccountOperationResponse>(CancellationToken);

        result.ShouldBeProblem();
        result.Problem.ShouldNotBeNull();
        result.Problem.Status.ShouldBe(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task UpdateAccountOperation_FromAnotherUserAccount_ShouldReturnNotFound()
    {
        var otherUser = UserFixture.CreateValidUser(login: "otheruser");
        DbContext.Users.Add(otherUser);
        await DbContext.SaveChangesAsync(CancellationToken);

        var otherAccount = AccountFixture.CreateValidAccount(otherUser.Id, name: "Other Account", initialBalance: 100m);
        DbContext.Accounts.Add(otherAccount);
        await DbContext.SaveChangesAsync(CancellationToken);

        otherAccount.AddOperation("Test", 50m, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);
        await DbContext.SaveChangesAsync(CancellationToken);

        var operationId = otherAccount.Operations[0].Id;
        var updateRequest = new UpdateAccountOperationRequest(100m, "Trying to update");

        var response = await ApiClient
            .LoggedAs(UserToken)
            .PutAsJsonAsync(
                $"{BaseEndpoint}/{otherAccount.Id}/operations/{operationId}",
                updateRequest,
                CancellationToken
            );
        var result = await response.ReadResponseOrProblemAsync<UpdateAccountOperationResponse>(CancellationToken);

        result.ShouldBeProblem();
        result.Problem.ShouldNotBeNull();
        result.Problem.Status.ShouldBe(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task UpdateAccountOperation_ShouldPersistInDatabase()
    {
        var account = AccountFixture.CreateValidAccount(User.Id, name: "Test Account", initialBalance: 100m);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        account.AddOperation("Original Description", 75m, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);
        await DbContext.SaveChangesAsync(CancellationToken);

        var operation = account.Operations[0];
        var originalCreatedAt = operation.CreatedAt;

        var updateRequest = new UpdateAccountOperationRequest(75m, "Updated Description");

        var response = await ApiClient
            .LoggedAs(UserToken)
            .PutAsJsonAsync($"{BaseEndpoint}/{account.Id}/operations/{operation.Id}", updateRequest, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<UpdateAccountOperationResponse>(CancellationToken);

        result.ShouldBeSuccessful();
        result.Response.ShouldNotBeNull();

        var operationInDb = await DbContext
            .AccountOperations.AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == operation.Id, CancellationToken);

        operationInDb.ShouldNotBeNull();
        operationInDb.Description.ShouldBe(updateRequest.Description);
        operationInDb.Amount.ShouldBe(75m);
        operationInDb.PreviousBalance.ShouldBe(100m);
        operationInDb.NextBalance.ShouldBe(175m);
        operationInDb.AccountId.ShouldBe(account.Id);
        operationInDb.CreatedAt.ShouldBeCloseTo(originalCreatedAt, TimeSpan.FromMilliseconds(1));
        operationInDb.UpdatedAt.ShouldBeGreaterThan(operationInDb.CreatedAt);
    }

    [Fact]
    public async Task UpdateAccountOperation_WithMultipleOperations_ShouldOnlyUpdateSpecifiedOne()
    {
        var account = AccountFixture.CreateValidAccount(User.Id, name: "Test Account", initialBalance: 100m);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        var now = DateTimeOffset.UtcNow;
        account.AddOperation("Operation 1", 10m, now, now);
        account.AddOperation("Operation 2", 20m, now.AddMilliseconds(10), now.AddMilliseconds(10));
        account.AddOperation("Operation 3", 30m, now.AddMilliseconds(20), now.AddMilliseconds(20));
        await DbContext.SaveChangesAsync(CancellationToken);

        var targetOperation = account.Operations.OrderBy(o => o.CreatedAt).ElementAt(1);

        var updateRequest = new UpdateAccountOperationRequest(20m, "Updated Operation 2");

        var response = await ApiClient
            .LoggedAs(UserToken)
            .PutAsJsonAsync(
                $"{BaseEndpoint}/{account.Id}/operations/{targetOperation.Id}",
                updateRequest,
                CancellationToken
            );
        var result = await response.ReadResponseOrProblemAsync<UpdateAccountOperationResponse>(CancellationToken);

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

    [Fact]
    public async Task UpdateAccountOperation_WithExplicitOperationDate_ShouldUseProvidedTimestamp()
    {
        var account = AccountFixture.CreateValidAccount(User.Id, name: "Test Account", initialBalance: 100m);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        account.AddOperation("Original Description", 50m, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);
        await DbContext.SaveChangesAsync(CancellationToken);

        var operation = account.Operations[0];
        var originalOperationDate = operation.OperationDate;

        var explicitOperationDate = new DateTimeOffset(2025, 6, 15, 10, 30, 0, TimeSpan.Zero);
        var updateRequest = new UpdateAccountOperationRequest(75m, "Updated Description", explicitOperationDate);

        var response = await ApiClient
            .LoggedAs(UserToken)
            .PutAsJsonAsync($"{BaseEndpoint}/{account.Id}/operations/{operation.Id}", updateRequest, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<UpdateAccountOperationResponse>(CancellationToken);

        result.ShouldBeSuccessful();
        result.Response.ShouldNotBeNull();
        result.Response.Description.ShouldBe("Updated Description");
        result.Response.Amount.ShouldBe(75m);
        result.Response.OperationDate.ShouldBeCloseTo(explicitOperationDate, TimeSpan.FromMilliseconds(1));
        result.Response.UpdatedAt.ShouldBeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));

        var operationInDb = await DbContext
            .AccountOperations.AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == operation.Id, CancellationToken);

        operationInDb.ShouldNotBeNull();
        operationInDb.OperationDate.ShouldBeCloseTo(explicitOperationDate, TimeSpan.FromMilliseconds(1));
    }

    [Fact]
    public async Task UpdateAccountOperation_WithoutOperationDate_ShouldNotChangeOperationDate()
    {
        var account = AccountFixture.CreateValidAccount(User.Id, name: "Test Account", initialBalance: 100m);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        account.AddOperation("Original Description", 50m, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);
        await DbContext.SaveChangesAsync(CancellationToken);

        var operation = account.Operations[0];
        var originalOperationDate = operation.OperationDate;

        var updateRequest = new UpdateAccountOperationRequest(75m, "Updated Description");

        var response = await ApiClient
            .LoggedAs(UserToken)
            .PutAsJsonAsync($"{BaseEndpoint}/{account.Id}/operations/{operation.Id}", updateRequest, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<UpdateAccountOperationResponse>(CancellationToken);

        result.ShouldBeSuccessful();
        result.Response.ShouldNotBeNull();
        result.Response.OperationDate.ShouldBeCloseTo(originalOperationDate, TimeSpan.FromMilliseconds(1));
    }

    [Fact]
    public async Task UpdateAccountOperation_WithFutureOperationDate_ShouldReturnValidationError()
    {
        var account = AccountFixture.CreateValidAccount(User.Id, name: "Test Account", initialBalance: 100m);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        account.AddOperation("Original Description", 50m, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);
        await DbContext.SaveChangesAsync(CancellationToken);

        var operation = account.Operations[0];
        var futureDate = DateTimeOffset.UtcNow.AddDays(1);
        var updateRequest = new UpdateAccountOperationRequest(75m, "Updated Description", futureDate);

        var response = await ApiClient
            .LoggedAs(UserToken)
            .PutAsJsonAsync($"{BaseEndpoint}/{account.Id}/operations/{operation.Id}", updateRequest, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<UpdateAccountOperationResponse>(CancellationToken);

        result.ShouldBeProblem();
        result.Problem.ShouldNotBeNull();
        result.Problem.Status.ShouldBe(StatusCodes.Status400BadRequest);
        result.Problem.ShouldHaveError(AccountOperationErrors.AccountOperationDateInFuture.Code);
    }
}
