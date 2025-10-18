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
        var response = await ApiClient.GetAsync(
            Endpoint,
            CancellationToken.None
        );
        response.EnsureSuccessStatusCode();
    }
}
