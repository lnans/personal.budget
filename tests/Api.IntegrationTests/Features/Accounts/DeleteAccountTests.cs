namespace Api.IntegrationTests.Features.Accounts;

[Collection("Shared")]
public class DeleteAccountTests : TestBase
{
    public DeleteAccountTests(ApiFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task DeleteAccount_Should_DeleteAccountAndLinkedOperations()
    {
        // Arrange
        var dbContext = DbContext();
        var account = new Account
        {
            Name = "Account",
            Bank = "Bank",
            Balance = 0,
            InitialBalance = 0,
            OwnerId = FakeJwtManager.UserId,
            Type = AccountType.Expenses,
            CreationDate = DateTime.UtcNow,
            Operations = new List<Operation>
            {
                new()
                {
                    Amount = 0,
                    Type = OperationType.Budget,
                    CreationDate = DateTime.UtcNow,
                    OwnerId = FakeJwtManager.UserId,
                    Description = "test"
                }
            }
        };
        await dbContext.Accounts.AddAsync(account);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        // Act
        var response = await Api.DeleteAsync($"accounts/{account.Id}");
        var accountsCount = await DbContext().Accounts.CountAsync();
        var operationsCount = await DbContext().Operations.CountAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        accountsCount.Should().Be(0);
        operationsCount.Should().Be(0);
    }

    [Fact]
    public async Task DeleteAccount_WhenIdNotExist_ShouldReturn_404NotFound()
    {
        // Act
        var response = await Api.DeleteAsync("accounts/fake");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}