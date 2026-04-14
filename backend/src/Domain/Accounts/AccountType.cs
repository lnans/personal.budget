using System.Text.Json.Serialization;

namespace Domain.Accounts;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AccountType
{
    Checking,
    Savings,
}
