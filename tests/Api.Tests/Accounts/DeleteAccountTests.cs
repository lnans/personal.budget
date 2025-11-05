using Application.Features.Accounts.Commands.DeleteAccount;
using Application.Features.Accounts.Queries.GetAccounts;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TestFixtures.Domain;

namespace Api.Tests.Accounts;

[Collection(ApiTestCollection.CollectionName)]
public class DeleteAccountTests : ApiTestBase
{
    private const string Endpoint = "/accounts";

    public DeleteAccountTests(ApiTestFixture factory)
        : base(factory) { }

    [Fact]
    public async Task DeleteAccount_WithValidId_ShouldSoftDeleteAccount()
    {
        // Arrange
        var account = AccountFixture.CreateValidAccount(User.Id);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        // Act
        var response = await ApiClient.LoggedAs(UserToken).DeleteAsync($"{Endpoint}/{account.Id}", CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<DeleteAccountResponse>(CancellationToken);

        // Assert
        result.ShouldBeSuccessful();
        result.Response.ShouldNotBeNull();
        result.Response.Id.ShouldBe(account.Id);
        result.Response.Name.ShouldBe(account.Name);
        result.Response.DeletedAt.ShouldBeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task DeleteAccount_AfterDeletion_ShouldNotBeReturnedInGetAccounts()
    {
        // Arrange
        var accountToDelete = AccountFixture.CreateValidAccount(User.Id);
        var accountToKeep = AccountFixture.CreateValidAccount(User.Id);
        DbContext.Accounts.AddRange(accountToDelete, accountToKeep);
        await DbContext.SaveChangesAsync(CancellationToken);

        // Act
        await ApiClient.LoggedAs(UserToken).DeleteAsync($"{Endpoint}/{accountToDelete.Id}", CancellationToken);

        // Assert
        var getResponse = await ApiClient.LoggedAs(UserToken).GetAsync(Endpoint, CancellationToken);
        var getResult = await getResponse.ReadResponseOrProblemAsync<List<GetAccountsResponse>>(CancellationToken);

        getResult.ShouldBeSuccessful();
        getResult.Response.ShouldNotBeNull();
        getResult.Response.Count.ShouldBe(1);
        getResult.Response.ShouldNotContain(a => a.Id == accountToDelete.Id);
        getResult.Response.ShouldContain(a => a.Id == accountToKeep.Id);
    }

    [Fact]
    public async Task DeleteAccount_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await ApiClient
            .LoggedAs(UserToken)
            .DeleteAsync($"{Endpoint}/{nonExistentId}", CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<DeleteAccountResponse>(CancellationToken);

        // Assert
        result.ShouldBeProblem();
        result.Problem.ShouldNotBeNull();
        result.Problem.Status.ShouldBe(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task DeleteAccount_WhenAlreadyDeleted_ShouldReturnNotFound()
    {
        // Arrange
        var account = AccountFixture.CreateValidAccount(User.Id);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        // Act
        var firstDeleteResponse = await ApiClient
            .LoggedAs(UserToken)
            .DeleteAsync($"{Endpoint}/{account.Id}", CancellationToken);
        var firstResult = await firstDeleteResponse.ReadResponseOrProblemAsync<DeleteAccountResponse>(
            CancellationToken
        );

        var secondDeleteResponse = await ApiClient
            .LoggedAs(UserToken)
            .DeleteAsync($"{Endpoint}/{account.Id}", CancellationToken);
        var secondResult = await secondDeleteResponse.ReadResponseOrProblemAsync<DeleteAccountResponse>(
            CancellationToken
        );

        // Assert
        firstResult.ShouldBeSuccessful();
        secondResult.ShouldBeProblem();
        secondResult.Problem.ShouldNotBeNull();
        secondResult.Problem.Status.ShouldBe(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task DeleteAccount_ShouldPersistInDatabase()
    {
        // Arrange
        var account = AccountFixture.CreateValidAccount(User.Id);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);
        var accountId = account.Id;

        // Act
        var response = await ApiClient.LoggedAs(UserToken).DeleteAsync($"{Endpoint}/{accountId}", CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<DeleteAccountResponse>(CancellationToken);

        // Assert
        result.ShouldBeSuccessful();

        // Use a fresh DbContext to query the database and verify the soft delete
        using var freshScope = CreateFreshScope();
        var freshDbContext = freshScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var accountInDb = await freshDbContext
            .Accounts.IgnoreQueryFilters()
            .FirstOrDefaultAsync(a => a.Id == accountId, CancellationToken);
        accountInDb.ShouldNotBeNull();
        accountInDb.DeletedAt.ShouldNotBeNull();
        accountInDb.DeletedAt.Value.ShouldBeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task DeleteAccount_ShouldUpdateUpdatedAtTimestamp()
    {
        // Arrange
        var account = AccountFixture.CreateValidAccount(User.Id);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        // Act
        var response = await ApiClient.LoggedAs(UserToken).DeleteAsync($"{Endpoint}/{account.Id}", CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<DeleteAccountResponse>(CancellationToken);

        // Assert
        result.ShouldBeSuccessful();
        result.Response.ShouldNotBeNull();
        result.Response.UpdatedAt.ShouldBeCloseTo(result.Response.DeletedAt, TimeSpan.FromMilliseconds(1));
    }

    [Fact]
    public async Task DeleteAccount_ShouldCascadeSoftDeleteToOperations()
    {
        // Arrange
        var account = AccountFixture.CreateValidAccount(User.Id);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        account.AddOperation("Operation 1", 100m, DateTimeOffset.UtcNow);
        account.AddOperation("Operation 2", 50m, DateTimeOffset.UtcNow);
        await DbContext.SaveChangesAsync(CancellationToken);
        var accountId = account.Id;

        // Act
        var response = await ApiClient.LoggedAs(UserToken).DeleteAsync($"{Endpoint}/{accountId}", CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<DeleteAccountResponse>(CancellationToken);

        // Assert
        result.ShouldBeSuccessful();

        // Verify operations are also soft-deleted
        using var freshScope = CreateFreshScope();
        var freshDbContext = freshScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var operations = await freshDbContext
            .AccountOperations.IgnoreQueryFilters()
            .Where(op => op.AccountId == accountId)
            .ToListAsync(CancellationToken);

        operations.ShouldNotBeEmpty();
        operations.ShouldAllBe(op => op.DeletedAt != null);
        foreach (var operation in operations)
        {
            operation.DeletedAt.ShouldNotBeNull();
            operation.DeletedAt.Value.ShouldBeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(2));
        }
    }
}
