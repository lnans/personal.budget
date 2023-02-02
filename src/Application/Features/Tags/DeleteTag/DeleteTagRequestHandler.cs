using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Errors;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Tags.DeleteTag;

internal sealed class DeleteTagRequestHandler : IRequestHandler<DeleteTagRequest, Unit>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IAuthContext _authContext;

    public DeleteTagRequestHandler(IApplicationDbContext dbContext, IAuthContext authContext)
    {
        _dbContext = dbContext;
        _authContext = authContext;
    }

    public async Task<Unit> Handle(DeleteTagRequest request, CancellationToken cancellationToken)
    {
        var userId = _authContext.GetAuthenticatedUserId();
        var tag = await _dbContext
            .Tags
            .Include(t => t.Operations)
            .FirstOrDefaultAsync(op => op.Id == request.Id && op.OwnerId == userId, cancellationToken);

        if (tag is null) throw new NotFoundException(ErrorsTags.NotFound);

        if (tag.Operations is not null && tag.Operations.Any()) throw new BadRequestException(ErrorsTags.IsInUse);

        _dbContext.Tags.Remove(tag);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}