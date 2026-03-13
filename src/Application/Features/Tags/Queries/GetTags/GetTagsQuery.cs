using Application.Interfaces;

namespace Application.Features.Tags.Queries.GetTags;

public sealed record GetTagsQuery : IQuery<List<GetTagsResponse>>;
