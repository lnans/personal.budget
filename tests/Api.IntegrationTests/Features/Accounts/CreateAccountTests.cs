using Application.Features.Accounts.CreateAccount;

namespace Api.IntegrationTests.Features.Accounts;

[Collection("Shared")]
public class CreateAccountTests : TestBase
{
    public CreateAccountTests(ApiFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task CreateAccount_Should_CreateAccount()
    {
        // Arrange
        var request = new CreateAccountRequest
        {
            Name = "Account",
            Bank = "Bank",
            Type = AccountType.Expenses,
            InitialBalance = 0
        };

        // Act
        var response = await Api.PostAsJsonAsync("accounts", request);
        var accountInDb = await DbContext().Accounts.FirstOrDefaultAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        accountInDb.Should().NotBeNull();
        accountInDb?.Name.Should().Be(request.Name);
        accountInDb?.Bank.Should().Be(request.Bank);
        accountInDb?.Type.Should().Be(request.Type);
        accountInDb?.InitialBalance.Should().Be(request.InitialBalance);
        accountInDb?.Balance.Should().Be(request.InitialBalance);
        accountInDb?.OwnerId.Should().Be(FakeJwtManager.UserId);
    }

    [Fact]
    public async Task CreateAccount_WhenAlreadyExist_ShouldReturn_409Conflict()
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
            CreationDate = DateTime.UtcNow
        };
        await dbContext.Accounts.AddAsync(account);
        await dbContext.SaveChangesAsync(CancellationToken.None);
        var request = new CreateAccountRequest
        {
            Name = "Account",
            Bank = "Bank",
            Type = AccountType.Expenses,
            InitialBalance = 0
        };

        // Act
        var response = await Api.PostAsJsonAsync("accounts", request);
        var result = await response.Content.ReadFromJsonOrDefaultAsync<ErrorResponse>();
        var accountsCount = await DbContext().Accounts.CountAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        accountsCount.Should().Be(1);
        result.Should().NotBeNull();
        result!.Errors.Should().NotBeEmpty();
    }

    [Theory]
    [InlineData("", "")]
    [InlineData(null, null)]
    [InlineData("name", null)]
    [InlineData(null, "bank")]
    public async Task CreateAccount_WhenFormIsInvalid_ShouldReturn_400BadRequest(string name, string bank)
    {
        // Arrange
        var request = new CreateAccountRequest
        {
            Name = name,
            Bank = bank,
            Type = AccountType.Expenses,
            InitialBalance = 0
        };

        // Act
        var response = await Api.PostAsJsonAsync("accounts", request);
        var result = await response.Content.ReadFromJsonOrDefaultAsync<ErrorResponse>();
        var accountInDb = await DbContext().Accounts.FirstOrDefaultAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        accountInDb.Should().BeNull();
        result.Should().NotBeNull();
        result!.Errors.Should().NotBeEmpty();
    }
}