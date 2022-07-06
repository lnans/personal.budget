using Application.Common.Interfaces;
using Domain;
using Domain.Entities;
using Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Tags.Commands.CreateTag;

public record CreateTagRequest : IRequest<Guid>
{
    public string Name { get; init; }
    public string Color { get; init; }
}

public class CreateTagCommandHandler : IRequestHandler<CreateTagRequest, Guid>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IHttpUserContext _httpUserContext;

    public CreateTagCommandHandler(IApplicationDbContext dbContext, IHttpUserContext httpUserContext)
    {
        _dbContext = dbContext;
        _httpUserContext = httpUserContext;
    }

    public async Task<Guid> Handle(CreateTagRequest request, CancellationToken cancellationToken)
    {
        var userId = _httpUserContext.GetUserId();
        var existingTag = await _dbContext
            .Tags
            .FirstOrDefaultAsync(op => op.Name.ToLower() == request.Name.ToLower() && op.OwnerId == userId,
                cancellationToken);

        if (existingTag is not null) throw new AlreadyExistException(Errors.TagAlreadyExist);

        var guid = Guid.NewGuid();
        var tag = new Tag
        {
            Id = guid.ToString(),
            Name = request.Name,
            Color = request.Color,
            OwnerId = userId
        };
        await _dbContext.Tags.AddAsync(tag, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return guid;
    }
}