using MediatR;
using Merge.Application.DTOs.Search;

namespace Merge.Application.Search.Queries.GetTrendingSearches;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetTrendingSearchesQuery(
    int Days = 7,
    int MaxResults = 10
) : IRequest<IReadOnlyList<SearchSuggestionDto>>;
