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

namespace Merge.Application.Product.Queries.GetAllProducts;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetAllProductsQueryHandler : IRequestHandler<GetAllProductsQuery, PagedResult<ProductDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAllProductsQueryHandler> _logger;
    private readonly ICacheService _cache;
    private readonly PaginationSettings _paginationSettings;
    private const string CACHE_KEY_ALL_PRODUCTS_PAGED = "products_all_paged";
    private static readonly TimeSpan CACHE_EXPIRATION = TimeSpan.FromMinutes(15); // Products change more frequently than categories

    public GetAllProductsQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetAllProductsQueryHandler> logger,
        ICacheService cache,
        IOptions<PaginationSettings> paginationSettings)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _cache = cache;
        _paginationSettings = paginationSettings.Value;
    }

    public async Task<PagedResult<ProductDto>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching all products. Page: {Page}, PageSize: {PageSize}",
            request.Page, request.PageSize);

        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize > _paginationSettings.MaxPageSize
            ? _paginationSettings.MaxPageSize
            : request.PageSize;

        var cacheKey = $"{CACHE_KEY_ALL_PRODUCTS_PAGED}_{page}_{pageSize}";

        // ✅ BOLUM 10.2: Redis distributed cache for frequently accessed data
        var cachedResult = await _cache.GetOrCreateAsync(
            cacheKey,
            async () =>
            {
                _logger.LogInformation("Cache miss for all products (paged). Fetching from database.");

                // ✅ PERFORMANCE: AsNoTracking for read-only queries
                // ✅ PERFORMANCE: Removed manual !p.IsDeleted check (Global Query Filter handles it)
                var query = _context.Set<ProductEntity>()
                    .AsNoTracking()
                    .Include(p => p.Category)
                    .Where(p => p.IsActive);

                var totalCount = await query.CountAsync(cancellationToken);

                var products = await query
                    .OrderByDescending(p => p.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(cancellationToken);

                var dtos = _mapper.Map<IEnumerable<ProductDto>>(products);

                return new PagedResult<ProductDto>
                {
                    Items = dtos.ToList(),
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize
                };
            },
            CACHE_EXPIRATION,
            cancellationToken);

        return cachedResult!;
    }
}
