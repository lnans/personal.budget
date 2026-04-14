using System.Net.Http.Json;
using Api.Contracts.Accounts;
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
        var request = new CreateAccountRequest("Test Account", "Test Bank", AccountType.Checking, 100m);

        // Act
        var response = await ApiClient.LoggedAs(UserToken).PostAsJsonAsync(Endpoint, request, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<CreateAccountResponse>(CancellationToken);

        // Assert
        result.ShouldBeSuccessful();
        result.Response.ShouldNotBeNull();
        result.Response.Name.ShouldBe(request.Name);
        result.Response.Bank.ShouldBe(request.Bank);
        result.Response.Type.ShouldBe(request.Type);
        result.Response.InitialBalance.ShouldBe(request.InitialBalance);
        result.Response.Balance.ShouldBe(request.InitialBalance);
        result.Response.Id.ShouldNotBe(Guid.Empty);

        result.Response.CreatedAt.ShouldBeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
        result.Response.UpdatedAt.ShouldBeCloseTo(result.Response.CreatedAt, TimeSpan.FromMilliseconds(1));
    }

    [Fact]
    public async Task CreateAccount_WithEmptyName_ShouldReturnValidationError()
    {
        // Arrange
        var request = new CreateAccountRequest("", "Test Bank", AccountType.Checking, 100m);

        // Act
        var response = await ApiClient.LoggedAs(UserToken).PostAsJsonAsync(Endpoint, request, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<CreateAccountResponse>(CancellationToken);

        // Assert
        result.ShouldBeProblem();
        result.Problem.ShouldNotBeNull();
        result.Problem.Status.ShouldBe(400);
        result.Problem.ShouldHaveError(AccountErrors.AccountNameRequired.Code);
    }

    [Fact]
    public async Task CreateAccount_WithTooLongName_ShouldReturnValidationError()
    {
        // Arrange
        var request = new CreateAccountRequest(
            new string('a', AccountConstants.MaxNameLength + 1),
            "Test Bank",
            AccountType.Checking,
            100m
        );

        // Act
        var response = await ApiClient.LoggedAs(UserToken).PostAsJsonAsync(Endpoint, request, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<CreateAccountResponse>(CancellationToken);

        // Assert
        result.ShouldBeProblem();
        result.Problem.ShouldNotBeNull();
        result.Problem.Status.ShouldBe(400);
        result.Problem.ShouldHaveError(AccountErrors.AccountNameTooLong.Code);
    }

    [Fact]
    public async Task CreateAccount_WithEmptyBank_ShouldReturnValidationError()
    {
        // Arrange
        var request = new CreateAccountRequest("Test Account", "", AccountType.Checking, 100m);

        // Act
        var response = await ApiClient.LoggedAs(UserToken).PostAsJsonAsync(Endpoint, request, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<CreateAccountResponse>(CancellationToken);

        // Assert
        result.ShouldBeProblem();
        result.Problem.ShouldNotBeNull();
        result.Problem.Status.ShouldBe(400);
        result.Problem.ShouldHaveError(AccountErrors.AccountBankRequired.Code);
    }

    [Fact]
    public async Task CreateAccount_WithTooLongBank_ShouldReturnValidationError()
    {
        // Arrange
        var request = new CreateAccountRequest(
            "Test Account",
            new string('b', AccountConstants.MaxBankLength + 1),
            AccountType.Checking,
            100m
        );

        // Act
        var response = await ApiClient.LoggedAs(UserToken).PostAsJsonAsync(Endpoint, request, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<CreateAccountResponse>(CancellationToken);

        // Assert
        result.ShouldBeProblem();
        result.Problem.ShouldNotBeNull();
        result.Problem.Status.ShouldBe(400);
        result.Problem.ShouldHaveError(AccountErrors.AccountBankTooLong.Code);
    }

    [Fact]
    public async Task CreateAccount_WithNegativeBalance_ShouldCreateAccount()
    {
        // Arrange
        var request = new CreateAccountRequest("Test Account", "Test Bank", AccountType.Savings, -50m);

        // Act
        var response = await ApiClient.LoggedAs(UserToken).PostAsJsonAsync(Endpoint, request, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<CreateAccountResponse>(CancellationToken);

        // Assert
        result.ShouldBeSuccessful();
        result.Response.ShouldNotBeNull();
        result.Response.Name.ShouldBe(request.Name);
        result.Response.Bank.ShouldBe(request.Bank);
        result.Response.Type.ShouldBe(request.Type);
        result.Response.InitialBalance.ShouldBe(request.InitialBalance);
        result.Response.Balance.ShouldBe(request.InitialBalance);
    }

    [Fact]
    public async Task CreateAccount_ShouldPersistInDatabase()
    {
        // Arrange
        var request = new CreateAccountRequest("Persistent Account", "Persistent Bank", AccountType.Savings, 200m);

        // Act
        var response = await ApiClient.LoggedAs(UserToken).PostAsJsonAsync(Endpoint, request, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<CreateAccountResponse>(CancellationToken);

        // Assert
        result.ShouldBeSuccessful();
        result.Response.ShouldNotBeNull();

        var accountInDb = await DbContext.Accounts.FindAsync([result.Response.Id], CancellationToken);
        accountInDb.ShouldNotBeNull();
        accountInDb.Name.ShouldBe(request.Name);
        accountInDb.Bank.ShouldBe(request.Bank);
        accountInDb.Type.ShouldBe(request.Type);
        accountInDb.InitialBalance.ShouldBe(request.InitialBalance);
        accountInDb.Balance.ShouldBe(request.InitialBalance);
        accountInDb.UserId.ShouldBe(User.Id);
        accountInDb.CreatedAt.ShouldBeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
        accountInDb.UpdatedAt.ShouldBeCloseTo(accountInDb.CreatedAt, TimeSpan.FromMilliseconds(1));
    }

    [Fact]
    public async Task CreateAccount_WithInvalidAccountType_ShouldReturnValidationError()
    {
        // Arrange
        var json = """
            {
                "name": "Test Account",
                "bank": "Test Bank",
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
        result.Problem.ShouldHaveError(AccountErrors.AccountTypeUnknown.Code);
    }
}
