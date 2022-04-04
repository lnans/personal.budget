using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Api.IntegrationTests;

public static class Extensions
{
    public static async Task<T?> ReadFromJsonOrDefaultAsync<T>(this HttpContent httpContent)
    {
        try
        {
            return await httpContent.ReadFromJsonAsync<T>();
        }
        catch (Exception)
        {
            return default;
        }
    }

    public static Task<HttpResponseMessage> PatchAsJsonAsync<TValue>(this HttpClient client, string? requestUri, TValue value)
    {
        if (client == null)
        {
            throw new ArgumentNullException(nameof(client));
        }

        var content = JsonContent.Create(value);
        return client.PatchAsync(requestUri, content);
    }
}