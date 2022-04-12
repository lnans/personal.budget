using System;
using System.Collections;
using System.Linq;
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
    
    public static string ToQueryString(this object request, string separator = ",")
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        // Get all properties on the object
        var properties = request.GetType().GetProperties()
            .Where(x => x.CanRead)
            .Where(x => x.GetValue(request, null) != null)
            .ToDictionary(x => x.Name, x => x.GetValue(request, null));

        // Get names for all IEnumerable properties (excl. string)
        var propertyNames = properties
            .Where(x => !(x.Value is string) && x.Value is IEnumerable)
            .Select(x => x.Key)
            .ToList();

        // Concat all IEnumerable properties into a comma separated string
        foreach (var key in from key in propertyNames let valueType = properties[key]?.GetType() let valueElemType = valueType is {IsGenericType: true}
                     ? valueType.GetGenericArguments()[0]
                     : valueType?.GetElementType() where valueElemType != null && (valueElemType.IsPrimitive || valueElemType == typeof(string)) select key)
        {
            if (properties[key] is IEnumerable enumerable) properties[key] = string.Join(separator, enumerable.Cast<object>());
        }

        // Concat all key/value pairs into a string separated by ampersand
        return string.Join("&", properties
            .Select(x => string.Concat(
                Uri.EscapeDataString(x.Key), "=",
                Uri.EscapeDataString(x.Value.ToString()))));
    }
}