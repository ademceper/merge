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

namespace Merge.Application.Product.Queries.GetSizeGuide;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetSizeGuideQueryHandler : IRequestHandler<GetSizeGuideQuery, SizeGuideDto?>
{
    private readonly IDbContext _context;
    private readonly AutoMapper.IMapper _mapper;
    private readonly ILogger<GetSizeGuideQueryHandler> _logger;
    private readonly ICacheService _cache;
    private readonly CacheSettings _cacheSettings;
    private const string CACHE_KEY_SIZE_GUIDE_BY_ID = "size_guide_";

    public GetSizeGuideQueryHandler(
        IDbContext context,
        AutoMapper.IMapper mapper,
        ILogger<GetSizeGuideQueryHandler> logger,
        ICacheService cache,
        IOptions<CacheSettings> cacheSettings)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _cache = cache;
        _cacheSettings = cacheSettings.Value;
    }

    public async Task<SizeGuideDto?> Handle(GetSizeGuideQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching size guide by Id: {SizeGuideId}", request.Id);

        // ✅ BOLUM 10.1: Cache-Aside Pattern
        var cacheKey = $"{CACHE_KEY_SIZE_GUIDE_BY_ID}{request.Id}";
        var cachedSizeGuide = await _cache.GetAsync<SizeGuideDto>(cacheKey, cancellationToken);
        if (cachedSizeGuide != null)
        {
            _logger.LogInformation("Size guide retrieved from cache. SizeGuideId: {SizeGuideId}", request.Id);
            return cachedSizeGuide;
        }

        _logger.LogInformation("Cache miss for size guide. SizeGuideId: {SizeGuideId}", request.Id);

        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: AsSplitQuery to prevent Cartesian Explosion (multiple Includes)
        var sizeGuide = await _context.Set<SizeGuide>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(sg => sg.Category)
            .Include(sg => sg.Entries)
            .FirstOrDefaultAsync(sg => sg.Id == request.Id, cancellationToken);

        if (sizeGuide == null)
        {
            _logger.LogWarning("Size guide not found with Id: {SizeGuideId}", request.Id);
            return null;
        }

        var sizeGuideDto = _mapper.Map<SizeGuideDto>(sizeGuide);

        // ✅ BOLUM 10.1: Cache-Aside Pattern - Cache'e yaz
        // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma (Clean Architecture)
        await _cache.SetAsync(cacheKey, sizeGuideDto, TimeSpan.FromMinutes(_cacheSettings.SizeGuideCacheExpirationMinutes), cancellationToken);

        _logger.LogInformation("Size guide retrieved successfully. SizeGuideId: {SizeGuideId}", request.Id);

        return sizeGuideDto;
    }
}
