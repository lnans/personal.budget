using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Errors;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Tags.UpdateTag;

internal sealed class UpdateTagRequestHandler : IRequestHandler<UpdateTagRequest, Unit>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserContext _userContext;

    public UpdateTagRequestHandler(IApplicationDbContext dbContext, IUserContext userContext)
    {
        _dbContext = dbContext;
        _userContext = userContext;
    }

    public async Task<Unit> Handle(UpdateTagRequest request, CancellationToken cancellationToken)
    {
        var userId = _userContext.GetAuthenticatedUserId();
        var tag = await _dbContext
            .Tags
            .FirstOrDefaultAsync(op => op.Id == request.Id && op.OwnerId == userId, cancellationToken);

        if (tag is null) throw new NotFoundException(ErrorsTags.NotFound);

        tag.Name = request.Name!;
        tag.Color = request.Color!;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}