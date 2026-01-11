using MediatR;

namespace Merge.Application.Search.Queries.GetPopularSearches;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetPopularSearchesQuery(
    int MaxResults = 10
) : IRequest<IReadOnlyList<string>>;
