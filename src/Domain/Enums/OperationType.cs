using System.Text.Json.Serialization;

namespace Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum OperationType
{
    Expense,
    Income,
    Fixed,
    Transfer,
    Budget
}