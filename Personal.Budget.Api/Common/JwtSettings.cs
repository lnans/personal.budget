namespace Personal.Budget.Api.Common;

public class JwtSettings
{
    public string Issuer { get; init; } = default!;
    public string Audience { get; init; } = default!;
    public string Key { get; init; } = default!;
}