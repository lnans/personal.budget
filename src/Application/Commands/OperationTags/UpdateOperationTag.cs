using Application.Common.Interfaces;
using Domain;
using Domain.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Commands.OperationTags;

public record UpdateOperationTagRequest(string Name, string Color);

public record UpdateOperationTagRequestWithId(string Id, UpdateOperationTagRequest Request) : IRequest<UpdateOperationTagResponse>;

public record UpdateOperationTagResponse(string Id, string Name, string Color);

public class UpdateOperationTagValidator : AbstractValidator<UpdateOperationTagRequestWithId>
{
    public UpdateOperationTagValidator()
    {
        RuleFor(operationTag => operationTag.Request.Name)
            .NotEmpty()
            .WithMessage(Errors.OperationTagNameRequired);

        RuleFor(operationTag => operationTag.Request.Color)
            .Matches(RegexRules.ColorHex)
            .WithMessage(Errors.OperationTagColorInvalid);
    }
}

public class UpdateOperationTag : IRequestHandler<UpdateOperationTagRequestWithId, UpdateOperationTagResponse>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserContext _userContext;

    public UpdateOperationTag(IApplicationDbContext dbContext, IUserContext userContext)
    {
        _dbContext = dbContext;
        _userContext = userContext;
    }

    public async Task<UpdateOperationTagResponse> Handle(UpdateOperationTagRequestWithId request, CancellationToken cancellationToken)
    {
        var userId = _userContext.GetUserId();
        var operationTag = await _dbContext
            .OperationTags
            .FirstOrDefaultAsync(op => op.Id == request.Id && op.OwnerId == userId, cancellationToken);

        if (operationTag is null) throw new NotFoundException(Errors.OperationTagNotFound);

        operationTag.Name = request.Request.Name;
        operationTag.Color = request.Request.Color;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new UpdateOperationTagResponse(operationTag.Id, operationTag.Name, operationTag.Color);
    }
}