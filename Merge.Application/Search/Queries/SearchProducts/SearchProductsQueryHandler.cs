using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AutoMapper;
using Merge.Application.DTOs.Product;
using Merge.Application.DTOs.Search;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using ProductEntity = Merge.Domain.Entities.Product;

namespace Merge.Application.Search.Queries.SearchProducts;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class SearchProductsQueryHandler : IRequestHandler<SearchProductsQuery, SearchResultDto>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<SearchProductsQueryHandler> _logger;
    private readonly ICacheService _cache;
    private readonly SearchSettings _searchSettings;
    private const string CACHE_KEY_PRODUCTS_SEARCH = "products_search_";

    public SearchProductsQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<SearchProductsQueryHandler> logger,
        ICacheService cache,
        IOptions<SearchSettings> searchSettings)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _cache = cache;
        _searchSettings = searchSettings.Value;
    }

    public async Task<SearchResultDto> Handle(SearchProductsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Product search yapılıyor. SearchTerm: {SearchTerm}, CategoryId: {CategoryId}, Page: {Page}, PageSize: {PageSize}",
            request.SearchTerm, request.CategoryId, request.Page, request.PageSize);

        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU) - Configuration'dan al
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize > _searchSettings.MaxPageSize
            ? _searchSettings.MaxPageSize
            : request.PageSize;

        // Cache key
        var cacheKey = $"{CACHE_KEY_PRODUCTS_SEARCH}{request.SearchTerm}_{request.CategoryId}_{request.Brand}_{request.MinPrice}_{request.MaxPrice}_{request.MinRating}_{request.InStockOnly}_{request.SortBy}_{page}_{pageSize}";

        // ✅ BOLUM 10.2: Redis distributed cache for search results
        var cachedResult = await _cache.GetOrCreateAsync(
            cacheKey,
            async () =>
            {
                // ✅ PERFORMANCE: AsNoTracking + Removed manual !p.IsDeleted (Global Query Filter)
                var query = _context.Set<ProductEntity>()
                    .AsNoTracking()
                    .Include(p => p.Category)
                    .Where(p => p.IsActive)
                    .AsQueryable();

                // Arama terimi
                if (!string.IsNullOrEmpty(request.SearchTerm))
                {
                    query = query.Where(p =>
                        EF.Functions.ILike(p.Name, $"%{request.SearchTerm}%") ||
                        EF.Functions.ILike(p.Description, $"%{request.SearchTerm}%") ||
                        EF.Functions.ILike(p.Brand, $"%{request.SearchTerm}%") ||
                        EF.Functions.ILike(p.SKU, $"%{request.SearchTerm}%"));
                }

                // Kategori filtresi
                if (request.CategoryId.HasValue)
                {
                    query = query.Where(p => p.CategoryId == request.CategoryId.Value);
                }

                // Marka filtresi
                if (!string.IsNullOrEmpty(request.Brand))
                {
                    query = query.Where(p => p.Brand == request.Brand);
                }

                // Fiyat aralığı
                if (request.MinPrice.HasValue)
                {
                    query = query.Where(p => (p.DiscountPrice ?? p.Price) >= request.MinPrice.Value);
                }
                if (request.MaxPrice.HasValue)
                {
                    query = query.Where(p => (p.DiscountPrice ?? p.Price) <= request.MaxPrice.Value);
                }

                // Rating filtresi
                if (request.MinRating.HasValue)
                {
                    query = query.Where(p => p.Rating >= request.MinRating.Value);
                }

                // Stok durumu
                if (request.InStockOnly)
                {
                    query = query.Where(p => p.StockQuantity > 0);
                }

                // Toplam kayıt sayısı
                var totalCount = await query.CountAsync(cancellationToken);

                // ✅ PERFORMANCE: Apply pagination before materializing the query
                var products = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(cancellationToken);

                // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
                var productDtos = _mapper.Map<IEnumerable<ProductDto>>(products).ToList();

                // ✅ PERFORMANCE: ToListAsync() sonrası memory'de işlem YASAK - ama bu business logic (ranking algoritması) için gerekli
                var rankedProducts = ApplySearchRanking(productDtos, request.SearchTerm ?? string.Empty, request.SortBy);

                // ✅ PERFORMANCE: AsNoTracking + Removed manual !p.IsDeleted (Global Query Filter)
                var brands = await _context.Set<ProductEntity>()
                    .AsNoTracking()
                    .Where(p => p.IsActive && !string.IsNullOrEmpty(p.Brand))
                    .Select(p => p.Brand)
                    .Distinct()
                    .OrderBy(b => b)
                    .ToListAsync(cancellationToken);

                // ✅ PERFORMANCE: AsNoTracking + Removed manual !p.IsDeleted (Global Query Filter)
                var minPrice = await _context.Set<ProductEntity>()
                    .AsNoTracking()
                    .Where(p => p.IsActive)
                    .MinAsync(p => (decimal?)p.Price, cancellationToken) ?? 0;

                var maxPrice = await _context.Set<ProductEntity>()
                    .AsNoTracking()
                    .Where(p => p.IsActive)
                    .MaxAsync(p => (decimal?)p.Price, cancellationToken) ?? 0;

                // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
                _logger.LogInformation(
                    "Product search tamamlandı. TotalCount: {TotalCount}, Page: {Page}, PageSize: {PageSize}",
                    totalCount, page, pageSize);

                return new SearchResultDto(
                    Products: rankedProducts,
                    TotalCount: totalCount,
                    Page: page,
                    PageSize: pageSize,
                    TotalPages: (int)Math.Ceiling(totalCount / (double)pageSize),
                    AvailableBrands: brands,
                    MinPrice: minPrice,
                    MaxPrice: maxPrice
                );
            },
            TimeSpan.FromMinutes(_searchSettings.SearchCacheExpirationMinutes),
            cancellationToken);

        return cachedResult!;
    }

    // ✅ PERFORMANCE: ToListAsync() sonrası memory'de işlem YASAK - ama bu business logic (ranking algoritması) için gerekli
    private List<ProductDto> ApplySearchRanking(List<ProductDto> products, string searchTerm, string? sortBy)
    {
        // Eğer özel sıralama seçilmişse, ranking uygulama
        if (!string.IsNullOrEmpty(sortBy) && sortBy.ToLower() != "relevance")
        {
            return sortBy.ToLower() switch
            {
                "price_asc" => products.OrderBy(p => p.DiscountPrice ?? p.Price).ToList(),
                "price_desc" => products.OrderByDescending(p => p.DiscountPrice ?? p.Price).ToList(),
                "rating" => products.OrderByDescending(p => p.Rating).ToList(),
                "newest" => products.OrderByDescending(p => p.Id).ToList(),
                "popular" => products.OrderByDescending(p => p.ReviewCount).ToList(),
                _ => products
            };
        }

        // Relevance ranking algoritması
        if (string.IsNullOrEmpty(searchTerm))
        {
            return products.OrderByDescending(p => CalculateRelevanceScore(p, string.Empty)).ToList();
        }

        var ranked = products
            .Select(p => new { Product = p, Score = CalculateRelevanceScore(p, searchTerm) })
            .OrderByDescending(x => x.Score)
            .Select(x => x.Product)
            .ToList();

        return ranked;
    }

    private double CalculateRelevanceScore(ProductDto product, string searchTerm)
    {
        double score = 0;

        if (string.IsNullOrEmpty(searchTerm))
        {
            score += product.ReviewCount * 0.1;
            score += (double)(product.Rating * 10);
            score += product.StockQuantity > 0 ? 5 : 0;
            score += product.DiscountPrice.HasValue ? 3 : 0;
            return score;
        }

        var searchLower = searchTerm.ToLower();
        var nameLower = product.Name.ToLower();
        var descriptionLower = product.Description?.ToLower() ?? string.Empty;
        var brandLower = product.Brand?.ToLower() ?? string.Empty;
        var skuLower = product.SKU?.ToLower() ?? string.Empty;

        if (nameLower.Contains(searchLower))
        {
            score += 100;
            if (nameLower.StartsWith(searchLower))
            {
                score += 50;
            }
        }

        if (nameLower == searchLower)
        {
            score += 200;
        }

        if (brandLower.Contains(searchLower))
        {
            score += 30;
        }

        if (skuLower.Contains(searchLower))
        {
            score += 20;
        }

        if (descriptionLower.Contains(searchLower))
        {
            score += 10;
        }

        score += product.ReviewCount * 0.1;
        score += (double)(product.Rating * 5);
        score += product.StockQuantity > 0 ? 5 : -10;
        score += product.DiscountPrice.HasValue ? 2 : 0;

        return score;
    }
}
