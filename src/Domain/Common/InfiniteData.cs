namespace Domain.Common;

public record InfiniteData<TData>(IEnumerable<TData> Data, int? NextCursor);