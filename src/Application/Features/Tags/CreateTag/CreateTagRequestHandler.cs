using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Errors;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Tags.CreateTag;

internal sealed class CreateTagRequestHandler : IRequestHandler<CreateTagRequest, Guid>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserContext _userContext;

    public CreateTagRequestHandler(IApplicationDbContext dbContext, IUserContext userContext)
    {
        _dbContext = dbContext;
        _userContext = userContext;
    }

    public async Task<Guid> Handle(CreateTagRequest request, CancellationToken cancellationToken)
    {
        var userId = _userContext.GetAuthenticatedUserId();
        var existingTag = await _dbContext
            .Tags
            .FirstOrDefaultAsync(op => op.Name.ToLower() == request.Name!.ToLower() && op.OwnerId == userId,
                cancellationToken);

        if (existingTag is not null) throw new ConflictException(ErrorsTags.AlreadyExist);

        var tag = new Tag
        {
            Name = request.Name!,
            Color = request.Color!,
            OwnerId = userId
        };
        await _dbContext.Tags.AddAsync(tag, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return tag.Id;
    }
}