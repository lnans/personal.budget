namespace Api.IntegrationTests.Features.Operations;

[Collection("Shared")]
public class DeleteOperationTests : TestBase
{
    public DeleteOperationTests(ApiFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task DeleteOperation_Should_DeleteOperationAndUpdateAccountBalance()
    {
        // Arrange
        var dbContext = DbContext();
        var account = new Account
        {
            Name = "Account1",
            Bank = "Bank1",
            Balance = -10,
            InitialBalance = -10,
            OwnerId = FakeJwtManager.UserId,
            Type = AccountType.Expenses,
            CreationDate = DateTime.UtcNow,
            Operations = new List<Operation>
            {
                new()
                {
                    Description = "Test",
                    Amount = -10,
                    Type = OperationType.Budget,
                    CreationDate = DateTime.UtcNow,
                    OwnerId = FakeJwtManager.UserId
                }
            }
        };
        await dbContext.Accounts.AddAsync(account);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        // Act
        var response = await Api.DeleteAsync($"operations/{account.Operations.First().Id}");
        var accountInDb = await DbContext().Accounts.FirstOrDefaultAsync();
        var operationInDb = await DbContext().Operations.FirstOrDefaultAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        accountInDb!.Balance.Should().Be(0);
        operationInDb.Should().BeNull();
    }

    [Fact]
    public async Task DeleteOperation_WhenIdNotExist_ShouldReturn_404NotFound()
    {
        // Act
        var response = await Api.DeleteAsync($"operations/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}