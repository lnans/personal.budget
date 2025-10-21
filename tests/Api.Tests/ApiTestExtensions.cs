using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace Api.Tests;

public static class ApiTestExtensions
{
    public static HttpClient LoggedAs(this HttpClient client, string bearerToken)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
        return client;
    }

    public static async Task<(TResponse? Response, ProblemDetails? Problem)> ReadResponseOrProblemAsync<TResponse>(
        this HttpResponseMessage responseMessage,
        CancellationToken cancellationToken = default
    )
    {
        if (responseMessage.IsSuccessStatusCode)
        {
            var response = await responseMessage.Content.ReadFromJsonAsync<TResponse>(cancellationToken);
            return (response, null);
        }

        var problem = await responseMessage.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken);
        return (default, problem);
    }

    public static void ShouldBeSuccessful<TResponse>(this (TResponse? Response, ProblemDetails? Problem) result)
    {
        if (result.Problem is not null)
        {
            var problemJson = JsonSerializer.Serialize(
                result.Problem,
                new JsonSerializerOptions { WriteIndented = true }
            );
            throw new InvalidOperationException($"Expected successful response but got ProblemDetails:\n{problemJson}");
        }
    }

    public static void ShouldBeProblem<TResponse>(this (TResponse? Response, ProblemDetails? Problem) result)
    {
        if (result.Problem is null)
        {
            throw new InvalidOperationException("Expected ProblemDetails but got successful response.");
        }
    }
}
