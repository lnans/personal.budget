using Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Commands.OperationTag;

public record DeleteOperationTagCommand(string Id) : IRequest<Unit>;

public class DeleteOperationTagCommandHandler : IRequestHandler<DeleteOperationTagCommand, Unit>
{
    private readonly IApplicationDbContext _dbContext;

    public DeleteOperationTagCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Unit> Handle(DeleteOperationTagCommand request, CancellationToken cancellationToken)
    {
        var operationTag = await _dbContext.OperationTags
            .FirstOrDefaultAsync(op => op.Id == request.Id, cancellationToken);

        if (operationTag is null) throw new NotFoundException("operation_tag.not_found");

        _dbContext.OperationTags.Remove(operationTag);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}