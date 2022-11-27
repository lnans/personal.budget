using Application.Features.Accounts.UpdateAccount;

namespace Api.IntegrationTests.Features.Accounts;

[Collection("Shared")]
public class UpdateAccountTests : TestBase
{
    public UpdateAccountTests(ApiFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task UpdateAccount_Should_UpdateAccount()
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
        var request = new UpdateAccountRequest { Name = "updated", Bank = "updated" };

        // Act
        var response = await Api.PatchAsJsonAsync($"accounts/{account.Id}", request);
        var accountInDb = await DbContext().Accounts.FirstOrDefaultAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        accountInDb!.Name.Should().Be(request.Name);
        accountInDb.Bank.Should().Be(request.Bank);
    }

    [Fact]
    public async Task UpdateAccount_WhenAlreadyExist_ShouldReturn_409Conflict()
    {
        // Arrange
        var dbContext = DbContext();
        var account1 = new Account
        {
            Name = "Account1",
            Bank = "Bank1",
            Balance = 0,
            InitialBalance = 0,
            OwnerId = FakeJwtManager.UserId,
            Type = AccountType.Expenses,
            CreationDate = DateTime.UtcNow
        };
        var account2 = new Account
        {
            Name = "Account2",
            Bank = "Bank2",
            Balance = 0,
            InitialBalance = 0,
            OwnerId = FakeJwtManager.UserId,
            Type = AccountType.Expenses,
            CreationDate = DateTime.UtcNow
        };
        await dbContext.Accounts.AddRangeAsync(account1, account2);
        await dbContext.SaveChangesAsync(CancellationToken.None);
        var request = new UpdateAccountRequest { Name = account2.Name, Bank = account2.Bank };

        // Act
        var response = await Api.PatchAsJsonAsync($"accounts/{account1.Id}", request);
        var accountInDb = await DbContext().Accounts.FirstOrDefaultAsync(a => a.Id == account1.Id);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        accountInDb!.Name.Should().NotBe(request.Name);
        accountInDb.Bank.Should().NotBe(request.Bank);
    }

    [Theory]
    [InlineData("", "")]
    [InlineData(null, null)]
    [InlineData("name", null)]
    [InlineData(null, "bank")]
    public async Task UpdateAccount_WhenFormisInvalid_ShouldReturn_400BadRequest(string name, string bank)
    {
        // Arrange
        var request = new UpdateAccountRequest
        {
            Name = name,
            Bank = bank
        };

        // Act
        var response = await Api.PatchAsJsonAsync($"accounts/{Guid.NewGuid()}", request);
        var result = await response.Content.ReadFromJsonOrDefaultAsync<ErrorResponse>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.Should().NotBeNull();
        result!.Errors.Should().NotBeEmpty();
    }
}