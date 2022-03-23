using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Domain.Exceptions;

namespace Application.Commands.OperationTag;

public record CreateOperationTagCommand(string Name, string Color) : IRequest<CreateOperationTagResponse>;
public record CreateOperationTagResponse(string Id, string Name, string Color);

public class CreateOperationTagValidator : AbstractValidator<CreateOperationTagCommand>
{
    public CreateOperationTagValidator()
    {
        RuleFor(operationTag => operationTag.Name).NotEmpty().WithMessage("name.required");
        RuleFor(operationTag => operationTag.Color).Matches(@"^#(?:[0-9a-fA-F]{3}){1,2}$").WithMessage("color.invalid");
    }
}

public class CreateOperationTagHandler : IRequestHandler<CreateOperationTagCommand, CreateOperationTagResponse>
{
    private readonly IApplicationDbContext _dbContext;

    public CreateOperationTagHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CreateOperationTagResponse> Handle(CreateOperationTagCommand command, CancellationToken cancellationToken)
    {
        var (name, color) = command;
        var operationTag = await _dbContext
            .OperationTags
            .FirstOrDefaultAsync(op => op.Name.ToLower() == name.ToLower(), cancellationToken);

        if (operationTag is not null) throw new AlreadyExistException("operation_tag.already_exist");
        operationTag = new Domain.Entities.OperationTag
        {
            Name = name,
            Color = color
        };
        await _dbContext.OperationTags.AddAsync(operationTag, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CreateOperationTagResponse(operationTag.Id, operationTag.Name, operationTag.Color);
    }
}