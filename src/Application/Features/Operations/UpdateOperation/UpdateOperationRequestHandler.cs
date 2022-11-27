using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Errors;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Operations.UpdateOperation;

internal sealed class UpdateOperationRequestHandler : IRequestHandler<UpdateOperationRequest, Unit>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserContext _userContext;

    public UpdateOperationRequestHandler(IApplicationDbContext dbContext, IUserContext userContext)
    {
        _dbContext = dbContext;
        _userContext = userContext;
    }

    public async Task<Unit> Handle(UpdateOperationRequest request, CancellationToken cancellationToken)
    {
        var userId = _userContext.GetAuthenticatedUserId();
        var operation = await _dbContext
            .Operations
            .Include(o => o.Account)
            .Include(o => o.Tags)
            .FirstOrDefaultAsync(o => o.Id == request.Id && o.Account.OwnerId == userId, cancellationToken);

        if (operation is null) throw new NotFoundException(ErrorsOperations.NotFound);

        if (request.TagIds != null)
        {
            var tags = new List<Tag>();
            foreach (var tagId in request.TagIds)
            {
                var tag = await _dbContext
                    .Tags
                    .FirstOrDefaultAsync(o => o.Id == tagId && o.OwnerId == userId, cancellationToken);

                if (tag is null) throw new NotFoundException(ErrorsTags.NotFound);
            }

            operation.Tags = tags;
        }
        else if (operation.Tags is not null)
        {
            operation.Tags = null;
        }

        // Update transaction Amount
        operation.Account.Balance += request.Amount - operation.Amount;

        operation.Description = request.Description!;
        operation.Amount = request.Amount;
        operation.CreationDate = request.CreationDate;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}