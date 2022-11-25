using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Errors;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Tags.DeleteTag;

internal sealed class DeleteTagRequestHandler : IRequestHandler<DeleteTagRequest, Unit>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserContext _userContext;

    public DeleteTagRequestHandler(IApplicationDbContext dbContext, IUserContext userContext)
    {
        _dbContext = dbContext;
        _userContext = userContext;
    }

    public async Task<Unit> Handle(DeleteTagRequest request, CancellationToken cancellationToken)
    {
        var userId = _userContext.GetAuthenticatedUserId();
        var tag = await _dbContext
            .Tags
            .FirstOrDefaultAsync(op => op.Id == request.Id && op.OwnerId == userId, cancellationToken);

        if (tag is null) throw new NotFoundException(ErrorsTags.NotFound);

        _dbContext.Tags.Remove(tag);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}