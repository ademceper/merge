using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AutoMapper;
using Merge.Application.DTOs.Product;
using Merge.Application.DTOs.Search;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Search.Queries.SearchProducts;

public class SearchProductsQueryHandler(IDbContext context, IMapper mapper, ILogger<SearchProductsQueryHandler> logger, ICacheService cache, IOptions<SearchSettings> searchSettings) : IRequestHandler<SearchProductsQuery, SearchResultDto>
{
    private const string CACHE_KEY_PRODUCTS_SEARCH = "products_search_";

    public async Task<SearchResultDto> Handle(SearchProductsQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Product search yapılıyor. SearchTerm: {SearchTerm}, CategoryId: {CategoryId}, Page: {Page}, PageSize: {PageSize}",
            request.SearchTerm, request.CategoryId, request.Page, request.PageSize);

        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize > searchSettings.Value.MaxPageSize
            ? searchSettings.Value.MaxPageSize
            : request.PageSize;

        // Cache key
        var cacheKey = $"{CACHE_KEY_PRODUCTS_SEARCH}{request.SearchTerm}_{request.CategoryId}_{request.Brand}_{request.MinPrice}_{request.MaxPrice}_{request.MinRating}_{request.InStockOnly}_{request.SortBy}_{page}_{pageSize}";

        var cachedResult = await cache.GetOrCreateAsync(
            cacheKey,
            async () =>
            {
                var query = context.Set<ProductEntity>()
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

                var products = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(cancellationToken);

                var productDtos = mapper.Map<IEnumerable<ProductDto>>(products).ToList();

                var rankedProducts = ApplySearchRanking(productDtos, request.SearchTerm ?? string.Empty, request.SortBy);

                var brands = await context.Set<ProductEntity>()
                    .AsNoTracking()
                    .Where(p => p.IsActive && !string.IsNullOrEmpty(p.Brand))
                    .Select(p => p.Brand)
                    .Distinct()
                    .OrderBy(b => b)
                    .ToListAsync(cancellationToken);

                var minPrice = await context.Set<ProductEntity>()
                    .AsNoTracking()
                    .Where(p => p.IsActive)
                    .MinAsync(p => (decimal?)p.Price, cancellationToken) ?? 0;

                var maxPrice = await context.Set<ProductEntity>()
                    .AsNoTracking()
                    .Where(p => p.IsActive)
                    .MaxAsync(p => (decimal?)p.Price, cancellationToken) ?? 0;

                logger.LogInformation(
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
            TimeSpan.FromMinutes(searchSettings.Value.SearchCacheExpirationMinutes),
            cancellationToken);

        return cachedResult!;
    }

    private List<ProductDto> ApplySearchRanking(List<ProductDto> products, string searchTerm, string? sortBy)
    {
        var sortByNorm = string.IsNullOrEmpty(sortBy) ? null : sortBy.ToLowerInvariant();
        if (sortByNorm is not null && sortByNorm != "relevance")
        {
            return sortByNorm switch
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

        if (product.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
        {
            score += 100;
            if (product.Name.StartsWith(searchTerm, StringComparison.OrdinalIgnoreCase))
            {
                score += 50;
            }
        }

        if (string.Equals(product.Name, searchTerm, StringComparison.OrdinalIgnoreCase))
        {
            score += 200;
        }

        if ((product.Brand ?? string.Empty).Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
        {
            score += 30;
        }

        if (product.SKU.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
        {
            score += 20;
        }

        if ((product.Description ?? string.Empty).Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
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
