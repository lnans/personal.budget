using Application.Common.Interfaces;
using Domain;
using Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Tags.Commands.PutTag;

public record PutTagRequest : IRequest<Unit>
{
    public string Id { get; set; }
    public string Name { get; init; }
    public string Color { get; init; }
}

public class PutTagCommandHandler : IRequestHandler<PutTagRequest, Unit>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IHttpUserContext _httpUserContext;

    public PutTagCommandHandler(IApplicationDbContext dbContext, IHttpUserContext httpUserContext)
    {
        _dbContext = dbContext;
        _httpUserContext = httpUserContext;
    }

    public async Task<Unit> Handle(PutTagRequest request, CancellationToken cancellationToken)
    {
        var userId = _httpUserContext.GetUserId();
        var tag = await _dbContext
            .Tags
            .FirstOrDefaultAsync(op => op.Id == request.Id && op.OwnerId == userId, cancellationToken);

        if (tag is null) throw new NotFoundException(Errors.TagNotFound);

        tag.Name = request.Name;
        tag.Color = request.Color;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}