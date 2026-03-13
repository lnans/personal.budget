using Application.Interfaces;
using Domain.Tags;
using ErrorOr;

namespace Application.Features.Tags.Commands.CreateTag;

public sealed class CreateTagHandler : ICommandHandler<CreateTagCommand, CreateTagResponse>
{
    private readonly IAppDbContext _dbContext;
    private readonly IAuthContext _authContext;
    private readonly TimeProvider _timeProvider;

    public CreateTagHandler(IAppDbContext dbContext, IAuthContext authContext, TimeProvider timeProvider)
    {
        _dbContext = dbContext;
        _authContext = authContext;
        _timeProvider = timeProvider;
    }

    public async Task<ErrorOr<CreateTagResponse>> Handle(
        CreateTagCommand command,
        CancellationToken cancellationToken
    ) =>
        await Tag.Create(_authContext.CurrentUserId, command.Name, command.Color, _timeProvider.GetUtcNow())
            .ThenDo(tag => _dbContext.Tags.Add(tag))
            .ThenDoAsync(_ => _dbContext.SaveChangesAsync(cancellationToken))
            .MatchFirst(
                tag => new CreateTagResponse(tag.Id, tag.Name, tag.Color, tag.CreatedAt, tag.UpdatedAt).ToErrorOr(),
                error => error
            );
}
