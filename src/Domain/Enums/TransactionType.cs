using System.Text.Json.Serialization;

namespace Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TransactionType
{
    Expense,
    Income,
    Fixed,
    Transfer,
    Budget
}