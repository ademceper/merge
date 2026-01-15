using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Search;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using OrderEntity = Merge.Domain.Modules.Ordering.Order;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using Merge.Application.DTOs.Product;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Ordering;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;


namespace Merge.Application.Services.Search;

public class ProductRecommendationService : IProductRecommendationService
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<ProductRecommendationService> _logger;

    public ProductRecommendationService(IDbContext context, IMapper mapper, ILogger<ProductRecommendationService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<IEnumerable<ProductRecommendationDto>> GetSimilarProductsAsync(Guid productId, int maxResults = 10, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !p.IsDeleted (Global Query Filter)
        var product = await _context.Set<ProductEntity>()
            .AsNoTracking()
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);

        if (product == null)
        {
            return Enumerable.Empty<ProductRecommendationDto>();
        }

        // Find products in same category with similar price range
        var priceMin = product.Price * 0.7m;
        var priceMax = product.Price * 1.3m;

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !p.IsDeleted (Global Query Filter)
        var similarProducts = await _context.Set<ProductEntity>()
            .AsNoTracking()
            .Where(p => p.IsActive &&
                       p.Id != productId &&
                       p.CategoryId == product.CategoryId &&
                       p.Price >= priceMin &&
                       p.Price <= priceMax)
            .OrderByDescending(p => p.Rating)
            .ThenByDescending(p => p.ReviewCount)
            .Take(maxResults)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        // ✅ BOLUM 7.1.5: Records - Record constructor kullanımı (object initializer YASAK)
        var recommendations = _mapper.Map<IEnumerable<ProductRecommendationDto>>(similarProducts)
            .Select(rec => new ProductRecommendationDto(
                rec.ProductId,
                rec.Name,
                rec.Description,
                rec.Price,
                rec.DiscountPrice,
                rec.ImageUrl,
                rec.Rating,
                rec.ReviewCount,
                "Similar to what you're viewing",
                rec.Rating
            ))
            .ToList();
        return recommendations;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<IEnumerable<ProductRecommendationDto>> GetFrequentlyBoughtTogetherAsync(Guid productId, int maxResults = 5, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !oi.IsDeleted, !oi2.IsDeleted (Global Query Filter)
        // ✅ PERFORMANCE: Database'de grouping yap (memory'de işlem YASAK)
        // ✅ PERFORMANCE: Explicit Join yaklaşımı - tek sorgu (N+1 fix)
        // Find products that are frequently purchased together
        var frequentlyBought = await (
            from oi1 in _context.Set<OrderItem>().AsNoTracking()
            join oi2 in _context.Set<OrderItem>().AsNoTracking()
                on oi1.OrderId equals oi2.OrderId
            where oi1.ProductId == productId && oi2.ProductId != productId
            group oi2 by oi2.ProductId into g
            orderby g.Count() descending
            select new
            {
                ProductId = g.Key,
                Count = g.Count()
            }
        ).Take(maxResults).ToListAsync(cancellationToken);

        // ✅ PERFORMANCE: Batch load products to avoid N+1 queries
        var productIds = frequentlyBought.Select(fb => fb.ProductId).ToList();
        var products = await _context.Set<ProductEntity>()
            .AsNoTracking()
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        // ✅ BOLUM 7.1.5: Records - Record constructor kullanımı (object initializer YASAK)
        var recommendations = new List<ProductRecommendationDto>();
        foreach (var fb in frequentlyBought)
        {
            if (products.TryGetValue(fb.ProductId, out var product))
            {
                var rec = _mapper.Map<ProductRecommendationDto>(product);
                recommendations.Add(new ProductRecommendationDto(
                    rec.ProductId,
                    rec.Name,
                    rec.Description,
                    rec.Price,
                    rec.DiscountPrice,
                    rec.ImageUrl,
                    rec.Rating,
                    rec.ReviewCount,
                    "Frequently bought together",
                    fb.Count
                ));
            }
        }

        return recommendations;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<IEnumerable<ProductRecommendationDto>> GetPersonalizedRecommendationsAsync(Guid userId, int maxResults = 10, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Subquery yaklaşımı - memory'de hiçbir şey tutma (ISSUE #3.1 fix)
        // Get user's purchase history categories
        var userOrderCategoriesSubquery = (
            from o in _context.Set<OrderEntity>().AsNoTracking()
            join oi in _context.Set<OrderItem>().AsNoTracking() on o.Id equals oi.OrderId
            join p in _context.Set<ProductEntity>().AsNoTracking() on oi.ProductId equals p.Id
            where o.UserId == userId
            select p.CategoryId
        ).Distinct();

        // Get user's wishlist categories
        var wishlistCategoriesSubquery = from w in _context.Set<Wishlist>().AsNoTracking()
                                        join p in _context.Set<ProductEntity>().AsNoTracking() on w.ProductId equals p.Id
                                        where w.UserId == userId
                                        select p.CategoryId;

        // Combine categories using Union in subquery
        var preferredCategoriesSubquery = userOrderCategoriesSubquery.Union(wishlistCategoriesSubquery);

        // Check if user has any preferences
        var hasPreferences = await preferredCategoriesSubquery.AnyAsync(cancellationToken);
        if (!hasPreferences)
        {
            // If no history, return trending products
            return await GetTrendingProductsAsync(7, maxResults, cancellationToken);
        }

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !p.IsDeleted (Global Query Filter)
        // Get highly rated products from preferred categories
        var recommendations = await _context.Set<ProductEntity>()
            .AsNoTracking()
            .Where(p => p.IsActive &&
                       preferredCategoriesSubquery.Contains(p.CategoryId) &&
                       p.Rating >= 4.0m)
            .OrderByDescending(p => p.Rating)
            .ThenByDescending(p => p.ReviewCount)
            .Take(maxResults)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        // ✅ BOLUM 7.1.5: Records - Record constructor kullanımı (object initializer YASAK)
        var recommendationDtos = _mapper.Map<IEnumerable<ProductRecommendationDto>>(recommendations)
            .Select(rec => new ProductRecommendationDto(
                rec.ProductId,
                rec.Name,
                rec.Description,
                rec.Price,
                rec.DiscountPrice,
                rec.ImageUrl,
                rec.Rating,
                rec.ReviewCount,
                "Based on your interests",
                rec.Rating * rec.ReviewCount
            ))
            .ToList();
        return recommendationDtos;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<IEnumerable<ProductRecommendationDto>> GetBasedOnViewHistoryAsync(Guid userId, int maxResults = 10, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !rv.IsDeleted (Global Query Filter)
        // Get recently viewed products
        // ✅ PERFORMANCE: AsSplitQuery to prevent Cartesian Explosion (ThenInclude)
        var recentlyViewed = await _context.Set<Merge.Domain.Modules.Catalog.RecentlyViewedProduct>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(rv => rv.Product)
                .ThenInclude(p => p.Category)
            .Where(rv => rv.UserId == userId)
            .OrderByDescending(rv => rv.ViewedAt)
            .Take(5)
            .ToListAsync(cancellationToken);

        if (recentlyViewed.Count == 0)
        {
            return Enumerable.Empty<ProductRecommendationDto>();
        }

        // ✅ PERFORMANCE: recentlyViewed zaten materialize edilmiş küçük liste (5 item), bu yüzden ID'leri almak kabul edilebilir
        // Ancak category'ler için subquery kullanıyoruz (ISSUE #3.1 fix)
        var viewedProductIds = recentlyViewed.Select(rv => rv.ProductId).ToList();
        
        // Category'ler için subquery kullan (büyük olabilir)
        var viewedProductIdsSubquery = from rv in _context.Set<Merge.Domain.Modules.Catalog.RecentlyViewedProduct>().AsNoTracking()
                                      where rv.UserId == userId
                                      orderby rv.ViewedAt descending
                                      select rv.ProductId;
        var viewedCategoriesSubquery = from rv in _context.Set<Merge.Domain.Modules.Catalog.RecentlyViewedProduct>().AsNoTracking()
                                      join p in _context.Set<ProductEntity>().AsNoTracking() on rv.ProductId equals p.Id
                                      where rv.UserId == userId
                                      orderby rv.ViewedAt descending
                                      select p.CategoryId;

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !p.IsDeleted (Global Query Filter)
        // Get products from same categories, excluding already viewed
        var recommendations = await _context.Set<ProductEntity>()
            .AsNoTracking()
            .Where(p => p.IsActive &&
                       viewedCategoriesSubquery.Distinct().Contains(p.CategoryId) &&
                       !viewedProductIds.Contains(p.Id))
            .OrderByDescending(p => p.Rating)
            .ThenByDescending(p => p.ReviewCount)
            .Take(maxResults)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        // ✅ BOLUM 7.1.5: Records - Record constructor kullanımı (object initializer YASAK)
        var recommendationDtos = _mapper.Map<IEnumerable<ProductRecommendationDto>>(recommendations)
            .Select(rec => new ProductRecommendationDto(
                rec.ProductId,
                rec.Name,
                rec.Description,
                rec.Price,
                rec.DiscountPrice,
                rec.ImageUrl,
                rec.Rating,
                rec.ReviewCount,
                "Based on your browsing history",
                rec.Rating
            ))
            .ToList();
        return recommendationDtos;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<IEnumerable<ProductRecommendationDto>> GetTrendingProductsAsync(int days = 7, int maxResults = 10, CancellationToken cancellationToken = default)
    {
        var startDate = DateTime.UtcNow.AddDays(-days);

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !oi.IsDeleted (Global Query Filter)
        // ✅ PERFORMANCE: Database'de grouping yap (memory'de işlem YASAK)
        var trendingProducts = await _context.Set<OrderItem>()
            .AsNoTracking()
            .Include(oi => oi.Product)
            .Where(oi => oi.CreatedAt >= startDate)
            .GroupBy(oi => oi.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                RecommendationScore = g.Sum(oi => oi.Quantity)
            })
            .OrderByDescending(x => x.RecommendationScore)
            .Take(maxResults)
            .ToListAsync(cancellationToken);

        // ✅ PERFORMANCE: Batch load products to avoid N+1 queries
        var productIds = trendingProducts.Select(tp => tp.ProductId).ToList();
        var products = await _context.Set<ProductEntity>()
            .AsNoTracking()
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        // ✅ BOLUM 7.1.5: Records - Record constructor kullanımı (object initializer YASAK)
        var recommendations = new List<ProductRecommendationDto>();
        foreach (var tp in trendingProducts)
        {
            if (products.TryGetValue(tp.ProductId, out var product))
            {
                var rec = _mapper.Map<ProductRecommendationDto>(product);
                recommendations.Add(new ProductRecommendationDto(
                    rec.ProductId,
                    rec.Name,
                    rec.Description,
                    rec.Price,
                    rec.DiscountPrice,
                    rec.ImageUrl,
                    rec.Rating,
                    rec.ReviewCount,
                    $"Trending in last {days} days",
                    tp.RecommendationScore
                ));
            }
        }
        return recommendations;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<IEnumerable<ProductRecommendationDto>> GetBestSellersAsync(int maxResults = 10, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !oi.IsDeleted (Global Query Filter)
        // ✅ PERFORMANCE: Database'de grouping yap (memory'de işlem YASAK)
        var bestSellers = await _context.Set<OrderItem>()
            .AsNoTracking()
            .Include(oi => oi.Product)
            .GroupBy(oi => oi.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                RecommendationScore = g.Sum(oi => oi.Quantity)
            })
            .OrderByDescending(x => x.RecommendationScore)
            .Take(maxResults)
            .ToListAsync(cancellationToken);

        // ✅ PERFORMANCE: Batch load products to avoid N+1 queries
        var productIds = bestSellers.Select(bs => bs.ProductId).ToList();
        var products = await _context.Set<ProductEntity>()
            .AsNoTracking()
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        // ✅ BOLUM 7.1.5: Records - Record constructor kullanımı (object initializer YASAK)
        var recommendations = new List<ProductRecommendationDto>();
        foreach (var bs in bestSellers)
        {
            if (products.TryGetValue(bs.ProductId, out var product))
            {
                var rec = _mapper.Map<ProductRecommendationDto>(product);
                recommendations.Add(new ProductRecommendationDto(
                    rec.ProductId,
                    rec.Name,
                    rec.Description,
                    rec.Price,
                    rec.DiscountPrice,
                    rec.ImageUrl,
                    rec.Rating,
                    rec.ReviewCount,
                    "Best seller",
                    bs.RecommendationScore
                ));
            }
        }
        return recommendations;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<IEnumerable<ProductRecommendationDto>> GetNewArrivalsAsync(int days = 30, int maxResults = 10, CancellationToken cancellationToken = default)
    {
        var startDate = DateTime.UtcNow.AddDays(-days);

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !p.IsDeleted (Global Query Filter)
        var newArrivals = await _context.Set<ProductEntity>()
            .AsNoTracking()
            .Where(p => p.IsActive && p.CreatedAt >= startDate)
            .OrderByDescending(p => p.CreatedAt)
            .Take(maxResults)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        // ✅ BOLUM 7.1.5: Records - Record constructor kullanımı (object initializer YASAK)
        var recommendations = _mapper.Map<IEnumerable<ProductRecommendationDto>>(newArrivals)
            .Select(rec => new ProductRecommendationDto(
                rec.ProductId,
                rec.Name,
                rec.Description,
                rec.Price,
                rec.DiscountPrice,
                rec.ImageUrl,
                rec.Rating,
                rec.ReviewCount,
                "New arrival",
                0
            ))
            .ToList();
        return recommendations;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<PersonalizedRecommendationsDto> GetCompleteRecommendationsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: ToListAsync() sonrası memory'de işlem YASAK - ama bu sadece property assignment (kabul edilebilir)
        // ✅ BOLUM 7.1.5: Records - Record constructor kullanımı (object initializer YASAK)
        var recommendations = new PersonalizedRecommendationsDto(
            ForYou: (await GetPersonalizedRecommendationsAsync(userId, 10, cancellationToken)).ToList(),
            BasedOnHistory: (await GetBasedOnViewHistoryAsync(userId, 10, cancellationToken)).ToList(),
            Trending: (await GetTrendingProductsAsync(7, 10, cancellationToken)).ToList(),
            BestSellers: (await GetBestSellersAsync(10, cancellationToken)).ToList()
        );

        return recommendations;
    }
}
