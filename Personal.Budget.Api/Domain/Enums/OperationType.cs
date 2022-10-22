using System.Text.Json.Serialization;

namespace Personal.Budget.Api.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum OperationType
{
    Expense,
    Income,
    Fixed,
    Transfer,
    Budget
}