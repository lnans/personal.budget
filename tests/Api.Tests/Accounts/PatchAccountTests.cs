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
    public async Task PatchAccount_WithValidData_ShouldUpdateAccountName()
    {
        // Arrange
        var account = AccountFixture.CreateValidAccount(User.Id, "Original Name", 100m);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        var originalCreatedAt = account.CreatedAt;
        var patchCommand = new PatchAccountCommand { Id = account.Id, Name = "Updated Name" };

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
        result.Response.Balance.ShouldBe(100m);
        result.Response.CreatedAt.ShouldBe(originalCreatedAt);
        result.Response.UpdatedAt.ShouldBeGreaterThan(result.Response.CreatedAt);
        result.Response.UpdatedAt.ShouldBeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task PatchAccount_WithEmptyName_ShouldReturnValidationError()
    {
        // Arrange
        var account = AccountFixture.CreateValidAccount(User.Id, "Test Account", 100m);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        var patchCommand = new PatchAccountCommand { Id = account.Id, Name = "" };

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
        var account = AccountFixture.CreateValidAccount(User.Id, "Test Account", 100m);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        var patchCommand = new PatchAccountCommand { Id = account.Id, Name = AccountFixture.GenerateLongAccountName() };

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
    public async Task PatchAccount_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var patchCommand = new PatchAccountCommand { Id = nonExistentId, Name = "Updated Name" };

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
        var account = AccountFixture.CreateValidAccount(User.Id, "Original Name", 200m);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        var originalCreatedAt = account.CreatedAt;
        var patchCommand = new PatchAccountCommand { Id = account.Id, Name = "Updated Name" };

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
        accountInDb.Balance.ShouldBe(200m);
        accountInDb.UserId.ShouldBe(User.Id);
        accountInDb.CreatedAt.ShouldBe(originalCreatedAt);
        accountInDb.UpdatedAt.ShouldBeGreaterThan(accountInDb.CreatedAt);
        accountInDb.UpdatedAt.ShouldBeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task PatchAccount_ShouldNotUpdateBalance()
    {
        // Arrange
        var account = AccountFixture.CreateValidAccount(User.Id, "Test Account", 500m);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        var originalBalance = account.Balance;
        var patchCommand = new PatchAccountCommand { Id = account.Id, Name = "Updated Name" };

        // Act
        var response = await ApiClient
            .LoggedAs(UserToken)
            .PatchAsJsonAsync($"{BaseEndpoint}/{account.Id}", patchCommand, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<PatchAccountResponse>(CancellationToken);

        // Assert
        result.ShouldBeSuccessful();
        result.Response.ShouldNotBeNull();
        result.Response.Balance.ShouldBe(originalBalance);
    }
}
