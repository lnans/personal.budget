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
        var command = new CreateAccountCommand { Name = "Test Account", InitialBalance = 100m };

        // Act
        var response = await ApiClient.LoggedAs(UserToken).PostAsJsonAsync(Endpoint, command, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<CreateAccountResponse>(CancellationToken);

        // Assert
        result.ShouldBeSuccessful();
        result.Response.ShouldNotBeNull();
        result.Response.Name.ShouldBe(command.Name);
        result.Response.Balance.ShouldBe(command.InitialBalance);
        result.Response.Id.ShouldNotBe(Guid.Empty);

        result.Response.CreatedAt.ShouldBeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
        result.Response.UpdatedAt.ShouldBe(result.Response.CreatedAt);
    }

    [Fact]
    public async Task CreateAccount_WithEmptyName_ShouldReturnValidationError()
    {
        // Arrange
        var command = new CreateAccountCommand { Name = "", InitialBalance = 100m };

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
        var command = new CreateAccountCommand { Name = "Test Account", InitialBalance = -50m };

        // Act
        var response = await ApiClient.LoggedAs(UserToken).PostAsJsonAsync(Endpoint, command, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<CreateAccountResponse>(CancellationToken);

        // Assert
        result.ShouldBeSuccessful();
        result.Response.ShouldNotBeNull();
        result.Response.Name.ShouldBe(command.Name);
        result.Response.Balance.ShouldBe(command.InitialBalance);
    }

    [Fact]
    public async Task CreateAccount_ShouldPersistInDatabase()
    {
        // Arrange
        var command = new CreateAccountCommand { Name = "Persistent Account", InitialBalance = 200m };

        // Act
        var response = await ApiClient.LoggedAs(UserToken).PostAsJsonAsync(Endpoint, command, CancellationToken);
        var result = await response.ReadResponseOrProblemAsync<CreateAccountResponse>(CancellationToken);

        // Assert
        result.ShouldBeSuccessful();
        result.Response.ShouldNotBeNull();

        var accountInDb = await DbContext.Accounts.FindAsync([result.Response.Id], CancellationToken);
        accountInDb.ShouldNotBeNull();
        accountInDb.Name.ShouldBe(command.Name);
        accountInDb.Balance.ShouldBe(command.InitialBalance);
        accountInDb.UserId.ShouldBe(User.Id);
        accountInDb.CreatedAt.ShouldBeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
        accountInDb.UpdatedAt.ShouldBe(accountInDb.CreatedAt);
    }
}
