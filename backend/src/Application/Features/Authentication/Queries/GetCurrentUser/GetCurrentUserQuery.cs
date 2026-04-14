using Application.Interfaces;

namespace Application.Features.Authentication.Queries.GetCurrentUser;

public sealed record GetCurrentUserQuery : IQuery<GetCurrentUserResponse>;
