using ErrorOr;
using MediatR;

namespace Application.Features.Authentication.Queries.GetCurrentUser;

public sealed class GetCurrentUserQuery : IRequest<ErrorOr<GetCurrentUserResponse>> { }
