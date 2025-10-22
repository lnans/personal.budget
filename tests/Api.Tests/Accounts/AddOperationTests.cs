using System.Net.Http.Json;
using Application.Features.Accounts.Commands.AddOperation;
using Domain.AccountOperations;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TestFixtures.Domain;

namespace Api.Tests.Accounts;

[Collection(ApiTestCollection.CollectionName)]
public class AddOperationTests : ApiTestBase
{
    private const string BaseEndpoint = "/accounts";

    public AddOperationTests(ApiTestFixture factory)
        : base(factory) { }

    [Fact]
    public async Task AddOperation_WithValidPositiveAmount_ShouldAddOperationAndUpdateBalance()
    {
        // Arrange
        var account = AccountFixture.CreateValidAccount(User.Id, name: "Test Account", initialBalance: 100m);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        var command = new AddOperationCommand
        {
            AccountId = account.Id,
            Description = "Salary",
            Amount = 500m,
        };

        // Act
        var response = await ApiClient
            .LoggedAs(UserToken)
            .PostAsJsonAsync($"{BaseEndpoint}/{account.Id}/operations", command, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<AddOperationResponse>(CancellationToken);

        // Assert
        result.ShouldBeSuccessful();
        result.Response.ShouldNotBeNull();
        result.Response.Id.ShouldBe(account.Id);
        result.Response.Name.ShouldBe("Test Account");
        result.Response.Type.ShouldBe(account.Type);
        result.Response.Balance.ShouldBe(600m); // 100 + 500
    }

    [Fact]
    public async Task AddOperation_WithValidNegativeAmount_ShouldAddOperationAndUpdateBalance()
    {
        // Arrange
        var account = AccountFixture.CreateValidAccount(User.Id, name: "Test Account", initialBalance: 100m);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        var command = new AddOperationCommand
        {
            AccountId = account.Id,
            Description = "Rent payment",
            Amount = -50m,
        };

        // Act
        var response = await ApiClient
            .LoggedAs(UserToken)
            .PostAsJsonAsync($"{BaseEndpoint}/{account.Id}/operations", command, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<AddOperationResponse>(CancellationToken);

        // Assert
        result.ShouldBeSuccessful();
        result.Response.ShouldNotBeNull();
        result.Response.Id.ShouldBe(account.Id);
        result.Response.Type.ShouldBe(account.Type);
        result.Response.Balance.ShouldBe(50m); // 100 - 50
    }

    [Fact]
    public async Task AddOperation_WithZeroAmount_ShouldAddOperation()
    {
        // Arrange
        var account = AccountFixture.CreateValidAccount(User.Id, name: "Test Account", initialBalance: 100m);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        var command = new AddOperationCommand
        {
            AccountId = account.Id,
            Description = "No change",
            Amount = 0m,
        };

        // Act
        var response = await ApiClient
            .LoggedAs(UserToken)
            .PostAsJsonAsync($"{BaseEndpoint}/{account.Id}/operations", command, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<AddOperationResponse>(CancellationToken);

        // Assert
        result.ShouldBeSuccessful();
        result.Response.ShouldNotBeNull();
        result.Response.Type.ShouldBe(account.Type);
        result.Response.Balance.ShouldBe(100m);
    }

    [Fact]
    public async Task AddOperation_WithEmptyDescription_ShouldReturnValidationError()
    {
        // Arrange
        var account = AccountFixture.CreateValidAccount(User.Id, name: "Test Account", initialBalance: 100m);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        var command = new AddOperationCommand
        {
            AccountId = account.Id,
            Description = "",
            Amount = 100m,
        };

        // Act
        var response = await ApiClient
            .LoggedAs(UserToken)
            .PostAsJsonAsync($"{BaseEndpoint}/{account.Id}/operations", command, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<AddOperationResponse>(CancellationToken);

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
    public async Task AddOperation_WithTooLongDescription_ShouldReturnValidationError()
    {
        // Arrange
        var account = AccountFixture.CreateValidAccount(User.Id, name: "Test Account", initialBalance: 100m);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        var command = new AddOperationCommand
        {
            AccountId = account.Id,
            Description = new string('a', AccountOperationConstants.MaxDescriptionLength + 1),
            Amount = 100m,
        };

        // Act
        var response = await ApiClient
            .LoggedAs(UserToken)
            .PostAsJsonAsync($"{BaseEndpoint}/{account.Id}/operations", command, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<AddOperationResponse>(CancellationToken);

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
    public async Task AddOperation_WithNonExistentAccountId_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var command = new AddOperationCommand
        {
            AccountId = nonExistentId,
            Description = "Test",
            Amount = 100m,
        };

        // Act
        var response = await ApiClient
            .LoggedAs(UserToken)
            .PostAsJsonAsync($"{BaseEndpoint}/{nonExistentId}/operations", command, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<AddOperationResponse>(CancellationToken);

        // Assert
        result.ShouldBeProblem();
        result.Problem.ShouldNotBeNull();
        result.Problem.Status.ShouldBe(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task AddOperation_ShouldPersistOperationInDatabase()
    {
        // Arrange
        var account = AccountFixture.CreateValidAccount(User.Id, name: "Test Account", initialBalance: 100m);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        var command = new AddOperationCommand
        {
            AccountId = account.Id,
            Description = "Test Operation",
            Amount = 200m,
        };

        // Act
        var response = await ApiClient
            .LoggedAs(UserToken)
            .PostAsJsonAsync($"{BaseEndpoint}/{account.Id}/operations", command, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<AddOperationResponse>(CancellationToken);

        // Assert
        result.ShouldBeSuccessful();

        var accountInDb = await DbContext
            .Accounts.AsNoTracking()
            .Include(a => a.Operations)
            .FirstOrDefaultAsync(a => a.Id == account.Id, CancellationToken);

        accountInDb.ShouldNotBeNull();
        accountInDb.Balance.ShouldBe(300m);
        accountInDb.Operations.Count.ShouldBe(1);

        var operation = accountInDb.Operations.First();
        operation.Description.ShouldBe(command.Description);
        operation.Amount.ShouldBe(command.Amount);
        operation.PreviousBalance.ShouldBe(100m);
        operation.NextBalance.ShouldBe(300m);
        operation.AccountId.ShouldBe(account.Id);
        operation.CreatedAt.ShouldBeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task AddOperation_WithMultipleOperations_ShouldUpdateBalanceCorrectly()
    {
        // Arrange
        var account = AccountFixture.CreateValidAccount(User.Id, name: "Test Account", initialBalance: 100m);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        var command1 = new AddOperationCommand
        {
            AccountId = account.Id,
            Description = "First",
            Amount = 50m,
        };
        var command2 = new AddOperationCommand
        {
            AccountId = account.Id,
            Description = "Second",
            Amount = -30m,
        };
        var command3 = new AddOperationCommand
        {
            AccountId = account.Id,
            Description = "Third",
            Amount = 100m,
        };

        // Act
        await ApiClient
            .LoggedAs(UserToken)
            .PostAsJsonAsync($"{BaseEndpoint}/{account.Id}/operations", command1, CancellationToken);
        await ApiClient
            .LoggedAs(UserToken)
            .PostAsJsonAsync($"{BaseEndpoint}/{account.Id}/operations", command2, CancellationToken);
        var response = await ApiClient
            .LoggedAs(UserToken)
            .PostAsJsonAsync($"{BaseEndpoint}/{account.Id}/operations", command3, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<AddOperationResponse>(CancellationToken);

        // Assert
        result.ShouldBeSuccessful();
        result.Response.ShouldNotBeNull();
        result.Response.Type.ShouldBe(account.Type);
        result.Response.Balance.ShouldBe(220m); // 100 + 50 - 30 + 100

        var accountInDb = await DbContext
            .Accounts.AsNoTracking()
            .Include(a => a.Operations)
            .FirstOrDefaultAsync(a => a.Id == account.Id, CancellationToken);

        accountInDb.ShouldNotBeNull();
        accountInDb.Operations.Count.ShouldBe(3);
        accountInDb.Balance.ShouldBe(220m);
    }

    [Fact]
    public async Task AddOperation_WithNegativeBalanceResult_ShouldSucceed()
    {
        // Arrange
        var account = AccountFixture.CreateValidAccount(User.Id, name: "Test Account", initialBalance: 50m);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        var command = new AddOperationCommand
        {
            AccountId = account.Id,
            Description = "Overdraft",
            Amount = -100m,
        };

        // Act
        var response = await ApiClient
            .LoggedAs(UserToken)
            .PostAsJsonAsync($"{BaseEndpoint}/{account.Id}/operations", command, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<AddOperationResponse>(CancellationToken);

        // Assert
        result.ShouldBeSuccessful();
        result.Response.ShouldNotBeNull();
        result.Response.Type.ShouldBe(account.Type);
        result.Response.Balance.ShouldBe(-50m);
    }
}
