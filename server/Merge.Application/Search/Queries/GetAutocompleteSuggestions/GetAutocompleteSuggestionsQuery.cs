using MediatR;
using Merge.Application.DTOs.Search;

namespace Merge.Application.Search.Queries.GetAutocompleteSuggestions;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetAutocompleteSuggestionsQuery(
    string Query,
    int MaxResults = 10
) : IRequest<AutocompleteResultDto>;
