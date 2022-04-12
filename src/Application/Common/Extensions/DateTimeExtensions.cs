namespace Application.Common.Extensions;

public static class DateTimeExtensions
{
    public static string ToMidnightIsoString(this DateTime date)
    {
        var midnight = new DateTime(date.Year, date.Month, date.Day);
        return midnight.ToString("o");
    }
}