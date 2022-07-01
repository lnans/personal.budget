namespace Application.Features.Authentication.Queries.GetAuthInfo;

public record AuthenticationInfoDto
{
    public string Id { get; init; }
    public string Username { get; init; }
}