namespace Application;

public static class Utils
{
    public static Guid ToGuid(this string value)
    {
        var isValid = Guid.TryParse(value, out var guid);
        return isValid ? guid : Guid.Empty;
    }
}