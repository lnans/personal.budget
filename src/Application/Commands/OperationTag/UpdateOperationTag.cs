using Domain.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Commands.OperationTag;

public record UpdateOperationTagRequest(string Name, string Color);
public record UpdateOperationTagCommand(string Id, string Name, string Color) : IRequest<UpdateOperationTagResponse>;
public record UpdateOperationTagResponse(Guid Id, string Name, string Color);

public class UpdateOperationTagValidator : AbstractValidator<UpdateOperationTagCommand>
{
    public UpdateOperationTagValidator()
    {
        RuleFor(operationTag => operationTag.Name).NotEmpty().WithMessage("name.required");
        RuleFor(operationTag => operationTag.Color).Matches(@"^#(?:[0-9a-fA-F]{3}){1,2}$").WithMessage("color.invalid");
    }
}

public class UpdateOperationTagCommandHandler : IRequestHandler<UpdateOperationTagCommand, UpdateOperationTagResponse>
{
    private readonly IApplicationDbContext _dbContext;

    public UpdateOperationTagCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UpdateOperationTagResponse> Handle(UpdateOperationTagCommand request, CancellationToken cancellationToken)
    {
        var operationTag = await _dbContext.OperationTags
            .FirstOrDefaultAsync(op => op.Id == request.Id.ToGuid(), cancellationToken);

        if (operationTag is null) throw new NotFoundException("operation_tag.not_found");

        var sameNameOperationTag = await _dbContext
            .OperationTags
            .FirstOrDefaultAsync(op =>
                    op.Name.ToLower() == request.Name.ToLower() &&
                    op.Id != request.Id.ToGuid(),
                cancellationToken);

        if (sameNameOperationTag is not null) throw new AlreadyExistException("operation_tag.already_exist");

        operationTag.Name = request.Name;
        operationTag.Color = request.Color;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new UpdateOperationTagResponse(operationTag.Id, operationTag.Name, operationTag.Color);
    }
}