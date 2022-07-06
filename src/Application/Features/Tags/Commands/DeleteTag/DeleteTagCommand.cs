using Application.Common.Interfaces;
using Domain;
using Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Tags.Commands.DeleteTag;

public record DeleteTagRequest : IRequest<Unit>
{
    public string Id { get; init; }
}

public class DeleteTagCommandHandler : IRequestHandler<DeleteTagRequest, Unit>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IHttpUserContext _httpUserContext;

    public DeleteTagCommandHandler(IApplicationDbContext dbContext, IHttpUserContext httpUserContext)
    {
        _dbContext = dbContext;
        _httpUserContext = httpUserContext;
    }

    public async Task<Unit> Handle(DeleteTagRequest request, CancellationToken cancellationToken)
    {
        var userId = _httpUserContext.GetUserId();
        var tag = await _dbContext
            .Tags
            .FirstOrDefaultAsync(op => op.Id == request.Id && op.OwnerId == userId, cancellationToken);

        if (tag is null) throw new NotFoundException(Errors.TagNotFound);

        _dbContext.Tags.Remove(tag);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}