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

    public static void ShouldHaveValidationError(this ProblemDetails problem, string fieldName, string errorCode)
    {
        if (problem is null)
        {
            throw new InvalidOperationException("ProblemDetails is null.");
        }

        if (!problem.Extensions.TryGetValue("errors", out var errorsObj) || errorsObj is not JsonElement errorsJson)
        {
            throw new InvalidOperationException("ProblemDetails does not contain validation errors.");
        }

        var fieldErrors = errorsJson.EnumerateObject().FirstOrDefault(p => p.Name == fieldName);
        if (fieldErrors.Value.ValueKind == JsonValueKind.Undefined)
        {
            var availableFields = string.Join(", ", errorsJson.EnumerateObject().Select(p => p.Name));
            throw new InvalidOperationException(
                $"Field '{fieldName}' not found in validation errors. Available fields: {availableFields}"
            );
        }

        var hasError = fieldErrors.Value.EnumerateArray().Any(e => e.GetProperty("code").GetString() == errorCode);
        if (!hasError)
        {
            var availableCodes = string.Join(
                ", ",
                fieldErrors.Value.EnumerateArray().Select(e => e.GetProperty("code").GetString())
            );
            throw new InvalidOperationException(
                $"Error code '{errorCode}' not found for field '{fieldName}'. Available codes: {availableCodes}"
            );
        }
    }

    public static void ShouldHaveValidationErrors(
        this ProblemDetails problem,
        params (string FieldName, string ErrorCode)[] expectedErrors
    )
    {
        foreach (var (fieldName, errorCode) in expectedErrors)
        {
            problem.ShouldHaveValidationError(fieldName, errorCode);
        }
    }
}
