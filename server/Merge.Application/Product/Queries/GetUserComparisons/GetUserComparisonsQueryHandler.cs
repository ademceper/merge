using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Product;
using Merge.Application.Common;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using ReviewEntity = Merge.Domain.Modules.Catalog.Review;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Product.Queries.GetUserComparisons;

public class GetUserComparisonsQueryHandler : IRequestHandler<GetUserComparisonsQuery, PagedResult<ProductComparisonDto>>
{
    private readonly IDbContext _context;
    private readonly AutoMapper.IMapper _mapper;
    private readonly ILogger<GetUserComparisonsQueryHandler> _logger;
    private readonly ICacheService _cache;
    private readonly PaginationSettings _paginationSettings;
    private readonly CacheSettings _cacheSettings;
    private const string CACHE_KEY_USER_COMPARISONS = "user_comparisons_";

    public GetUserComparisonsQueryHandler(
        IDbContext context,
        AutoMapper.IMapper mapper,
        ILogger<GetUserComparisonsQueryHandler> logger,
        ICacheService cache,
        IOptions<PaginationSettings> paginationSettings,
        IOptions<CacheSettings> cacheSettings)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _cache = cache;
        _paginationSettings = paginationSettings.Value;
        _cacheSettings = cacheSettings.Value;
    }

    public async Task<PagedResult<ProductComparisonDto>> Handle(GetUserComparisonsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching user comparisons. UserId: {UserId}, Page: {Page}, PageSize: {PageSize}, SavedOnly: {SavedOnly}",
            request.UserId, request.Page, request.PageSize, request.SavedOnly);

        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        // ✅ BOLUM 12.0: Magic number YASAK - Config kullan (ZORUNLU)
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize > _paginationSettings.MaxPageSize
            ? _paginationSettings.MaxPageSize
            : request.PageSize;

        var cacheKey = $"{CACHE_KEY_USER_COMPARISONS}{request.UserId}_{request.SavedOnly}_{page}_{pageSize}";

        // ✅ BOLUM 10.2: Redis distributed cache
        var cachedResult = await _cache.GetOrCreateAsync(
            cacheKey,
            async () =>
            {
                _logger.LogInformation("Cache miss for user comparisons. Fetching from database.");

                // ✅ PERFORMANCE: AsSplitQuery to prevent Cartesian Explosion (nested ThenInclude)
                var query = _context.Set<ProductComparison>()
                    .AsNoTracking()
                    .AsSplitQuery()
                    .Include(c => c.Items)
                        .ThenInclude(i => i.Product)
                            .ThenInclude(p => p.Category)
                    .Where(c => c.UserId == request.UserId);

                if (request.SavedOnly)
                {
                    query = query.Where(c => c.IsSaved);
                }

                var totalCount = await query.CountAsync(cancellationToken);

                var comparisons = await query
                    .OrderByDescending(c => c.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(cancellationToken);

                var dtos = new List<ProductComparisonDto>(comparisons.Count);
                foreach (var comparison in comparisons)
                {
                    dtos.Add(await MapToDto(comparison, cancellationToken));
                }

                return new PagedResult<ProductComparisonDto>
                {
                    Items = dtos,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize
                };
            },
            TimeSpan.FromMinutes(_cacheSettings.ProductComparisonCacheExpirationMinutes),
            cancellationToken);

        return cachedResult!;
    }

    private async Task<ProductComparisonDto> MapToDto(ProductComparison comparison, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: Subquery yaklaşımı - memory'de hiçbir şey tutma (ISSUE #3.1 fix)
        var itemsQuery = _context.Set<ProductComparisonItem>()
            .AsNoTracking()
            .Where(i => i.ComparisonId == comparison.Id)
            .OrderBy(i => i.Position);

        var items = await itemsQuery
            .AsSplitQuery()
            .Include(i => i.Product)
                .ThenInclude(p => p.Category)
            .ToListAsync(cancellationToken);

        // ✅ PERFORMANCE: Subquery yaklaşımı - memory'de hiçbir şey tutma (ISSUE #3.1 fix)
        var productIdsSubquery = from i in itemsQuery select i.ProductId;
        Dictionary<Guid, (decimal Rating, int Count)> reviewsDict;
        var reviews = await _context.Set<ReviewEntity>()
            .AsNoTracking()
            .Where(r => productIdsSubquery.Contains(r.ProductId))
            .GroupBy(r => r.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                Rating = (decimal)g.Average(r => r.Rating),
                Count = g.Count()
            })
            .ToListAsync(cancellationToken);
        reviewsDict = reviews.ToDictionary(x => x.ProductId, x => (x.Rating, x.Count));

        // ✅ BOLUM 7.1.5: Records - with expression kullanımı (immutable record'lar için)
        var products = new List<ComparisonProductDto>();
        foreach (var item in items)
        {
            var hasReviewStats = reviewsDict.TryGetValue(item.ProductId, out var stats);
            var compProduct = _mapper.Map<ComparisonProductDto>(item.Product);
            compProduct = compProduct with
            {
                Position = item.Position,
                Rating = hasReviewStats ? (decimal?)stats.Rating : null,
                ReviewCount = hasReviewStats ? stats.Count : 0,
                Specifications = new Dictionary<string, string>().AsReadOnly(),
                Features = new List<string>().AsReadOnly()
            };
            products.Add(compProduct);
        }

        var comparisonDto = _mapper.Map<ProductComparisonDto>(comparison);
        comparisonDto = comparisonDto with { Products = products.AsReadOnly() };
        return comparisonDto;
    }
}
