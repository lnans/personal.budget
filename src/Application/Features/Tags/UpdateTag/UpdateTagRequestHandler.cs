using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Errors;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Tags.UpdateTag;

internal sealed class UpdateTagRequestHandler : IRequestHandler<UpdateTagRequest, Unit>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IAuthContext _authContext;

    public UpdateTagRequestHandler(IApplicationDbContext dbContext, IAuthContext authContext)
    {
        _dbContext = dbContext;
        _authContext = authContext;
    }

    public async Task<Unit> Handle(UpdateTagRequest request, CancellationToken cancellationToken)
    {
        var userId = _authContext.GetAuthenticatedUserId();
        var existingTag = await _dbContext
            .Tags
            .FirstOrDefaultAsync(tag => tag.Name.ToLower() == request.Name!.ToLower() && tag.OwnerId == userId,
                cancellationToken);

        if (existingTag is not null) throw new ConflictException(ErrorsTags.AlreadyExist);

        var updatedTag = await _dbContext
            .Tags
            .FirstOrDefaultAsync(op => op.Id == request.Id && op.OwnerId == userId, cancellationToken);

        if (updatedTag is null) throw new NotFoundException(ErrorsTags.NotFound);

        updatedTag.Name = request.Name!;
        updatedTag.Color = request.Color!;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}