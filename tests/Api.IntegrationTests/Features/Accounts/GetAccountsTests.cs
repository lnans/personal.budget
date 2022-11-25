using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Application.Features.Accounts.GetAccounts;
using FluentAssertions;
using Xunit;

namespace Api.IntegrationTests.Features.Accounts;

[Collection("Shared")]
public class GetAccountsTests : IAsyncLifetime
{
    private readonly HttpClient _api;
    private readonly Func<Task> _resetDatabase;

    public GetAccountsTests(ApiFactory factory)
    {
        _api = factory.ApiClient;
        _resetDatabase = factory.ResetDatabaseAsync;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => _resetDatabase();

    [Fact]
    public async Task Test()
    {
        // Arrange
        var request = new GetAccountsRequest();

        // Act
        var response = await _api.PostAsJsonAsync("accounts", request);
        var responseText = await response.Content.ReadAsStringAsync();
        //var result = JsonSerializer.Deserialize<GetAccountsResponse>(responseText);

        // Assert
        response.StatusCode
            .Should().Be(HttpStatusCode.OK);
    }
}