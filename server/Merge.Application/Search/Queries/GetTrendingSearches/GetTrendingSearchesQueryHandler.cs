using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Search;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Search.Queries.GetTrendingSearches;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetTrendingSearchesQueryHandler(IDbContext context, ILogger<GetTrendingSearchesQueryHandler> logger, IOptions<SearchSettings> searchSettings) : IRequestHandler<GetTrendingSearchesQuery, IReadOnlyList<SearchSuggestionDto>>
{
    private readonly SearchSettings searchConfig = searchSettings.Value;

    public async Task<IReadOnlyList<SearchSuggestionDto>> Handle(GetTrendingSearchesQuery request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "Trending searches isteniyor. Days: {Days}, MaxResults: {MaxResults}",
            request.Days, request.MaxResults);

        var days = request.Days < 1 ? searchConfig.DefaultTrendingDays : request.Days;
        if (days > searchConfig.MaxTrendingDays) days = searchConfig.MaxTrendingDays;

        var maxResults = request.MaxResults > searchConfig.MaxAutocompleteResults
            ? searchConfig.MaxAutocompleteResults
            : request.MaxResults;

        var startDate = DateTime.UtcNow.AddDays(-days);

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !sh.IsDeleted (Global Query Filter)
        // ✅ PERFORMANCE: Database'de grouping yap (memory'de işlem YASAK)
        var trendingSearches = await context.Set<SearchHistory>()
            .AsNoTracking()
            .Where(sh => sh.CreatedAt >= startDate)
            .GroupBy(sh => sh.SearchTerm.ToLower())
            .Where(g => g.Any()) // ✅ ERROR HANDLING FIX: Ensure group has elements before First()
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
        logger.LogInformation(
            "Trending searches tamamlandı. Days: {Days}, Count: {Count}",
            days, trendingSearches.Count);

        return trendingSearches;
    }
}
