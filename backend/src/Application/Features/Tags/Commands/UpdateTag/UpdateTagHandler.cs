using Application.Extensions;
using Application.Interfaces;
using Domain.Tags;
using ErrorOr;

namespace Application.Features.Tags.Commands.UpdateTag;

public sealed class UpdateTagHandler : ICommandHandler<UpdateTagCommand, UpdateTagResponse>
{
    private readonly IAppDbContext _dbContext;
    private readonly IAuthContext _authContext;
    private readonly TimeProvider _timeProvider;

    public UpdateTagHandler(IAppDbContext dbContext, IAuthContext authContext, TimeProvider timeProvider)
    {
        _dbContext = dbContext;
        _authContext = authContext;
        _timeProvider = timeProvider;
    }

    public async Task<ErrorOr<UpdateTagResponse>> Handle(
        UpdateTagCommand command,
        CancellationToken cancellationToken
    ) =>
        await _dbContext
            .Tags.FirstOrErrorAsync(
                tag => tag.Id == command.Id && tag.UserId == _authContext.CurrentUserId,
                TagErrors.TagNotFound,
                cancellationToken
            )
            .Then(tag => tag.Update(command.Name, command.Color, _timeProvider.GetUtcNow()))
            .ThenDoAsync(_ => _dbContext.SaveChangesAsync(cancellationToken))
            .MatchFirst(
                tag => new UpdateTagResponse(tag.Id, tag.Name, tag.Color, tag.CreatedAt, tag.UpdatedAt).ToErrorOr(),
                error => error
            );
}
