using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Product;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Product.Queries.GetSizeGuidesByCategory;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetSizeGuidesByCategoryQueryHandler : IRequestHandler<GetSizeGuidesByCategoryQuery, IEnumerable<SizeGuideDto>>
{
    private readonly IDbContext _context;
    private readonly AutoMapper.IMapper _mapper;
    private readonly ILogger<GetSizeGuidesByCategoryQueryHandler> _logger;
    private readonly ICacheService _cache;
    private readonly CacheSettings _cacheSettings;
    private const string CACHE_KEY_SIZE_GUIDES_BY_CATEGORY = "size_guides_by_category_";

    public GetSizeGuidesByCategoryQueryHandler(
        IDbContext context,
        AutoMapper.IMapper mapper,
        ILogger<GetSizeGuidesByCategoryQueryHandler> logger,
        ICacheService cache,
        IOptions<CacheSettings> cacheSettings)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _cache = cache;
        _cacheSettings = cacheSettings.Value;
    }

    public async Task<IEnumerable<SizeGuideDto>> Handle(GetSizeGuidesByCategoryQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching size guides by category. CategoryId: {CategoryId}", request.CategoryId);

        // ✅ BOLUM 10.1: Cache-Aside Pattern
        var cacheKey = $"{CACHE_KEY_SIZE_GUIDES_BY_CATEGORY}{request.CategoryId}";
        var cachedSizeGuides = await _cache.GetAsync<IEnumerable<SizeGuideDto>>(cacheKey, cancellationToken);
        if (cachedSizeGuides != null)
        {
            _logger.LogInformation("Size guides retrieved from cache. CategoryId: {CategoryId}", request.CategoryId);
            return cachedSizeGuides;
        }

        _logger.LogInformation("Cache miss for size guides by category. CategoryId: {CategoryId}", request.CategoryId);

        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: AsSplitQuery to prevent Cartesian Explosion (multiple Includes)
        var sizeGuides = await _context.Set<SizeGuide>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(sg => sg.Category)
            .Include(sg => sg.Entries)
            .Where(sg => sg.CategoryId == request.CategoryId && sg.IsActive)
            .ToListAsync(cancellationToken);

        var sizeGuideDtos = _mapper.Map<IEnumerable<SizeGuideDto>>(sizeGuides).ToList();

        // ✅ BOLUM 10.1: Cache-Aside Pattern - Cache'e yaz
        // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma (Clean Architecture)
        await _cache.SetAsync(cacheKey, sizeGuideDtos, TimeSpan.FromMinutes(_cacheSettings.SizeGuideCacheExpirationMinutes), cancellationToken);

        _logger.LogInformation("Retrieved size guides by category. CategoryId: {CategoryId}, Count: {Count}", 
            request.CategoryId, sizeGuides.Count);

        return sizeGuideDtos;
    }
}
