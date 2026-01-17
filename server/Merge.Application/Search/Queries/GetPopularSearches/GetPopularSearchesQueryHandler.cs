using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Search.Queries.GetPopularSearches;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetPopularSearchesQueryHandler(IDbContext context, ILogger<GetPopularSearchesQueryHandler> logger, IOptions<SearchSettings> searchSettings) : IRequestHandler<GetPopularSearchesQuery, IReadOnlyList<string>>
{
    private readonly SearchSettings searchConfig = searchSettings.Value;

    public async Task<IReadOnlyList<string>> Handle(GetPopularSearchesQuery request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "Popular searches isteniyor. MaxResults: {MaxResults}",
            request.MaxResults);

        var maxResults = request.MaxResults > searchConfig.MaxAutocompleteResults
            ? searchConfig.MaxAutocompleteResults
            : request.MaxResults;

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !ps.IsDeleted (Global Query Filter)
        var popularSearches = await context.Set<PopularSearch>()
            .AsNoTracking()
            .OrderByDescending(ps => ps.SearchCount)
            .Take(maxResults)
            .Select(ps => ps.SearchTerm)
            .ToListAsync(cancellationToken);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "Popular searches tamamlandı. Count: {Count}",
            popularSearches.Count);

        return popularSearches;
    }
}
