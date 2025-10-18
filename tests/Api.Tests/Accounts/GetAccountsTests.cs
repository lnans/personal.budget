using System.Net.Http.Json;
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
        var response = await ApiClient.GetAsync(Endpoint, CancellationToken.None);
        var result = await response.Content.ReadFromJsonAsync<List<GetAccountsResponse>>(CancellationToken);

        // Assert
        response.EnsureSuccessStatusCode();
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetAccounts_ReturnsAccountsList_WhenAccountsExist()
    {
        // Arrange
        var account1 = AccountFixture.CreateValidAccount();
        var account2 = AccountFixture.CreateValidAccount();
        DbContext.Accounts.AddRange(account1, account2);
        await DbContext.SaveChangesAsync(CancellationToken.None);

        // Act
        var response = await ApiClient.GetAsync(Endpoint, CancellationToken);
        var result = await response.Content.ReadFromJsonAsync<List<GetAccountsResponse>>(CancellationToken);

        // Assert
        response.EnsureSuccessStatusCode();
        result.ShouldNotBeNull();
        result.Count.ShouldBe(2);

        var resultAccount1 = result.FirstOrDefault(account => account.Id == account1.Id);
        resultAccount1.ShouldNotBeNull();
        resultAccount1.Name.ShouldBe(account1.Name);
        resultAccount1.Balance.ShouldBe(account1.Balance);
        resultAccount1.CreatedAt.ShouldBe(account1.CreatedAt);
        resultAccount1.UpdatedAt.ShouldBe(account1.UpdatedAt);

        var resultAccount2 = result.FirstOrDefault(account => account.Id == account2.Id);
        resultAccount2.ShouldNotBeNull();
        resultAccount2.Name.ShouldBe(account2.Name);
        resultAccount2.Balance.ShouldBe(account2.Balance);
        resultAccount2.CreatedAt.ShouldBe(account2.CreatedAt);
        resultAccount2.UpdatedAt.ShouldBe(account2.UpdatedAt);
    }
}
