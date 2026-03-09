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

    public static void ShouldHaveError(this ProblemDetails problem, string errorCode)
    {
        if (problem is null)
        {
            throw new InvalidOperationException("ProblemDetails is null.");
        }

        if (!problem.Extensions.TryGetValue("errors", out var errorsObj) || errorsObj is not JsonElement errorsJson)
        {
            throw new InvalidOperationException("ProblemDetails does not contain validation errors.");
        }

        var fieldError = errorsJson.EnumerateObject().FirstOrDefault(p => p.Name == errorCode);
        if (fieldError.Value.ValueKind == JsonValueKind.Undefined)
        {
            var availableErrors = string.Join(", ", errorsJson.EnumerateObject().Select(p => p.Name));
            throw new InvalidOperationException(
                $"Field '{errorCode}' not found in validation errors. Available errors: {availableErrors}"
            );
        }
    }

    public static void ShouldHaveErrors(this ProblemDetails problem, params string[] expectedErrors)
    {
        foreach (var errorCode in expectedErrors)
        {
            problem.ShouldHaveError(errorCode);
        }
    }

    public static void ShouldBeCloseTo(this DateTimeOffset actual, DateTimeOffset expected, TimeSpan? tolerance = null)
    {
        var actualTolerance = tolerance ?? TimeSpan.FromSeconds(1);
        var difference = (actual - expected).Duration();

        if (difference >= actualTolerance)
        {
            throw new InvalidOperationException(
                $"Expected {actual:O} to be close to {expected:O} (within {actualTolerance}), but difference was {difference}"
            );
        }
    }
}
