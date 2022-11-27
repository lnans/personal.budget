using Application.Features.Accounts.GetAccounts;

namespace Api.IntegrationTests.Features.Accounts;

[Collection("Shared")]
public class GetAccountsTests : TestBase
{
    public GetAccountsTests(ApiFactory factory) : base(factory)
    {
    }

    [Theory]
    [InlineData(false, 10)]
    [InlineData(true, 10)]
    public async Task GetAllAccounts_ShouldReturn_AccountsList(bool archived, int excepted)
    {
        // Arrange
        var dbContext = DbContext();
        var accounts = new List<Account>();
        Enumerable
            .Range(0, 20)
            .ToList()
            .ForEach(arg => accounts.Add(new Account
            {
                Name = $"string{arg}",
                Bank = "bank",
                Balance = 0,
                InitialBalance = 0,
                OwnerId = FakeJwtManager.UserId,
                Type = AccountType.Expenses,
                CreationDate = DateTime.UtcNow,
                Archived = arg % 2 == 0
            }));
        await dbContext.Accounts.AddRangeAsync(accounts);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        // Act
        var response = await Api.GetAsync($"accounts?archived={archived}");
        var result = await response.Content.ReadFromJsonOrDefaultAsync<IEnumerable<GetAccountsResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.Count().Should().Be(excepted);
    }
}