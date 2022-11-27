using Application.Features.Operations.CreateOperations;

namespace Api.IntegrationTests.Features.Operations;

[Collection("Shared")]
public class CreateOperationsTests : TestBase
{
    public CreateOperationsTests(ApiFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task CreateOperations_Should_CreateOperationsAndUpdateAccountBalance()
    {
        // Arrange
        var dbContext = DbContext();
        var tag = new Tag { Name = "Tag", Color = "#123456", OwnerId = FakeJwtManager.UserId };
        var account = new Account
        {
            Name = "Account1",
            Bank = "Bank1",
            Balance = 0,
            InitialBalance = 0,
            OwnerId = FakeJwtManager.UserId,
            Type = AccountType.Expenses,
            CreationDate = DateTime.UtcNow
        };
        await dbContext.Accounts.AddAsync(account);
        await dbContext.Tags.AddAsync(tag);
        await dbContext.SaveChangesAsync(CancellationToken.None);
        var request = new CreateOperationsRequest
        {
            AccountId = account.Id,
            Operations = new[]
            {
                new CreateOperationData
                {
                    Description = "Test",
                    Amount = -10,
                    Type = OperationType.Budget,
                    CreationDate = DateTime.UtcNow,
                    TagIds = new[] { tag.Id }
                }
            }
        };

        // Act
        var response = await Api.PostAsJsonAsync("operations", request);
        var accountInDb = await DbContext().Accounts.FirstOrDefaultAsync();
        var operationInDb = await DbContext().Operations.FirstOrDefaultAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        accountInDb!.Balance.Should().Be(request.Operations.First().Amount);
        operationInDb.Should().NotBeNull();
        operationInDb!.Amount.Should().Be(request.Operations.First().Amount);
    }

    [Fact]
    public async Task CreateOperations_WhenAccountIdNotExist_ShouldReturn_404NotFound()
    {
        // Arrange
        var request = new CreateOperationsRequest
        {
            AccountId = Guid.NewGuid(),
            Operations = new[]
            {
                new CreateOperationData
                {
                    Description = "Test",
                    Amount = -10,
                    Type = OperationType.Budget,
                    CreationDate = DateTime.UtcNow
                }
            }
        };

        // Act
        var response = await Api.PostAsJsonAsync("operations", request);
        var operationInDb = await DbContext().Operations.FirstOrDefaultAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        operationInDb.Should().BeNull();
    }

    [Fact]
    public async Task CreateOperations_WhenTagIdNotExist_ShouldReturn_404NotFound()
    {
        // Arrange
        var dbContext = DbContext();
        var account = new Account
        {
            Name = "Account1",
            Bank = "Bank1",
            Balance = 0,
            InitialBalance = 0,
            OwnerId = FakeJwtManager.UserId,
            Type = AccountType.Expenses,
            CreationDate = DateTime.UtcNow
        };
        await dbContext.Accounts.AddAsync(account);
        var request = new CreateOperationsRequest
        {
            AccountId = account.Id,
            Operations = new[]
            {
                new CreateOperationData
                {
                    Description = "Test",
                    Amount = -10,
                    Type = OperationType.Budget,
                    CreationDate = DateTime.UtcNow,
                    TagIds = new[] { Guid.NewGuid() }
                }
            }
        };

        // Act
        var response = await Api.PostAsJsonAsync("operations", request);
        var operationInDb = await DbContext().Operations.FirstOrDefaultAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        operationInDb.Should().BeNull();
    }

    [Theory]
    [InlineData(0, null, OperationType.Budget, 10, 1)]
    [InlineData(1, null, OperationType.Budget, 10, 2)]
    [InlineData(1, "desc", OperationType.Budget, 10, 1)]
    [InlineData(1, "desc", (OperationType)10, 10, 2)]
    [InlineData(1, "desc", (OperationType)10, 0, 3)]
    public async Task CreateOperations_WhenFormIsInvalid_ShouldReturn_400BadRequest(int opCount, string description, OperationType type, decimal amount,
        int errorsCount)
    {
        // Arrange
        var request = new CreateOperationsRequest
        {
            AccountId = Guid.NewGuid(),
            Operations = opCount > 0
                ? Enumerable
                    .Range(0, opCount)
                    .Select(_ => new CreateOperationData
                    {
                        Description = description,
                        Type = type,
                        Amount = amount,
                        CreationDate = default
                    })
                    .ToArray()
                : Array.Empty<CreateOperationData>()
        };

        // Act
        var response = await Api.PostAsJsonAsync("operations", request);
        var result = await response.Content.ReadFromJsonOrDefaultAsync<ErrorResponse>();
        var operationInDb = await DbContext().Operations.FirstOrDefaultAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.Should().NotBeNull();
        result!.Errors.Length.Should().Be(errorsCount);
        operationInDb.Should().BeNull();
    }
}