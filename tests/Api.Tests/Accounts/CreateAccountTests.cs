using System.Net.Http.Json;
using Application.Features.Accounts.Commands.CreateAccount;
using Domain.Accounts;

namespace Api.Tests.Accounts;

[Collection(ApiTestCollection.CollectionName)]
public class CreateAccountTests : ApiTestBase
{
    private const string Endpoint = "/accounts";

    public CreateAccountTests(ApiTestFixture factory)
        : base(factory) { }

    [Fact]
    public async Task CreateAccount_WithValidData_ShouldCreateAccount()
    {
        // Arrange
        var command = new CreateAccountCommand
        {
            Name = "Test Account",
            Type = AccountType.Checking,
            InitialBalance = 100m,
        };

        // Act
        var response = await ApiClient.LoggedAs(UserToken).PostAsJsonAsync(Endpoint, command, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<CreateAccountResponse>(CancellationToken);

        // Assert
        result.ShouldBeSuccessful();
        result.Response.ShouldNotBeNull();
        result.Response.Name.ShouldBe(command.Name);
        result.Response.Type.ShouldBe(command.Type);
        result.Response.Balance.ShouldBe(command.InitialBalance);
        result.Response.Id.ShouldNotBe(Guid.Empty);

        result.Response.CreatedAt.ShouldBeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
        result.Response.UpdatedAt.ShouldBe(result.Response.CreatedAt);
    }

    [Fact]
    public async Task CreateAccount_WithEmptyName_ShouldReturnValidationError()
    {
        // Arrange
        var command = new CreateAccountCommand
        {
            Name = "",
            Type = AccountType.Checking,
            InitialBalance = 100m,
        };

        // Act
        var response = await ApiClient.LoggedAs(UserToken).PostAsJsonAsync(Endpoint, command, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<CreateAccountResponse>(CancellationToken);

        // Assert
        result.ShouldBeProblem();
        result.Problem.ShouldNotBeNull();
        result.Problem.Status.ShouldBe(400);
        result.Problem.ShouldHaveValidationError("Name", AccountErrors.AccountNameRequired.Code);
    }

    [Fact]
    public async Task CreateAccount_WithTooLongName_ShouldReturnValidationError()
    {
        // Arrange
        var command = new CreateAccountCommand
        {
            Name = new string('a', AccountConstants.MaxNameLength + 1),
            Type = AccountType.Checking,
            InitialBalance = 100m,
        };

        // Act
        var response = await ApiClient.LoggedAs(UserToken).PostAsJsonAsync(Endpoint, command, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<CreateAccountResponse>(CancellationToken);

        // Assert
        result.ShouldBeProblem();
        result.Problem.ShouldNotBeNull();
        result.Problem.Status.ShouldBe(400);
        result.Problem.ShouldHaveValidationError("Name", AccountErrors.AccountNameTooLong.Code);
    }

    [Fact]
    public async Task CreateAccount_WithNegativeBalance_ShouldCreateAccount()
    {
        // Arrange
        var command = new CreateAccountCommand
        {
            Name = "Test Account",
            Type = AccountType.Savings,
            InitialBalance = -50m,
        };

        // Act
        var response = await ApiClient.LoggedAs(UserToken).PostAsJsonAsync(Endpoint, command, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<CreateAccountResponse>(CancellationToken);

        // Assert
        result.ShouldBeSuccessful();
        result.Response.ShouldNotBeNull();
        result.Response.Name.ShouldBe(command.Name);
        result.Response.Type.ShouldBe(command.Type);
        result.Response.Balance.ShouldBe(command.InitialBalance);
    }

    [Fact]
    public async Task CreateAccount_ShouldPersistInDatabase()
    {
        // Arrange
        var command = new CreateAccountCommand
        {
            Name = "Persistent Account",
            Type = AccountType.Savings,
            InitialBalance = 200m,
        };

        // Act
        var response = await ApiClient.LoggedAs(UserToken).PostAsJsonAsync(Endpoint, command, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<CreateAccountResponse>(CancellationToken);

        // Assert
        result.ShouldBeSuccessful();
        result.Response.ShouldNotBeNull();

        var accountInDb = await DbContext.Accounts.FindAsync([result.Response.Id], CancellationToken);
        accountInDb.ShouldNotBeNull();
        accountInDb.Name.ShouldBe(command.Name);
        accountInDb.Type.ShouldBe(command.Type);
        accountInDb.Balance.ShouldBe(command.InitialBalance);
        accountInDb.UserId.ShouldBe(User.Id);
        accountInDb.CreatedAt.ShouldBeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
        accountInDb.UpdatedAt.ShouldBe(accountInDb.CreatedAt);
    }

    [Fact]
    public async Task CreateAccount_WithInvalidAccountType_ShouldReturnValidationError()
    {
        // Arrange
        var json = """
            {
                "name": "Test Account",
                "type": 999,
                "initialBalance": 100
            }
            """;
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await ApiClient.LoggedAs(UserToken).PostAsync(Endpoint, content, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<CreateAccountResponse>(CancellationToken);

        // Assert
        result.ShouldBeProblem();
        result.Problem.ShouldNotBeNull();
        result.Problem.Status.ShouldBe(400);
        result.Problem.ShouldHaveValidationError("Type", AccountErrors.AccountTypeUnknown.Code);
    }
}
