using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AutoMapper;
using Merge.Application.Common;
using Merge.Application.DTOs.Product;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Product.Queries.GetProductsByCategory;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetProductsByCategoryQueryHandler : IRequestHandler<GetProductsByCategoryQuery, PagedResult<ProductDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetProductsByCategoryQueryHandler> _logger;
    private readonly ICacheService _cache;
    private readonly PaginationSettings _paginationSettings;
    private readonly CacheSettings _cacheSettings;
    private const string CACHE_KEY_PRODUCTS_BY_CATEGORY = "products_by_category_";

    public GetProductsByCategoryQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetProductsByCategoryQueryHandler> logger,
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

    public async Task<PagedResult<ProductDto>> Handle(GetProductsByCategoryQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching products by category. CategoryId: {CategoryId}, Page: {Page}, PageSize: {PageSize}",
            request.CategoryId, request.Page, request.PageSize);

        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize > _paginationSettings.MaxPageSize
            ? _paginationSettings.MaxPageSize
            : request.PageSize;

        var cacheKey = $"{CACHE_KEY_PRODUCTS_BY_CATEGORY}{request.CategoryId}_{page}_{pageSize}";

        // ✅ BOLUM 10.2: Redis distributed cache for frequently accessed data
        var cachedResult = await _cache.GetOrCreateAsync(
            cacheKey,
            async () =>
            {
                _logger.LogInformation("Cache miss for products by category. CategoryId: {CategoryId}, Page: {Page}, PageSize: {PageSize}",
                    request.CategoryId, request.Page, request.PageSize);

                // ✅ PERFORMANCE: AsNoTracking for read-only queries
                // ✅ PERFORMANCE: Removed manual !p.IsDeleted check (Global Query Filter handles it)
                var query = _context.Set<ProductEntity>()
                    .AsNoTracking()
                    .Include(p => p.Category)
                    .Where(p => p.IsActive && p.CategoryId == request.CategoryId);

                var totalCount = await query.CountAsync(cancellationToken);

                var products = await query
                    .OrderByDescending(p => p.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(cancellationToken);

                // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
                _logger.LogInformation(
                    "Retrieved products by category. CategoryId: {CategoryId}, Count: {Count}, TotalCount: {TotalCount}",
                    request.CategoryId, products.Count, totalCount);

                var dtos = _mapper.Map<IEnumerable<ProductDto>>(products);

                return new PagedResult<ProductDto>
                {
                    Items = dtos.ToList(),
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize
                };
            },
            TimeSpan.FromMinutes(_cacheSettings.ProductCategoryCacheExpirationMinutes),
            cancellationToken);

        return cachedResult!;
    }
}
