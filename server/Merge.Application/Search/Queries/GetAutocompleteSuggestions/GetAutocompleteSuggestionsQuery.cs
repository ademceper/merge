using MediatR;
using Merge.Application.DTOs.Search;

namespace Merge.Application.Search.Queries.GetAutocompleteSuggestions;

public record GetAutocompleteSuggestionsQuery(
    string Query,
    int MaxResults = 10
) : IRequest<AutocompleteResultDto>;
