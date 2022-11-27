using Application.Features.Operations.GetOperations;

namespace Api.IntegrationTests.Features.Operations;

[Collection("Shared")]
public class GetOperationsTests : TestBase
{
    public GetOperationsTests(ApiFactory factory) : base(factory)
    {
    }

    [Theory]
    [InlineData(null, null, null, 100)]
    [InlineData(0, null, null, 50)]
    [InlineData(1, null, null, 50)]
    [InlineData(null, "5", null, 19)]
    [InlineData(null, "op", null, 100)]
    [InlineData(null, null, OperationType.Expense, 20)]
    public async Task GetOperations_WithFilters_ShouldReturn_InfiniteDataListOfOperations(int? account, string? description, OperationType? type,
        int resultCount)
    {
        // Arrange
        var dbContext = DbContext();
        var account1 = new Account
        {
            Name = "Account1",
            Bank = "Bank1",
            Balance = -10,
            InitialBalance = -10,
            OwnerId = FakeJwtManager.UserId,
            Type = AccountType.Expenses,
            CreationDate = DateTime.UtcNow
        };
        var account2 = new Account
        {
            Name = "Account2",
            Bank = "Bank1",
            Balance = -10,
            InitialBalance = -10,
            OwnerId = FakeJwtManager.UserId,
            Type = AccountType.Expenses,
            CreationDate = DateTime.UtcNow
        };
        var accounts = new[] { account1, account2 };
        var operations = Enumerable
            .Range(0, 100)
            .Select(arg => new Operation
            {
                Account = arg % 2 == 0 ? account1 : account2,
                Description = $"op {arg}",
                Amount = -10,
                Type = (OperationType)(arg % 5),
                CreationDate = DateTime.UtcNow,
                OwnerId = FakeJwtManager.UserId
            }).ToList();
        await dbContext.Operations.AddRangeAsync(operations);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var request = new GetOperationsRequest
        {
            Description = description,
            Type = type,
            AccountId = account != null ? accounts[account.Value].Id : null,
            TagIds = null,
            PageSize = 100
        };

        // Act
        var response = await Api.GetAsync($"operations?{request.ToQueryString()}");
        var result = await response.Content.ReadFromJsonOrDefaultAsync<InfiniteDataList<GetOperationsResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Items.Count.Should().Be(resultCount);
    }
}