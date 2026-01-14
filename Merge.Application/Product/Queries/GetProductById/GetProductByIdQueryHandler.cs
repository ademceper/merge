using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AutoMapper;
using Merge.Application.DTOs.Product;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Product.Queries.GetProductById;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDto?>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetProductByIdQueryHandler> _logger;
    private readonly ICacheService _cache;
    private readonly CacheSettings _cacheSettings;
    private const string CACHE_KEY_PRODUCT_BY_ID = "product_";

    public GetProductByIdQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetProductByIdQueryHandler> logger,
        ICacheService cache,
        IOptions<CacheSettings> cacheSettings)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _cache = cache;
        _cacheSettings = cacheSettings.Value;
    }

    public async Task<ProductDto?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching product by Id: {ProductId}", request.ProductId);

        var cacheKey = $"{CACHE_KEY_PRODUCT_BY_ID}{request.ProductId}";

        // ✅ BOLUM 10.2: Redis distributed cache for single product (shorter TTL due to frequent updates)
        var cachedProduct = await _cache.GetAsync<ProductDto>(cacheKey, cancellationToken);
        if (cachedProduct != null)
        {
            _logger.LogInformation("Cache hit for product. ProductId: {ProductId}", request.ProductId);
            return cachedProduct;
        }

        _logger.LogInformation("Cache miss for product. ProductId: {ProductId}", request.ProductId);

        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !p.IsDeleted check (Global Query Filter handles it)
        var product = await _context.Set<ProductEntity>()
            .AsNoTracking()
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

        if (product == null)
        {
            _logger.LogWarning("Product not found with Id: {ProductId}", request.ProductId);
            return null;
        }

        var productDto = _mapper.Map<ProductDto>(product);
        
        // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma (Clean Architecture)
        // Cache the result
        var cacheExpiration = TimeSpan.FromMinutes(_cacheSettings.ProductCacheExpirationMinutes);
        await _cache.SetAsync(cacheKey, productDto, cacheExpiration, cancellationToken);

        return productDto;
    }
}
