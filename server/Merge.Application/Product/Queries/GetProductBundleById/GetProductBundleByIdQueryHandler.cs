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

namespace Merge.Application.Product.Queries.GetProductBundleById;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetProductBundleByIdQueryHandler : IRequestHandler<GetProductBundleByIdQuery, ProductBundleDto?>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetProductBundleByIdQueryHandler> _logger;
    private readonly ICacheService _cache;
    private readonly CacheSettings _cacheSettings;
    private const string CACHE_KEY_BUNDLE_BY_ID = "bundle_";

    public GetProductBundleByIdQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetProductBundleByIdQueryHandler> logger,
        ICacheService cache,
        IOptions<CacheSettings> cacheSettings)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _cache = cache;
        _cacheSettings = cacheSettings.Value;
    }

    public async Task<ProductBundleDto?> Handle(GetProductBundleByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching product bundle by Id: {BundleId}", request.Id);

        // ✅ BOLUM 10.1: Cache-Aside Pattern
        var cacheKey = $"{CACHE_KEY_BUNDLE_BY_ID}{request.Id}";
        var cachedBundle = await _cache.GetAsync<ProductBundleDto>(cacheKey, cancellationToken);
        if (cachedBundle != null)
        {
            _logger.LogInformation("Product bundle retrieved from cache. BundleId: {BundleId}", request.Id);
            return cachedBundle;
        }

        // ✅ PERFORMANCE: AsNoTracking for read-only queries, removed !b.IsDeleted (Global Query Filter)
        var bundle = await _context.Set<ProductBundle>()
            .AsNoTracking()
            .Include(b => b.BundleItems)
                .ThenInclude(bi => bi.Product)
            .FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken);

        if (bundle == null)
        {
            _logger.LogWarning("Product bundle not found. BundleId: {BundleId}", request.Id);
            return null;
        }

        var bundleDto = _mapper.Map<ProductBundleDto>(bundle);

        // ✅ BOLUM 10.1: Cache-Aside Pattern - Cache'e yaz
        // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma (Clean Architecture)
        await _cache.SetAsync(cacheKey, bundleDto, TimeSpan.FromMinutes(_cacheSettings.ProductBundleCacheExpirationMinutes), cancellationToken);

        _logger.LogInformation("Product bundle retrieved successfully. BundleId: {BundleId}", request.Id);

        return bundleDto;
    }
}
