using MediatR;

namespace Application.Features.Tags.GetTags;

public sealed record GetTagsRequest : IRequest<IEnumerable<GetTagsResponse>>;