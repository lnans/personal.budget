using System.Net;
using Application.Features.Accounts.Queries.GetPaginatedAccountOperations;
using Application.Models;
using Domain.Accounts;
using Microsoft.AspNetCore.Http;
using TestFixtures.Domain;

namespace Api.Tests.Accounts;

[Collection(ApiTestCollection.CollectionName)]
public class GetPaginatedAccountOperationsTests : ApiTestBase
{
    private const string Endpoint = "/accounts/operations";

    public GetPaginatedAccountOperationsTests(ApiTestFixture factory)
        : base(factory) { }

    [Fact]
    public async Task GetPaginatedAccountOperations_ReturnsEmptyList_WhenNoOperationsExist()
    {
        // Arrange
        var account = AccountFixture.CreateValidAccount(User.Id);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        // Act
        var response = await ApiClient.LoggedAs(UserToken).GetAsync(Endpoint, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<PaginatedList<GetPaginatedAccountOperationsResponse>>(
            CancellationToken
        );

        // Assert
        result.ShouldBeSuccessful();
        result.Response.ShouldNotBeNull();
        result.Response.Items.ShouldBeEmpty();
        result.Response.TotalCount.ShouldBe(0);
        result.Response.PageNumber.ShouldBe(1);
        result.Response.PageSize.ShouldBe(10);
        result.Response.TotalPages.ShouldBe(0);
        result.Response.HasPreviousPage.ShouldBeFalse();
        result.Response.HasNextPage.ShouldBeFalse();
    }

    [Fact]
    public async Task GetPaginatedAccountOperations_ReturnsOperations_WhenOperationsExist()
    {
        // Arrange
        var account = AccountFixture.CreateValidAccount(User.Id, name: "Test Account", initialBalance: 100m);
        account.AddOperation("First operation", 50m, DateTimeOffset.UtcNow);
        account.AddOperation("Second operation", -30m, DateTimeOffset.UtcNow);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        // Act
        var response = await ApiClient.LoggedAs(UserToken).GetAsync(Endpoint, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<PaginatedList<GetPaginatedAccountOperationsResponse>>(
            CancellationToken
        );

        // Assert
        result.ShouldBeSuccessful();
        result.Response.ShouldNotBeNull();
        result.Response.Items.Count.ShouldBe(2);
        result.Response.TotalCount.ShouldBe(2);
    }

    [Fact]
    public async Task GetPaginatedAccountOperations_ReturnsCorrectOperationData()
    {
        // Arrange
        var account = AccountFixture.CreateValidAccount(User.Id, name: "My Account", initialBalance: 200m);
        account.AddOperation("Salary", 500m, DateTimeOffset.UtcNow);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        // Act
        var response = await ApiClient.LoggedAs(UserToken).GetAsync(Endpoint, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<PaginatedList<GetPaginatedAccountOperationsResponse>>(
            CancellationToken
        );

        // Assert
        result.ShouldBeSuccessful();
        result.Response.ShouldNotBeNull();
        result.Response.Items.Count.ShouldBe(1);

        var operation = result.Response.Items.First();
        operation.AccountId.ShouldBe(account.Id);
        operation.AccountName.ShouldBe("My Account");
        operation.Description.ShouldBe("Salary");
        operation.Amount.ShouldBe(500m);
        operation.PreviousBalance.ShouldBe(200m);
        operation.NextBalance.ShouldBe(700m);
    }

    [Fact]
    public async Task GetPaginatedAccountOperations_FiltersByAccountId_WhenProvided()
    {
        // Arrange
        var account1 = AccountFixture.CreateValidAccount(User.Id, name: "Account 1", initialBalance: 100m);
        account1.AddOperation("Op 1", 50m, DateTimeOffset.UtcNow);
        account1.AddOperation("Op 2", 25m, DateTimeOffset.UtcNow);

        var account2 = AccountFixture.CreateValidAccount(User.Id, name: "Account 2", initialBalance: 200m);
        account2.AddOperation("Op 3", 75m, DateTimeOffset.UtcNow);

        DbContext.Accounts.AddRange(account1, account2);
        await DbContext.SaveChangesAsync(CancellationToken);

        // Act
        var response = await ApiClient
            .LoggedAs(UserToken)
            .GetAsync($"{Endpoint}?accountId={account1.Id}", CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<PaginatedList<GetPaginatedAccountOperationsResponse>>(
            CancellationToken
        );

        // Assert
        result.ShouldBeSuccessful();
        result.Response.ShouldNotBeNull();
        result.Response.Items.Count.ShouldBe(2);
        result.Response.TotalCount.ShouldBe(2);
        result.Response.Items.ShouldAllBe(op => op.AccountId == account1.Id);
    }

    [Fact]
    public async Task GetPaginatedAccountOperations_ReturnsAllOperations_WhenNoAccountIdProvided()
    {
        // Arrange
        var account1 = AccountFixture.CreateValidAccount(User.Id, name: "Account 1", initialBalance: 100m);
        account1.AddOperation("Op 1", 50m, DateTimeOffset.UtcNow);

        var account2 = AccountFixture.CreateValidAccount(User.Id, name: "Account 2", initialBalance: 200m);
        account2.AddOperation("Op 2", 75m, DateTimeOffset.UtcNow);

        DbContext.Accounts.AddRange(account1, account2);
        await DbContext.SaveChangesAsync(CancellationToken);

        // Act
        var response = await ApiClient.LoggedAs(UserToken).GetAsync(Endpoint, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<PaginatedList<GetPaginatedAccountOperationsResponse>>(
            CancellationToken
        );

        // Assert
        result.ShouldBeSuccessful();
        result.Response.ShouldNotBeNull();
        result.Response.Items.Count.ShouldBe(2);
        result.Response.TotalCount.ShouldBe(2);
    }

    [Fact]
    public async Task GetPaginatedAccountOperations_RespectsPageSize()
    {
        // Arrange
        var account = AccountFixture.CreateValidAccount(User.Id, initialBalance: 0m);
        for (var i = 0; i < 5; i++)
        {
            account.AddOperation($"Op {i}", 10m, DateTimeOffset.UtcNow.AddMinutes(i));
        }

        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        // Act
        var response = await ApiClient.LoggedAs(UserToken).GetAsync($"{Endpoint}?pageSize=2", CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<PaginatedList<GetPaginatedAccountOperationsResponse>>(
            CancellationToken
        );

        // Assert
        result.ShouldBeSuccessful();
        result.Response.ShouldNotBeNull();
        result.Response.Items.Count.ShouldBe(2);
        result.Response.TotalCount.ShouldBe(5);
        result.Response.PageSize.ShouldBe(2);
        result.Response.PageNumber.ShouldBe(1);
        result.Response.TotalPages.ShouldBe(3);
        result.Response.HasPreviousPage.ShouldBeFalse();
        result.Response.HasNextPage.ShouldBeTrue();
    }

    [Fact]
    public async Task GetPaginatedAccountOperations_RespectsPageNumber()
    {
        // Arrange
        var account = AccountFixture.CreateValidAccount(User.Id, initialBalance: 0m);
        for (var i = 0; i < 5; i++)
        {
            account.AddOperation($"Op {i}", 10m, DateTimeOffset.UtcNow.AddMinutes(i));
        }

        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        // Act
        var response = await ApiClient
            .LoggedAs(UserToken)
            .GetAsync($"{Endpoint}?pageNumber=2&pageSize=2", CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<PaginatedList<GetPaginatedAccountOperationsResponse>>(
            CancellationToken
        );

        // Assert
        result.ShouldBeSuccessful();
        result.Response.ShouldNotBeNull();
        result.Response.Items.Count.ShouldBe(2);
        result.Response.TotalCount.ShouldBe(5);
        result.Response.PageNumber.ShouldBe(2);
        result.Response.PageSize.ShouldBe(2);
        result.Response.HasPreviousPage.ShouldBeTrue();
        result.Response.HasNextPage.ShouldBeTrue();
    }

    [Fact]
    public async Task GetPaginatedAccountOperations_LastPage_HasCorrectItemCount()
    {
        // Arrange
        var account = AccountFixture.CreateValidAccount(User.Id, initialBalance: 0m);
        for (var i = 0; i < 5; i++)
        {
            account.AddOperation($"Op {i}", 10m, DateTimeOffset.UtcNow.AddMinutes(i));
        }

        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        // Act
        var response = await ApiClient
            .LoggedAs(UserToken)
            .GetAsync($"{Endpoint}?pageNumber=3&pageSize=2", CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<PaginatedList<GetPaginatedAccountOperationsResponse>>(
            CancellationToken
        );

        // Assert
        result.ShouldBeSuccessful();
        result.Response.ShouldNotBeNull();
        result.Response.Items.Count.ShouldBe(1);
        result.Response.TotalCount.ShouldBe(5);
        result.Response.PageNumber.ShouldBe(3);
        result.Response.TotalPages.ShouldBe(3);
        result.Response.HasPreviousPage.ShouldBeTrue();
        result.Response.HasNextPage.ShouldBeFalse();
    }

    [Fact]
    public async Task GetPaginatedAccountOperations_OrdersByCreatedAtDescending()
    {
        // Arrange
        var account = AccountFixture.CreateValidAccount(User.Id, initialBalance: 0m);
        account.AddOperation("Oldest", 10m, DateTimeOffset.UtcNow.AddMinutes(-2));
        account.AddOperation("Middle", 20m, DateTimeOffset.UtcNow.AddMinutes(-1));
        account.AddOperation("Newest", 30m, DateTimeOffset.UtcNow);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        // Act
        var response = await ApiClient.LoggedAs(UserToken).GetAsync(Endpoint, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<PaginatedList<GetPaginatedAccountOperationsResponse>>(
            CancellationToken
        );

        // Assert
        result.ShouldBeSuccessful();
        result.Response.ShouldNotBeNull();
        result.Response.Items.Count.ShouldBe(3);
        result.Response.Items[0].Description.ShouldBe("Newest");
        result.Response.Items[1].Description.ShouldBe("Middle");
        result.Response.Items[2].Description.ShouldBe("Oldest");
    }

    [Fact]
    public async Task GetPaginatedAccountOperations_DoesNotReturnOtherUsersOperations()
    {
        // Arrange
        var otherUser = UserFixture.CreateValidUser(login: "otheruser");
        DbContext.Users.Add(otherUser);
        await DbContext.SaveChangesAsync(CancellationToken);

        var ownAccount = AccountFixture.CreateValidAccount(User.Id, name: "My Account", initialBalance: 100m);
        ownAccount.AddOperation("My Op", 50m, DateTimeOffset.UtcNow);

        var otherAccount = AccountFixture.CreateValidAccount(otherUser.Id, name: "Other Account", initialBalance: 200m);
        otherAccount.AddOperation("Other Op", 75m, DateTimeOffset.UtcNow);

        DbContext.Accounts.AddRange(ownAccount, otherAccount);
        await DbContext.SaveChangesAsync(CancellationToken);

        // Act
        var response = await ApiClient.LoggedAs(UserToken).GetAsync(Endpoint, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<PaginatedList<GetPaginatedAccountOperationsResponse>>(
            CancellationToken
        );

        // Assert
        result.ShouldBeSuccessful();
        result.Response.ShouldNotBeNull();
        result.Response.Items.Count.ShouldBe(1);
        result.Response.TotalCount.ShouldBe(1);
        result.Response.Items[0].Description.ShouldBe("My Op");
        result.Response.Items[0].AccountId.ShouldBe(ownAccount.Id);
    }

    [Fact]
    public async Task GetPaginatedAccountOperations_UsesDefaultPagination_WhenNoParametersProvided()
    {
        // Arrange
        var account = AccountFixture.CreateValidAccount(User.Id, initialBalance: 0m);
        for (var i = 0; i < 15; i++)
        {
            account.AddOperation($"Op {i}", 10m, DateTimeOffset.UtcNow.AddMinutes(i));
        }

        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync(CancellationToken);

        // Act
        var response = await ApiClient.LoggedAs(UserToken).GetAsync(Endpoint, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<PaginatedList<GetPaginatedAccountOperationsResponse>>(
            CancellationToken
        );

        // Assert
        result.ShouldBeSuccessful();
        result.Response.ShouldNotBeNull();
        result.Response.Items.Count.ShouldBe(10);
        result.Response.TotalCount.ShouldBe(15);
        result.Response.PageNumber.ShouldBe(1);
        result.Response.PageSize.ShouldBe(10);
        result.Response.TotalPages.ShouldBe(2);
        result.Response.HasPreviousPage.ShouldBeFalse();
        result.Response.HasNextPage.ShouldBeTrue();
    }

    [Fact]
    public async Task GetPaginatedAccountOperations_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        // Act
        var response = await ApiClient.GetAsync(Endpoint, CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetPaginatedAccountOperations_ReturnsValidationError_WhenAccountIdIsNotValidGuid()
    {
        // Act
        var response = await ApiClient
            .LoggedAs(UserToken)
            .GetAsync($"{Endpoint}?accountId=not-a-guid", CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<PaginatedList<GetPaginatedAccountOperationsResponse>>(
            CancellationToken
        );

        // Assert
        result.ShouldBeProblem();
        result.Problem.ShouldNotBeNull();
        result.Problem.Status.ShouldBe(StatusCodes.Status400BadRequest);
        result.Problem.ShouldHaveValidationError("AccountId", AccountErrors.AccountIdInvalid.Code);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetPaginatedAccountOperations_ReturnsValidationError_WhenPageNumberIsZeroOrNegative(
        int pageNumber
    )
    {
        // Act
        var response = await ApiClient
            .LoggedAs(UserToken)
            .GetAsync($"{Endpoint}?pageNumber={pageNumber}", CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<PaginatedList<GetPaginatedAccountOperationsResponse>>(
            CancellationToken
        );

        // Assert
        result.ShouldBeProblem();
        result.Problem.ShouldNotBeNull();
        result.Problem.Status.ShouldBe(StatusCodes.Status400BadRequest);
        result.Problem.ShouldHaveValidationError("PageNumber", PaginationErrors.PageNumberInvalid.Code);
    }

    [Fact]
    public async Task GetPaginatedAccountOperations_ReturnsValidationError_WhenPageSizeExceedsMaximum()
    {
        // Act
        var response = await ApiClient
            .LoggedAs(UserToken)
            .GetAsync($"{Endpoint}?pageSize={PaginationConstants.MaxPageSize + 1}", CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<PaginatedList<GetPaginatedAccountOperationsResponse>>(
            CancellationToken
        );

        // Assert
        result.ShouldBeProblem();
        result.Problem.ShouldNotBeNull();
        result.Problem.Status.ShouldBe(StatusCodes.Status400BadRequest);
        result.Problem.ShouldHaveValidationError("PageSize", PaginationErrors.PageSizeTooLarge.Code);
    }

    [Fact]
    public async Task GetPaginatedAccountOperations_ReturnsValidationError_WhenAccountIdIsEmptyGuid()
    {
        // Act
        var response = await ApiClient
            .LoggedAs(UserToken)
            .GetAsync($"{Endpoint}?accountId={Guid.Empty}", CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<PaginatedList<GetPaginatedAccountOperationsResponse>>(
            CancellationToken
        );

        // Assert
        result.ShouldBeProblem();
        result.Problem.ShouldNotBeNull();
        result.Problem.Status.ShouldBe(StatusCodes.Status400BadRequest);
        result.Problem.ShouldHaveValidationError("AccountId", AccountErrors.AccountIdInvalid.Code);
    }

    [Fact]
    public async Task GetPaginatedAccountOperations_ReturnsValidationErrors_WhenMultipleFieldsAreInvalid()
    {
        // Act
        var response = await ApiClient
            .LoggedAs(UserToken)
            .GetAsync(
                $"{Endpoint}?pageNumber=-1&pageSize={PaginationConstants.MaxPageSize + 1}&accountId={Guid.Empty}",
                CancellationToken
            );
        var result = await response.ReadResponseOrProblemAsync<PaginatedList<GetPaginatedAccountOperationsResponse>>(
            CancellationToken
        );

        // Assert
        result.ShouldBeProblem();
        result.Problem.ShouldNotBeNull();
        result.Problem.Status.ShouldBe(StatusCodes.Status400BadRequest);
        result.Problem.ShouldHaveValidationErrors(
            ("PageNumber", PaginationErrors.PageNumberInvalid.Code),
            ("PageSize", PaginationErrors.PageSizeTooLarge.Code),
            ("AccountId", AccountErrors.AccountIdInvalid.Code)
        );
    }
}
