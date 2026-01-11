using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Search;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;

namespace Merge.Application.Search.Queries.GetTrendingSearches;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetTrendingSearchesQueryHandler : IRequestHandler<GetTrendingSearchesQuery, IReadOnlyList<SearchSuggestionDto>>
{
    private readonly IDbContext _context;
    private readonly ILogger<GetTrendingSearchesQueryHandler> _logger;
    private readonly SearchSettings _searchSettings;

    public GetTrendingSearchesQueryHandler(
        IDbContext context,
        ILogger<GetTrendingSearchesQueryHandler> logger,
        IOptions<SearchSettings> searchSettings)
    {
        _context = context;
        _logger = logger;
        _searchSettings = searchSettings.Value;
    }

    public async Task<IReadOnlyList<SearchSuggestionDto>> Handle(GetTrendingSearchesQuery request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Trending searches isteniyor. Days: {Days}, MaxResults: {MaxResults}",
            request.Days, request.MaxResults);

        var days = request.Days < 1 ? _searchSettings.DefaultTrendingDays : request.Days;
        if (days > _searchSettings.MaxTrendingDays) days = _searchSettings.MaxTrendingDays;

        var maxResults = request.MaxResults > _searchSettings.MaxAutocompleteResults
            ? _searchSettings.MaxAutocompleteResults
            : request.MaxResults;

        var startDate = DateTime.UtcNow.AddDays(-days);

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !sh.IsDeleted (Global Query Filter)
        // ✅ PERFORMANCE: Database'de grouping yap (memory'de işlem YASAK)
        var trendingSearches = await _context.Set<SearchHistory>()
            .AsNoTracking()
            .Where(sh => sh.CreatedAt >= startDate)
            .GroupBy(sh => sh.SearchTerm.ToLower())
            .Select(g => new SearchSuggestionDto(
                g.First().SearchTerm,
                "Trending",
                g.Count(),
                (Guid?)null
            ))
            .OrderByDescending(s => s.Frequency)
            .Take(maxResults)
            .ToListAsync(cancellationToken);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Trending searches tamamlandı. Days: {Days}, Count: {Count}",
            days, trendingSearches.Count);

        return trendingSearches;
    }
}
