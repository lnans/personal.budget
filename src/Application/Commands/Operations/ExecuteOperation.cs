using Application.Common.Interfaces;
using Domain;
using Domain.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Commands.Operations;

public record ExecuteOperationRequest(DateTime ExecutionDate);

public record ExecuteOperationRequestWithId(string Id, ExecuteOperationRequest Request) : IRequest<Unit>;

public class ExecuteOperationValidator : AbstractValidator<ExecuteOperationRequestWithId>
{
    public ExecuteOperationValidator()
    {
        RuleFor(p => p.Request.ExecutionDate)
            .NotEqual(default(DateTime))
            .WithMessage(Errors.OperationExecutionDateRequired);
    }
}

public class ExecuteOperation : IRequestHandler<ExecuteOperationRequestWithId, Unit>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserContext _userContext;

    public ExecuteOperation(IApplicationDbContext dbContext, IUserContext userContext)
    {
        _dbContext = dbContext;
        _userContext = userContext;
    }

    public async Task<Unit> Handle(ExecuteOperationRequestWithId request, CancellationToken cancellationToken)
    {
        var userId = _userContext.GetUserId();
        var operation = await _dbContext
            .Operations
            .FirstOrDefaultAsync(o => o.Id == request.Id && o.Account.OwnerId == userId, cancellationToken);

        if (operation is null) throw new NotFoundException(Errors.OperationNotFound);

        operation.ExecutionDate = request.Request.ExecutionDate;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}