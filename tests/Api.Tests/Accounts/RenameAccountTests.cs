using System.Net.Http.Json;
using Application.Features.Accounts.Commands.RenameAccount;
using Domain.Accounts;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TestFixtures.Domain;

namespace Api.Tests.Accounts;

[Collection(ApiTestCollection.CollectionName)]
public class RenameAccountTests : ApiTestBase
{
    private const string BaseEndpoint = "/accounts";

    public RenameAccountTests(ApiTestFixture factory)
        : base(factory) { }

    [Fact]
    public async Task RenameAccount_WithValidData_ShouldUpdateAccountName()
    {
        // Arrange
        var account = AccountFixture.CreateValidAccount(User.Id, name: "Original Name", initialBalance: 100m);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        var originalCreatedAt = account.CreatedAt;
        var renameCommand = new RenameAccountCommand { Id = account.Id, Name = "Updated Name" };

        // Act
        var response = await ApiClient
            .LoggedAs(UserToken)
            .PatchAsJsonAsync($"{BaseEndpoint}/{account.Id}", renameCommand, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<RenameAccountResponse>(CancellationToken);

        // Assert
        result.ShouldBeSuccessful();
        result.Response.ShouldNotBeNull();
        result.Response.Id.ShouldBe(account.Id);
        result.Response.Name.ShouldBe(renameCommand.Name);
        result.Response.Type.ShouldBe(account.Type);
        result.Response.Balance.ShouldBe(100m);
        result.Response.CreatedAt.ShouldBe(originalCreatedAt);
        result.Response.UpdatedAt.ShouldBeGreaterThan(result.Response.CreatedAt);
        result.Response.UpdatedAt.ShouldBeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task RenameAccount_WithEmptyName_ShouldReturnValidationError()
    {
        // Arrange
        var account = AccountFixture.CreateValidAccount(User.Id, name: "Test Account", initialBalance: 100m);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        var renameCommand = new RenameAccountCommand { Id = account.Id, Name = "" };

        // Act
        var response = await ApiClient
            .LoggedAs(UserToken)
            .PatchAsJsonAsync($"{BaseEndpoint}/{account.Id}", renameCommand, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<RenameAccountResponse>(CancellationToken);

        // Assert
        result.ShouldBeProblem();
        result.Problem.ShouldNotBeNull();
        result.Problem.Status.ShouldBe(StatusCodes.Status400BadRequest);
        result.Problem.ShouldHaveValidationError("Name", AccountErrors.AccountNameRequired.Code);
    }

    [Fact]
    public async Task RenameAccount_WithTooLongName_ShouldReturnValidationError()
    {
        // Arrange
        var account = AccountFixture.CreateValidAccount(User.Id, name: "Test Account", initialBalance: 100m);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        var renameCommand = new RenameAccountCommand
        {
            Id = account.Id,
            Name = AccountFixture.GenerateLongAccountName(),
        };

        // Act
        var response = await ApiClient
            .LoggedAs(UserToken)
            .PatchAsJsonAsync($"{BaseEndpoint}/{account.Id}", renameCommand, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<RenameAccountResponse>(CancellationToken);

        // Assert
        result.ShouldBeProblem();
        result.Problem.ShouldNotBeNull();
        result.Problem.Status.ShouldBe(StatusCodes.Status400BadRequest);
        result.Problem.ShouldHaveValidationError("Name", AccountErrors.AccountNameTooLong.Code);
    }

    [Fact]
    public async Task RenameAccount_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var renameCommand = new RenameAccountCommand { Id = nonExistentId, Name = "Updated Name" };

        // Act
        var response = await ApiClient
            .LoggedAs(UserToken)
            .PatchAsJsonAsync($"{BaseEndpoint}/{nonExistentId}", renameCommand, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<RenameAccountResponse>(CancellationToken);

        // Assert
        result.ShouldBeProblem();
        result.Problem.ShouldNotBeNull();
        result.Problem.Status.ShouldBe(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task RenameAccount_ShouldPersistInDatabase()
    {
        // Arrange
        var account = AccountFixture.CreateValidAccount(User.Id, name: "Original Name", initialBalance: 200m);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        var originalCreatedAt = account.CreatedAt;
        var renameCommand = new RenameAccountCommand { Id = account.Id, Name = "Updated Name" };

        // Act
        var response = await ApiClient
            .LoggedAs(UserToken)
            .PatchAsJsonAsync($"{BaseEndpoint}/{account.Id}", renameCommand, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<RenameAccountResponse>(CancellationToken);

        // Assert
        result.ShouldBeSuccessful();
        result.Response.ShouldNotBeNull();

        var accountInDb = await DbContext
            .Accounts.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == account.Id, CancellationToken);
        accountInDb.ShouldNotBeNull();
        accountInDb.Name.ShouldBe(renameCommand.Name);
        accountInDb.Type.ShouldBe(account.Type);
        accountInDb.Balance.ShouldBe(200m);
        accountInDb.UserId.ShouldBe(User.Id);
        accountInDb.CreatedAt.ShouldBe(originalCreatedAt);
        accountInDb.UpdatedAt.ShouldBeGreaterThan(accountInDb.CreatedAt);
        accountInDb.UpdatedAt.ShouldBeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task RenameAccount_ShouldNotUpdateBalance()
    {
        // Arrange
        var account = AccountFixture.CreateValidAccount(User.Id, name: "Test Account", initialBalance: 500m);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        var originalBalance = account.Balance;
        var renameCommand = new RenameAccountCommand { Id = account.Id, Name = "Updated Name" };

        // Act
        var response = await ApiClient
            .LoggedAs(UserToken)
            .PatchAsJsonAsync($"{BaseEndpoint}/{account.Id}", renameCommand, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<RenameAccountResponse>(CancellationToken);

        // Assert
        result.ShouldBeSuccessful();
        result.Response.ShouldNotBeNull();
        result.Response.Balance.ShouldBe(originalBalance);
    }
}
