using Application.Common.Interfaces;
using Domain;
using Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.OperationTags.Commands.DeleteOperationTag;

public record DeleteOperationTagRequest : IRequest<Unit>
{
    public string Id { get; init; }
}

public class DeleteOperationTagCommandHandler : IRequestHandler<DeleteOperationTagRequest, Unit>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IHttpUserContext _httpUserContext;

    public DeleteOperationTagCommandHandler(IApplicationDbContext dbContext, IHttpUserContext httpUserContext)
    {
        _dbContext = dbContext;
        _httpUserContext = httpUserContext;
    }

    public async Task<Unit> Handle(DeleteOperationTagRequest request, CancellationToken cancellationToken)
    {
        var userId = _httpUserContext.GetUserId();
        var operationTag = await _dbContext
            .OperationTags
            .FirstOrDefaultAsync(op => op.Id == request.Id && op.OwnerId == userId, cancellationToken);

        if (operationTag is null) throw new NotFoundException(Errors.OperationTagNotFound);

        _dbContext.OperationTags.Remove(operationTag);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}