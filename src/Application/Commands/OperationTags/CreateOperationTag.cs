using Application.Common.Interfaces;
using Domain;
using Domain.Entities;
using Domain.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Commands.OperationTags;

public record CreateOperationTagRequest(string Name, string Color) : IRequest<CreateOperationTagResponse>;
public record CreateOperationTagResponse(string Id, string Name, string Color);

public class CreateOperationTagValidator : AbstractValidator<CreateOperationTagRequest>
{
    public CreateOperationTagValidator()
    {
        RuleFor(operationTag => operationTag.Name)
            .NotEmpty()
            .WithMessage(Errors.OperationTagNameRequired);

        RuleFor(operationTag => operationTag.Color)
            .Matches(RegexRules.ColorHex)
            .WithMessage(Errors.OperationTagColorInvalid);
    }
}

public class CreateOperationTag : IRequestHandler<CreateOperationTagRequest, CreateOperationTagResponse>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserContext _userContext;

    public CreateOperationTag(IApplicationDbContext dbContext, IUserContext userContext)
    {
        _dbContext = dbContext;
        _userContext = userContext;
    }

    public async Task<CreateOperationTagResponse> Handle(CreateOperationTagRequest request, CancellationToken cancellationToken)
    {
        var userId = _userContext.GetUserId();
        var existingOperationTag = await _dbContext
            .OperationTags
            .FirstOrDefaultAsync(op => op.Name.ToLower() == request.Name.ToLower() && op.OwnerId == userId,
                cancellationToken);

        if (existingOperationTag is not null) throw new AlreadyExistException(Errors.OperationTagAlreadyExist);

        var operationTag = new OperationTag
        {
            Id = Guid.NewGuid().ToString(),
            Name = request.Name,
            Color = request.Color,
            OwnerId = userId
        };
        await _dbContext.OperationTags.AddAsync(operationTag, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CreateOperationTagResponse(operationTag.Id, operationTag.Name, operationTag.Color);
    }
}