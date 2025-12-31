using Merge.Application.DTOs.Search;

namespace Merge.Application.Interfaces.Search;

public interface ISearchSuggestionService
{
    Task<AutocompleteResultDto> GetAutocompleteSuggestionsAsync(string query, int maxResults = 10);
    Task<IEnumerable<string>> GetPopularSearchesAsync(int maxResults = 10);
    Task RecordSearchAsync(string searchTerm, Guid? userId, int resultCount, string? userAgent = null, string? ipAddress = null);
    Task RecordClickAsync(Guid searchHistoryId, Guid productId);
    Task<IEnumerable<SearchSuggestionDto>> GetTrendingSearchesAsync(int days = 7, int maxResults = 10);
}
