using Application.Extensions;
using Application.Interfaces;
using Domain.Tags;
using ErrorOr;

namespace Application.Features.Tags.Commands.DeleteTag;

public sealed class DeleteTagHandler : ICommandHandler<DeleteTagCommand, DeleteTagResponse>
{
    private readonly IAppDbContext _dbContext;
    private readonly IAuthContext _authContext;
    private readonly TimeProvider _timeProvider;

    public DeleteTagHandler(IAppDbContext dbContext, IAuthContext authContext, TimeProvider timeProvider)
    {
        _dbContext = dbContext;
        _authContext = authContext;
        _timeProvider = timeProvider;
    }

    public async Task<ErrorOr<DeleteTagResponse>> Handle(
        DeleteTagCommand command,
        CancellationToken cancellationToken
    ) =>
        await _dbContext
            .Tags.FirstOrErrorAsync(
                tag => tag.Id == command.Id && tag.UserId == _authContext.CurrentUserId,
                TagErrors.TagNotFound,
                cancellationToken
            )
            .Then(tag => tag.Delete(_timeProvider.GetUtcNow()))
            .ThenDoAsync(_ => _dbContext.SaveChangesAsync(cancellationToken))
            .MatchFirst(
                tag =>
                    new DeleteTagResponse(
                        tag.Id,
                        tag.Name,
                        tag.Color,
                        tag.CreatedAt,
                        tag.UpdatedAt,
                        tag.DeletedAt!.Value
                    ).ToErrorOr(),
                error => error
            );
}
