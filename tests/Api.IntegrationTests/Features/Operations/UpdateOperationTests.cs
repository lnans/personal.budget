using Application.Features.Operations.UpdateOperation;

namespace Api.IntegrationTests.Features.Operations;

[Collection("Shared")]
public class UpdateOperationTests : TestBase
{
    public UpdateOperationTests(ApiFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task UpdateOperation_Should_UpdateOperationAndAccountBalance()
    {
        // Arrange
        var dbContext = DbContext();
        var tag = new Tag { Name = "Tag", Color = "#123456", OwnerId = FakeJwtManager.UserId };
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
                    OwnerId = FakeJwtManager.UserId,
                    Tags = new[] { tag }
                }
            }
        };
        await dbContext.Accounts.AddAsync(account);
        await dbContext.SaveChangesAsync(CancellationToken.None);
        var request = new UpdateOperationRequest
        {
            Description = "updated",
            Amount = -20,
            CreationDate = DateTime.UtcNow.AddDays(-1),
            TagIds = new[] { tag.Id }
        };

        // Act
        var response = await Api.PatchAsJsonAsync($"operations/{account.Operations.First().Id}", request);
        var accountInDb = await DbContext().Accounts.FirstOrDefaultAsync();
        var operationInDb = await DbContext().Operations.FirstOrDefaultAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        accountInDb!.Balance.Should().Be(request.Amount);
        operationInDb!.Description.Should().Be(request.Description);
        operationInDb.Amount.Should().Be(request.Amount);
    }

    [Fact]
    public async Task UpdateOperation_WhenIdNotExist_ShouldReturn_404NotFound()
    {
        // Arrange
        var request = new UpdateOperationRequest
        {
            Description = "updated",
            Amount = -20,
            CreationDate = DateTime.UtcNow.AddDays(-1)
        };

        // Act
        var response = await Api.PatchAsJsonAsync($"operations/{Guid.NewGuid()}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData(null, 10, 2)]
    [InlineData("desc", 10, 1)]
    [InlineData("desc", 0, 2)]
    [InlineData(null, 0, 3)]
    public async Task UpdateOperation_WhenFormIsInvalid_ShouldReturn_400BadRequest(string description, decimal amount, int errorsCounts)
    {
        // Arrange
        var request = new UpdateOperationRequest
        {
            Description = description,
            Amount = amount,
            CreationDate = default
        };

        // Act
        var response = await Api.PatchAsJsonAsync($"operations/{Guid.NewGuid()}", request);
        var result = await response.Content.ReadFromJsonOrDefaultAsync<ErrorResponse>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.Should().NotBeNull();
        result!.Errors.Length.Should().Be(errorsCounts);
    }
}