using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AutoMapper;
using Merge.Application.DTOs.Product;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Product.Queries.GetAllProductBundles;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetAllProductBundlesQueryHandler : IRequestHandler<GetAllProductBundlesQuery, IEnumerable<ProductBundleDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAllProductBundlesQueryHandler> _logger;
    private readonly ICacheService _cache;
    private readonly CacheSettings _cacheSettings;
    private const string CACHE_KEY_ALL_BUNDLES = "bundles_all";
    private const string CACHE_KEY_ACTIVE_BUNDLES = "bundles_active";

    public GetAllProductBundlesQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetAllProductBundlesQueryHandler> logger,
        ICacheService cache,
        IOptions<CacheSettings> cacheSettings)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _cache = cache;
        _cacheSettings = cacheSettings.Value;
    }

    public async Task<IEnumerable<ProductBundleDto>> Handle(GetAllProductBundlesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching all product bundles. ActiveOnly: {ActiveOnly}", request.ActiveOnly);

        // ✅ BOLUM 10.1: Cache-Aside Pattern
        var cacheKey = request.ActiveOnly ? CACHE_KEY_ACTIVE_BUNDLES : CACHE_KEY_ALL_BUNDLES;
        var cachedBundles = await _cache.GetAsync<IEnumerable<ProductBundleDto>>(cacheKey, cancellationToken);
        if (cachedBundles != null)
        {
            _logger.LogInformation("Product bundles retrieved from cache. ActiveOnly: {ActiveOnly}", request.ActiveOnly);
            return cachedBundles;
        }

        // ✅ PERFORMANCE: AsNoTracking for read-only queries, removed !b.IsDeleted (Global Query Filter)
        // ✅ PERFORMANCE: AsSplitQuery to prevent Cartesian Explosion (ThenInclude)
        IQueryable<ProductBundle> query = _context.Set<ProductBundle>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(b => b.BundleItems)
                .ThenInclude(bi => bi.Product);

        if (request.ActiveOnly)
        {
            var now = DateTime.UtcNow;
            query = query.Where(b => b.IsActive &&
                  (!b.StartDate.HasValue || b.StartDate.Value <= now) &&
                  (!b.EndDate.HasValue || b.EndDate.Value >= now));
        }

        var bundles = await query
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync(cancellationToken);

        var bundleDtos = _mapper.Map<IEnumerable<ProductBundleDto>>(bundles).ToList();

        // ✅ BOLUM 10.1: Cache-Aside Pattern - Cache'e yaz
        // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma (Clean Architecture)
        await _cache.SetAsync(cacheKey, bundleDtos, TimeSpan.FromMinutes(_cacheSettings.ProductBundleCacheExpirationMinutes), cancellationToken);

        _logger.LogInformation("Retrieved all product bundles. Count: {Count}, ActiveOnly: {ActiveOnly}",
            bundles.Count, request.ActiveOnly);

        return bundleDtos;
    }
}
