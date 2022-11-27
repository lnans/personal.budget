using Application.Common.Models;
using Domain.Enums;
using MediatR;

namespace Application.Features.Operations.GetOperations;

public sealed record GetOperationsRequest : IRequest<InfiniteDataList<GetOperationsResponse>>
{
    public Guid? AccountId { get; init; }
    public string? Description { get; init; }
    public Guid[]? TagIds { get; init; }
    public OperationType? Type { get; init; }
    public int Cursor { get; init; }
    public int PageSize { get; init; } = 25;
}