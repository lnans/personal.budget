using Application.Common.Interfaces;
using Domain;
using Domain.Entities;
using Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.OperationTags.Commands.CreateOperationTag;

public record CreateOperationTagRequest : IRequest<Guid>
{
    public string Name { get; init; }
    public string Color { get; init; }
}

public class CreateOperationTagCommandHandler : IRequestHandler<CreateOperationTagRequest, Guid>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IHttpUserContext _httpUserContext;

    public CreateOperationTagCommandHandler(IApplicationDbContext dbContext, IHttpUserContext httpUserContext)
    {
        _dbContext = dbContext;
        _httpUserContext = httpUserContext;
    }

    public async Task<Guid> Handle(CreateOperationTagRequest request, CancellationToken cancellationToken)
    {
        var userId = _httpUserContext.GetUserId();
        var existingOperationTag = await _dbContext
            .OperationTags
            .FirstOrDefaultAsync(op => op.Name.ToLower() == request.Name.ToLower() && op.OwnerId == userId,
                cancellationToken);

        if (existingOperationTag is not null) throw new AlreadyExistException(Errors.OperationTagAlreadyExist);

        var guid = Guid.NewGuid();
        var operationTag = new OperationTag
        {
            Id = guid.ToString(),
            Name = request.Name,
            Color = request.Color,
            OwnerId = userId
        };
        await _dbContext.OperationTags.AddAsync(operationTag, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return guid;
    }
}