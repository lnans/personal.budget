using System.Net.Http.Json;
using Api.Contracts.AccountOperations;
using Application.Features.AccountOperations.Commands.AddAccountOperation;
using Domain.AccountOperations;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TestFixtures.Domain;

namespace Api.Tests.AccountOperations;

[Collection(ApiTestCollection.CollectionName)]
public class AddAccountOperationTests : ApiTestBase
{
    private const string BaseEndpoint = "/accounts";

    public AddAccountOperationTests(ApiTestFixture factory)
        : base(factory) { }

    [Fact]
    public async Task AddAccountOperation_WithValidPositiveAmount_ShouldAddOperationAndUpdateBalance()
    {
        // Arrange
        var account = AccountFixture.CreateValidAccount(User.Id, name: "Test Account", initialBalance: 100m);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        var request = new AddAccountOperationRequest("Salary", 500m);

        // Act
        var response = await ApiClient
            .LoggedAs(UserToken)
            .PostAsJsonAsync($"{BaseEndpoint}/{account.Id}/operations", request, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<AddAccountOperationResponse>(CancellationToken);

        // Assert
        result.ShouldBeSuccessful();
        result.Response.ShouldNotBeNull();
        result.Response.AccountId.ShouldBe(account.Id);
        result.Response.AccountName.ShouldBe("Test Account");
        result.Response.Description.ShouldBe(request.Description);
        result.Response.Amount.ShouldBe(500m);
        result.Response.PreviousBalance.ShouldBe(100m);
        result.Response.NextBalance.ShouldBe(600m); // 100 + 500
    }

    [Fact]
    public async Task AddAccountOperation_WithValidNegativeAmount_ShouldAddOperationAndUpdateBalance()
    {
        // Arrange
        var account = AccountFixture.CreateValidAccount(User.Id, name: "Test Account", initialBalance: 100m);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        var request = new AddAccountOperationRequest("Rent payment", -50m);

        // Act
        var response = await ApiClient
            .LoggedAs(UserToken)
            .PostAsJsonAsync($"{BaseEndpoint}/{account.Id}/operations", request, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<AddAccountOperationResponse>(CancellationToken);

        // Assert
        result.ShouldBeSuccessful();
        result.Response.ShouldNotBeNull();
        result.Response.AccountId.ShouldBe(account.Id);
        result.Response.AccountName.ShouldBe("Test Account");
        result.Response.Description.ShouldBe(request.Description);
        result.Response.Amount.ShouldBe(-50m);
        result.Response.PreviousBalance.ShouldBe(100m);
        result.Response.NextBalance.ShouldBe(50m); // 100 - 50
    }

    [Fact]
    public async Task AddAccountOperation_WithZeroAmount_ShouldAddOperation()
    {
        // Arrange
        var account = AccountFixture.CreateValidAccount(User.Id, name: "Test Account", initialBalance: 100m);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        var request = new AddAccountOperationRequest("No change", 0m);

        // Act
        var response = await ApiClient
            .LoggedAs(UserToken)
            .PostAsJsonAsync($"{BaseEndpoint}/{account.Id}/operations", request, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<AddAccountOperationResponse>(CancellationToken);

        // Assert
        result.ShouldBeSuccessful();
        result.Response.ShouldNotBeNull();
        result.Response.AccountId.ShouldBe(account.Id);
        result.Response.AccountName.ShouldBe("Test Account");
        result.Response.Description.ShouldBe(request.Description);
        result.Response.Amount.ShouldBe(0m);
        result.Response.PreviousBalance.ShouldBe(100m);
        result.Response.NextBalance.ShouldBe(100m);
    }

    [Fact]
    public async Task AddAccountOperation_WithEmptyDescription_ShouldReturnValidationError()
    {
        // Arrange
        var account = AccountFixture.CreateValidAccount(User.Id, name: "Test Account", initialBalance: 100m);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        var request = new AddAccountOperationRequest("", 100m);

        // Act
        var response = await ApiClient
            .LoggedAs(UserToken)
            .PostAsJsonAsync($"{BaseEndpoint}/{account.Id}/operations", request, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<AddAccountOperationResponse>(CancellationToken);

        // Assert
        result.ShouldBeProblem();
        result.Problem.ShouldNotBeNull();
        result.Problem.Status.ShouldBe(StatusCodes.Status400BadRequest);
        result.Problem.ShouldHaveError(AccountOperationErrors.AccountOperationDescriptionRequired.Code);
    }

    [Fact]
    public async Task AddAccountOperation_WithTooLongDescription_ShouldReturnValidationError()
    {
        // Arrange
        var account = AccountFixture.CreateValidAccount(User.Id, name: "Test Account", initialBalance: 100m);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        var request = new AddAccountOperationRequest(
            new string('a', AccountOperationConstants.MaxDescriptionLength + 1),
            100m
        );

        // Act
        var response = await ApiClient
            .LoggedAs(UserToken)
            .PostAsJsonAsync($"{BaseEndpoint}/{account.Id}/operations", request, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<AddAccountOperationResponse>(CancellationToken);

        // Assert
        result.ShouldBeProblem();
        result.Problem.ShouldNotBeNull();
        result.Problem.Status.ShouldBe(StatusCodes.Status400BadRequest);
        result.Problem.ShouldHaveError(AccountOperationErrors.AccountOperationDescriptionTooLong.Code);
    }

    [Fact]
    public async Task AddAccountOperation_WithNonExistentAccountId_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var request = new AddAccountOperationRequest("Test", 100m);

        // Act
        var response = await ApiClient
            .LoggedAs(UserToken)
            .PostAsJsonAsync($"{BaseEndpoint}/{nonExistentId}/operations", request, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<AddAccountOperationResponse>(CancellationToken);

        // Assert
        result.ShouldBeProblem();
        result.Problem.ShouldNotBeNull();
        result.Problem.Status.ShouldBe(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task AddAccountOperation_ShouldPersistOperationInDatabase()
    {
        // Arrange
        var account = AccountFixture.CreateValidAccount(User.Id, name: "Test Account", initialBalance: 100m);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        var request = new AddAccountOperationRequest("Test Operation", 200m);

        // Act
        var response = await ApiClient
            .LoggedAs(UserToken)
            .PostAsJsonAsync($"{BaseEndpoint}/{account.Id}/operations", request, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<AddAccountOperationResponse>(CancellationToken);

        // Assert
        result.ShouldBeSuccessful();
        result.Response.ShouldNotBeNull();

        var accountInDb = await DbContext
            .Accounts.AsNoTracking()
            .Include(a => a.Operations)
            .FirstOrDefaultAsync(a => a.Id == account.Id, CancellationToken);

        accountInDb.ShouldNotBeNull();
        accountInDb.Balance.ShouldBe(300m);
        accountInDb.Operations.Count.ShouldBe(1);

        var operation = accountInDb.Operations[0];
        operation.Id.ShouldBe(result.Response.Id);
        operation.Description.ShouldBe(request.Description);
        operation.Amount.ShouldBe(request.Amount);
        operation.PreviousBalance.ShouldBe(100m);
        operation.NextBalance.ShouldBe(300m);
        operation.AccountId.ShouldBe(account.Id);
        operation.CreatedAt.ShouldBeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task AddAccountOperation_WithMultipleOperations_ShouldUpdateBalanceCorrectly()
    {
        // Arrange
        var account = AccountFixture.CreateValidAccount(User.Id, name: "Test Account", initialBalance: 100m);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        var request1 = new AddAccountOperationRequest("First", 50m);
        var request2 = new AddAccountOperationRequest("Second", -30m);
        var request3 = new AddAccountOperationRequest("Third", 100m);

        // Act
        await ApiClient
            .LoggedAs(UserToken)
            .PostAsJsonAsync($"{BaseEndpoint}/{account.Id}/operations", request1, CancellationToken);
        await ApiClient
            .LoggedAs(UserToken)
            .PostAsJsonAsync($"{BaseEndpoint}/{account.Id}/operations", request2, CancellationToken);
        var response = await ApiClient
            .LoggedAs(UserToken)
            .PostAsJsonAsync($"{BaseEndpoint}/{account.Id}/operations", request3, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<AddAccountOperationResponse>(CancellationToken);

        // Assert
        result.ShouldBeSuccessful();
        result.Response.ShouldNotBeNull();
        result.Response.AccountId.ShouldBe(account.Id);
        result.Response.AccountName.ShouldBe("Test Account");
        result.Response.Description.ShouldBe(request3.Description);
        result.Response.Amount.ShouldBe(100m);
        result.Response.PreviousBalance.ShouldBe(120m);
        result.Response.NextBalance.ShouldBe(220m); // 100 + 50 - 30 + 100

        var accountInDb = await DbContext
            .Accounts.AsNoTracking()
            .Include(a => a.Operations)
            .FirstOrDefaultAsync(a => a.Id == account.Id, CancellationToken);

        accountInDb.ShouldNotBeNull();
        accountInDb.Operations.Count.ShouldBe(3);
        accountInDb.Balance.ShouldBe(220m);
    }

    [Fact]
    public async Task AddAccountOperation_WithNegativeBalanceResult_ShouldSucceed()
    {
        // Arrange
        var account = AccountFixture.CreateValidAccount(User.Id, name: "Test Account", initialBalance: 50m);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        var request = new AddAccountOperationRequest("Overdraft", -100m);

        // Act
        var response = await ApiClient
            .LoggedAs(UserToken)
            .PostAsJsonAsync($"{BaseEndpoint}/{account.Id}/operations", request, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<AddAccountOperationResponse>(CancellationToken);

        // Assert
        result.ShouldBeSuccessful();
        result.Response.ShouldNotBeNull();
        result.Response.AccountId.ShouldBe(account.Id);
        result.Response.AccountName.ShouldBe("Test Account");
        result.Response.Description.ShouldBe(request.Description);
        result.Response.Amount.ShouldBe(-100m);
        result.Response.PreviousBalance.ShouldBe(50m);
        result.Response.NextBalance.ShouldBe(-50m);
    }

    [Fact]
    public async Task AddAccountOperation_WithExplicitOperationDate_ShouldUseProvidedTimestamp()
    {
        var account = AccountFixture.CreateValidAccount(User.Id, name: "Test Account", initialBalance: 100m);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        var explicitOperationDate = new DateTimeOffset(2025, 6, 15, 10, 30, 0, TimeSpan.Zero);
        var request = new AddAccountOperationRequest("Backdated Operation", 200m, explicitOperationDate);

        var response = await ApiClient
            .LoggedAs(UserToken)
            .PostAsJsonAsync($"{BaseEndpoint}/{account.Id}/operations", request, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<AddAccountOperationResponse>(CancellationToken);

        result.ShouldBeSuccessful();
        result.Response.ShouldNotBeNull();
        result.Response.OperationDate.ShouldBeCloseTo(explicitOperationDate, TimeSpan.FromMilliseconds(1));
        result.Response.CreatedAt.ShouldBeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
        result.Response.Amount.ShouldBe(200m);
        result.Response.PreviousBalance.ShouldBe(100m);
        result.Response.NextBalance.ShouldBe(300m);

        var operationInDb = await DbContext
            .AccountOperations.AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == result.Response.Id, CancellationToken);

        operationInDb.ShouldNotBeNull();
        operationInDb.OperationDate.ShouldBeCloseTo(explicitOperationDate, TimeSpan.FromMilliseconds(1));
    }

    [Fact]
    public async Task AddAccountOperation_WithoutOperationDate_ShouldDefaultToCurrentTime()
    {
        var account = AccountFixture.CreateValidAccount(User.Id, name: "Test Account", initialBalance: 100m);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        var request = new AddAccountOperationRequest("Current Time Operation", 50m);

        var response = await ApiClient
            .LoggedAs(UserToken)
            .PostAsJsonAsync($"{BaseEndpoint}/{account.Id}/operations", request, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<AddAccountOperationResponse>(CancellationToken);

        result.ShouldBeSuccessful();
        result.Response.ShouldNotBeNull();
        result.Response.OperationDate.ShouldBeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task AddAccountOperation_WithFutureOperationDate_ShouldReturnValidationError()
    {
        var account = AccountFixture.CreateValidAccount(User.Id, name: "Test Account", initialBalance: 100m);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        var futureDate = DateTimeOffset.UtcNow.AddDays(1);
        var request = new AddAccountOperationRequest("Future Operation", 50m, futureDate);

        var response = await ApiClient
            .LoggedAs(UserToken)
            .PostAsJsonAsync($"{BaseEndpoint}/{account.Id}/operations", request, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<AddAccountOperationResponse>(CancellationToken);

        result.ShouldBeProblem();
        result.Problem.ShouldNotBeNull();
        result.Problem.Status.ShouldBe(StatusCodes.Status400BadRequest);
        result.Problem.ShouldHaveError(AccountOperationErrors.AccountOperationDateInFuture.Code);
    }
}
