using Merge.Application.DTOs.Search;

// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
namespace Merge.Application.Interfaces.Search;

public interface ISearchSuggestionService
{
    Task<AutocompleteResultDto> GetAutocompleteSuggestionsAsync(string query, int maxResults = 10, CancellationToken cancellationToken = default);
    Task<IEnumerable<string>> GetPopularSearchesAsync(int maxResults = 10, CancellationToken cancellationToken = default);
    Task RecordSearchAsync(string searchTerm, Guid? userId, int resultCount, string? userAgent = null, string? ipAddress = null, CancellationToken cancellationToken = default);
    Task RecordClickAsync(Guid searchHistoryId, Guid productId, CancellationToken cancellationToken = default);
    Task<IEnumerable<SearchSuggestionDto>> GetTrendingSearchesAsync(int days = 7, int maxResults = 10, CancellationToken cancellationToken = default);
}
