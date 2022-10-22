using System.Text.Json.Serialization;

namespace Personal.Budget.Api.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AccountType
{
    Expenses,
    Savings
}