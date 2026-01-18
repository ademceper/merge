using MediatR;

namespace Merge.Application.Search.Queries.GetPopularSearches;

public record GetPopularSearchesQuery(
    int MaxResults = 10
) : IRequest<IReadOnlyList<string>>;
