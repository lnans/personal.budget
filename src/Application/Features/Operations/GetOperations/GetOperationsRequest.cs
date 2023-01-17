using Application.Common.Models;
using Domain.Enums;
using MediatR;

namespace Application.Features.Operations.GetOperations;

public sealed record GetOperationsRequest : IRequest<PaginatedList<GetOperationsResponse>>
{
    public Guid? AccountId { get; init; }
    public string? Description { get; init; }
    public Guid[]? TagIds { get; init; }
    public OperationType? Type { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
}