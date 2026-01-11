using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;

namespace Merge.Application.Search.Queries.GetPopularSearches;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetPopularSearchesQueryHandler : IRequestHandler<GetPopularSearchesQuery, IReadOnlyList<string>>
{
    private readonly IDbContext _context;
    private readonly ILogger<GetPopularSearchesQueryHandler> _logger;
    private readonly SearchSettings _searchSettings;

    public GetPopularSearchesQueryHandler(
        IDbContext context,
        ILogger<GetPopularSearchesQueryHandler> logger,
        IOptions<SearchSettings> searchSettings)
    {
        _context = context;
        _logger = logger;
        _searchSettings = searchSettings.Value;
    }

    public async Task<IReadOnlyList<string>> Handle(GetPopularSearchesQuery request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Popular searches isteniyor. MaxResults: {MaxResults}",
            request.MaxResults);

        var maxResults = request.MaxResults > _searchSettings.MaxAutocompleteResults
            ? _searchSettings.MaxAutocompleteResults
            : request.MaxResults;

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !ps.IsDeleted (Global Query Filter)
        var popularSearches = await _context.Set<PopularSearch>()
            .AsNoTracking()
            .OrderByDescending(ps => ps.SearchCount)
            .Take(maxResults)
            .Select(ps => ps.SearchTerm)
            .ToListAsync(cancellationToken);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Popular searches tamamlandı. Count: {Count}",
            popularSearches.Count);

        return popularSearches;
    }
}
