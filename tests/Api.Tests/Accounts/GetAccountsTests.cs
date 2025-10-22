using Application.Features.Accounts.Queries.GetAccounts;
using TestFixtures.Domain;

namespace Api.Tests.Accounts;

[Collection(ApiTestCollection.CollectionName)]
public class GetAccountsTests : ApiTestBase
{
    private const string Endpoint = "/accounts";

    public GetAccountsTests(ApiTestFixture factory)
        : base(factory) { }

    [Fact]
    public async Task GetAccounts_ReturnsEmptyList_WhenNoAccountsExist()
    {
        // Act
        var response = await ApiClient.LoggedAs(UserToken).GetAsync(Endpoint, CancellationToken.None);
        var result = await response.ReadResponseOrProblemAsync<List<GetAccountsResponse>>(CancellationToken);

        // Assert
        result.ShouldBeSuccessful();
        result.Response.ShouldNotBeNull();
        result.Response.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetAccounts_ReturnsAccountsList_WhenAccountsExist()
    {
        // Arrange
        var account1 = AccountFixture.CreateValidAccount(User.Id);
        var account2 = AccountFixture.CreateValidAccount(User.Id);
        DbContext.Accounts.AddRange(account1, account2);
        await DbContext.SaveChangesAsync(CancellationToken.None);

        // Act
        var response = await ApiClient.LoggedAs(UserToken).GetAsync(Endpoint, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<List<GetAccountsResponse>>(CancellationToken);

        // Assert
        result.ShouldBeSuccessful();
        result.Response.ShouldNotBeNull();
        result.Response.Count.ShouldBe(2);

        var resultAccount1 = result.Response.FirstOrDefault(account => account.Id == account1.Id);
        resultAccount1.ShouldNotBeNull();
        resultAccount1.Name.ShouldBe(account1.Name);
        resultAccount1.Type.ShouldBe(account1.Type);
        resultAccount1.Balance.ShouldBe(account1.Balance);
        resultAccount1.CreatedAt.ShouldBe(account1.CreatedAt);
        resultAccount1.UpdatedAt.ShouldBe(account1.UpdatedAt);

        var resultAccount2 = result.Response.FirstOrDefault(account => account.Id == account2.Id);
        resultAccount2.ShouldNotBeNull();
        resultAccount2.Name.ShouldBe(account2.Name);
        resultAccount2.Type.ShouldBe(account2.Type);
        resultAccount2.Balance.ShouldBe(account2.Balance);
        resultAccount2.CreatedAt.ShouldBe(account2.CreatedAt);
        resultAccount2.UpdatedAt.ShouldBe(account2.UpdatedAt);
    }
}
