using Application.Common.Interfaces;
using Domain;
using Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.OperationTags.Commands.PatchOperationTag;

public record PatchOperationTagRequest : IRequest<Unit>
{
    public string Id { get; set; }
    public string Name { get; init; }
    public string Color { get; init; }
}

public class PatchOperationTagCommandHandler : IRequestHandler<PatchOperationTagRequest, Unit>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IHttpUserContext _httpUserContext;

    public PatchOperationTagCommandHandler(IApplicationDbContext dbContext, IHttpUserContext httpUserContext)
    {
        _dbContext = dbContext;
        _httpUserContext = httpUserContext;
    }

    public async Task<Unit> Handle(PatchOperationTagRequest request, CancellationToken cancellationToken)
    {
        var userId = _httpUserContext.GetUserId();
        var operationTag = await _dbContext
            .OperationTags
            .FirstOrDefaultAsync(op => op.Id == request.Id && op.OwnerId == userId, cancellationToken);

        if (operationTag is null) throw new NotFoundException(Errors.OperationTagNotFound);

        operationTag.Name = request.Name;
        operationTag.Color = request.Color;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}