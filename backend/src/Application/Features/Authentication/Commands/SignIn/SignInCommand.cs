using Application.Interfaces;

namespace Application.Features.Authentication.Commands.SignIn;

public sealed record SignInCommand(string Login, string Password) : ICommand<SignInResponse>;
