namespace Application.Features.Authentication.Commands.SignIn;

public record SignInResponse(string Bearer, string RefreshToken) { }
