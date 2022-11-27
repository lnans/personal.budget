using Application.Features.Accounts.ArchiveAccount;

namespace Api.IntegrationTests.Features.Accounts;

[Collection("Shared")]
public class ArchiveAccountTests : TestBase
{
    public ArchiveAccountTests(ApiFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task ArchiveAccount_Should_ChangeAccountState()
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
            Archived = false
        };
        await dbContext.Accounts.AddAsync(account);
        await dbContext.SaveChangesAsync(CancellationToken.None);
        var request = new ArchiveAccountRequest { Archived = true };

        // Act
        var response = await Api.PatchAsJsonAsync($"accounts/{account.Id}/archive", request);
        var accountInDb = await DbContext().Accounts.FirstOrDefaultAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        accountInDb!.Archived.Should().Be(request.Archived);
    }

    [Fact]
    public async Task ArchiveAccount_WhenIdNotExist_ShouldReturn_404NotFound()
    {
        // Arrange
        var request = new ArchiveAccountRequest { Archived = true };

        // Act
        var response = await Api.PatchAsJsonAsync("accounts/fake/archive", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}