using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Product;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;

namespace Merge.Application.Product.Queries.GetProductSizeGuide;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetProductSizeGuideQueryHandler : IRequestHandler<GetProductSizeGuideQuery, ProductSizeGuideDto?>
{
    private readonly IDbContext _context;
    private readonly AutoMapper.IMapper _mapper;
    private readonly ILogger<GetProductSizeGuideQueryHandler> _logger;
    private readonly ICacheService _cache;
    private const string CACHE_KEY_PRODUCT_SIZE_GUIDE = "product_size_guide_";
    private static readonly TimeSpan CACHE_EXPIRATION = TimeSpan.FromMinutes(30); // Size guides change less frequently

    public GetProductSizeGuideQueryHandler(
        IDbContext context,
        AutoMapper.IMapper mapper,
        ILogger<GetProductSizeGuideQueryHandler> logger,
        ICacheService cache)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _cache = cache;
    }

    public async Task<ProductSizeGuideDto?> Handle(GetProductSizeGuideQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching product size guide. ProductId: {ProductId}", request.ProductId);

        // ✅ BOLUM 10.1: Cache-Aside Pattern
        var cacheKey = $"{CACHE_KEY_PRODUCT_SIZE_GUIDE}{request.ProductId}";
        var cachedProductSizeGuide = await _cache.GetAsync<ProductSizeGuideDto>(cacheKey, cancellationToken);
        if (cachedProductSizeGuide != null)
        {
            _logger.LogInformation("Product size guide retrieved from cache. ProductId: {ProductId}", request.ProductId);
            return cachedProductSizeGuide;
        }

        _logger.LogInformation("Cache miss for product size guide. ProductId: {ProductId}", request.ProductId);

        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        var productSizeGuide = await _context.Set<ProductSizeGuide>()
            .AsNoTracking()
            .Include(psg => psg.Product)
            .Include(psg => psg.SizeGuide)
                .ThenInclude(sg => sg.Category)
            .Include(psg => psg.SizeGuide)
                .ThenInclude(sg => sg.Entries)
            .FirstOrDefaultAsync(psg => psg.ProductId == request.ProductId, cancellationToken);

        if (productSizeGuide == null)
        {
            _logger.LogWarning("Product size guide not found. ProductId: {ProductId}", request.ProductId);
            return null;
        }

        var sizeGuideDto = _mapper.Map<SizeGuideDto>(productSizeGuide.SizeGuide);
        // ✅ BOLUM 7.1.5: Records - Record constructor kullanımı (object initializer YASAK)
        var productSizeGuideDto = new ProductSizeGuideDto(
            ProductId: productSizeGuide.ProductId,
            ProductName: productSizeGuide.Product.Name,
            SizeGuide: sizeGuideDto,
            CustomNotes: productSizeGuide.CustomNotes,
            FitDescription: productSizeGuide.FitDescription
        );

        // ✅ BOLUM 10.1: Cache-Aside Pattern - Cache'e yaz
        await _cache.SetAsync(cacheKey, productSizeGuideDto, CACHE_EXPIRATION, cancellationToken);

        _logger.LogInformation("Product size guide retrieved successfully. ProductId: {ProductId}", request.ProductId);

        return productSizeGuideDto;
    }
}
