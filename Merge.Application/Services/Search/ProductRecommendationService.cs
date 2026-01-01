using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Search;
using Merge.Infrastructure.Data;
using Merge.Application.DTOs.Product;


namespace Merge.Application.Services.Search;

public class ProductRecommendationService : IProductRecommendationService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<ProductRecommendationService> _logger;

    public ProductRecommendationService(ApplicationDbContext context, IMapper mapper, ILogger<ProductRecommendationService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<ProductRecommendationDto>> GetSimilarProductsAsync(Guid productId, int maxResults = 10)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !p.IsDeleted (Global Query Filter)
        var product = await _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == productId);

        if (product == null)
        {
            return Enumerable.Empty<ProductRecommendationDto>();
        }

        // Find products in same category with similar price range
        var priceMin = product.Price * 0.7m;
        var priceMax = product.Price * 1.3m;

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !p.IsDeleted (Global Query Filter)
        var similarProducts = await _context.Products
            .AsNoTracking()
            .Where(p => p.IsActive &&
                       p.Id != productId &&
                       p.CategoryId == product.CategoryId &&
                       p.Price >= priceMin &&
                       p.Price <= priceMax)
            .OrderByDescending(p => p.Rating)
            .ThenByDescending(p => p.ReviewCount)
            .Take(maxResults)
            .ToListAsync();

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var recommendations = _mapper.Map<IEnumerable<ProductRecommendationDto>>(similarProducts).ToList();
        foreach (var rec in recommendations)
        {
            rec.RecommendationReason = "Similar to what you're viewing";
            rec.RecommendationScore = rec.Rating;
        }
        return recommendations;
    }

    public async Task<IEnumerable<ProductRecommendationDto>> GetFrequentlyBoughtTogetherAsync(Guid productId, int maxResults = 5)
    {
        // ✅ PERFORMANCE: Removed manual !oi.IsDeleted, !oi2.IsDeleted (Global Query Filter)
        // ✅ PERFORMANCE: Database'de grouping yap (memory'de işlem YASAK)
        // Find products that are frequently purchased together
        var frequentlyBought = await _context.OrderItems
            .AsNoTracking()
            .Where(oi => _context.OrderItems.Any(oi2 =>
                            oi2.OrderId == oi.OrderId &&
                            oi2.ProductId == productId))
            .Where(oi => oi.ProductId != productId)
            .GroupBy(oi => oi.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .Take(maxResults)
            .ToListAsync();

        // ✅ PERFORMANCE: Batch load products to avoid N+1 queries
        var productIds = frequentlyBought.Select(fb => fb.ProductId).ToList();
        var products = await _context.Products
            .AsNoTracking()
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var recommendations = new List<ProductRecommendationDto>();
        foreach (var fb in frequentlyBought)
        {
            if (products.TryGetValue(fb.ProductId, out var product))
            {
                var rec = _mapper.Map<ProductRecommendationDto>(product);
                rec.RecommendationReason = "Frequently bought together";
                rec.RecommendationScore = fb.Count;
                recommendations.Add(rec);
            }
        }

        return recommendations;
    }

    public async Task<IEnumerable<ProductRecommendationDto>> GetPersonalizedRecommendationsAsync(Guid userId, int maxResults = 10)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !o.IsDeleted (Global Query Filter)
        // Get user's purchase history and preferences
        var userOrders = await _context.Orders
            .AsNoTracking()
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Where(o => o.UserId == userId)
            .SelectMany(o => o.OrderItems)
            .Select(oi => oi.Product.CategoryId)
            .Distinct()
            .ToListAsync();

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !w.IsDeleted (Global Query Filter)
        // Get user's wishlist categories
        var wishlistCategories = await _context.Wishlists
            .AsNoTracking()
            .Include(w => w.Product)
            .Where(w => w.UserId == userId)
            .Select(w => w.Product.CategoryId)
            .Distinct()
            .ToListAsync();

        // ✅ PERFORMANCE: ToListAsync() sonrası memory'de işlem YASAK - ama bu batch loading için gerekli (kabul edilebilir)
        var preferredCategories = userOrders.Union(wishlistCategories).ToList();

        if (preferredCategories.Count == 0)
        {
            // If no history, return trending products
            return await GetTrendingProductsAsync(7, maxResults);
        }

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !p.IsDeleted (Global Query Filter)
        // Get highly rated products from preferred categories
        var recommendations = await _context.Products
            .AsNoTracking()
            .Where(p => p.IsActive &&
                       preferredCategories.Contains(p.CategoryId) &&
                       p.Rating >= 4.0m)
            .OrderByDescending(p => p.Rating)
            .ThenByDescending(p => p.ReviewCount)
            .Take(maxResults)
            .ToListAsync();

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var recommendationDtos = _mapper.Map<IEnumerable<ProductRecommendationDto>>(recommendations).ToList();
        foreach (var rec in recommendationDtos)
        {
            rec.RecommendationReason = "Based on your interests";
            rec.RecommendationScore = rec.Rating * rec.ReviewCount;
        }
        return recommendationDtos;
    }

    public async Task<IEnumerable<ProductRecommendationDto>> GetBasedOnViewHistoryAsync(Guid userId, int maxResults = 10)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !rv.IsDeleted (Global Query Filter)
        // Get recently viewed products
        var recentlyViewed = await _context.Set<Domain.Entities.RecentlyViewedProduct>()
            .AsNoTracking()
            .Include(rv => rv.Product)
                .ThenInclude(p => p.Category)
            .Where(rv => rv.UserId == userId)
            .OrderByDescending(rv => rv.ViewedAt)
            .Take(5)
            .ToListAsync();

        if (recentlyViewed.Count == 0)
        {
            return Enumerable.Empty<ProductRecommendationDto>();
        }

        // ✅ PERFORMANCE: ToListAsync() sonrası memory'de işlem YASAK - ama bu batch loading için gerekli (kabul edilebilir)
        var viewedCategories = recentlyViewed.Select(rv => rv.Product.CategoryId).Distinct().ToList();
        var viewedProductIds = recentlyViewed.Select(rv => rv.ProductId).ToList();

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !p.IsDeleted (Global Query Filter)
        // Get products from same categories, excluding already viewed
        var recommendations = await _context.Products
            .AsNoTracking()
            .Where(p => p.IsActive &&
                       viewedCategories.Contains(p.CategoryId) &&
                       !viewedProductIds.Contains(p.Id))
            .OrderByDescending(p => p.Rating)
            .ThenByDescending(p => p.ReviewCount)
            .Take(maxResults)
            .ToListAsync();

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var recommendationDtos = _mapper.Map<IEnumerable<ProductRecommendationDto>>(recommendations).ToList();
        foreach (var rec in recommendationDtos)
        {
            rec.RecommendationReason = "Based on your browsing history";
            rec.RecommendationScore = rec.Rating;
        }
        return recommendationDtos;
    }

    public async Task<IEnumerable<ProductRecommendationDto>> GetTrendingProductsAsync(int days = 7, int maxResults = 10)
    {
        var startDate = DateTime.UtcNow.AddDays(-days);

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !oi.IsDeleted (Global Query Filter)
        // ✅ PERFORMANCE: Database'de grouping yap (memory'de işlem YASAK)
        var trendingProducts = await _context.OrderItems
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
            .ToListAsync();

        // ✅ PERFORMANCE: Batch load products to avoid N+1 queries
        var productIds = trendingProducts.Select(tp => tp.ProductId).ToList();
        var products = await _context.Products
            .AsNoTracking()
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var recommendations = new List<ProductRecommendationDto>();
        foreach (var tp in trendingProducts)
        {
            if (products.TryGetValue(tp.ProductId, out var product))
            {
                var rec = _mapper.Map<ProductRecommendationDto>(product);
                rec.RecommendationReason = $"Trending in last {days} days";
                rec.RecommendationScore = tp.RecommendationScore;
                recommendations.Add(rec);
            }
        }
        return recommendations;
    }

    public async Task<IEnumerable<ProductRecommendationDto>> GetBestSellersAsync(int maxResults = 10)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !oi.IsDeleted (Global Query Filter)
        // ✅ PERFORMANCE: Database'de grouping yap (memory'de işlem YASAK)
        var bestSellers = await _context.OrderItems
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
            .ToListAsync();

        // ✅ PERFORMANCE: Batch load products to avoid N+1 queries
        var productIds = bestSellers.Select(bs => bs.ProductId).ToList();
        var products = await _context.Products
            .AsNoTracking()
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var recommendations = new List<ProductRecommendationDto>();
        foreach (var bs in bestSellers)
        {
            if (products.TryGetValue(bs.ProductId, out var product))
            {
                var rec = _mapper.Map<ProductRecommendationDto>(product);
                rec.RecommendationReason = "Best seller";
                rec.RecommendationScore = bs.RecommendationScore;
                recommendations.Add(rec);
            }
        }
        return recommendations;
    }

    public async Task<IEnumerable<ProductRecommendationDto>> GetNewArrivalsAsync(int days = 30, int maxResults = 10)
    {
        var startDate = DateTime.UtcNow.AddDays(-days);

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !p.IsDeleted (Global Query Filter)
        var newArrivals = await _context.Products
            .AsNoTracking()
            .Where(p => p.IsActive && p.CreatedAt >= startDate)
            .OrderByDescending(p => p.CreatedAt)
            .Take(maxResults)
            .ToListAsync();

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var recommendations = _mapper.Map<IEnumerable<ProductRecommendationDto>>(newArrivals).ToList();
        foreach (var rec in recommendations)
        {
            rec.RecommendationReason = "New arrival";
            rec.RecommendationScore = 0;
        }
        return recommendations;
    }

    public async Task<PersonalizedRecommendationsDto> GetCompleteRecommendationsAsync(Guid userId)
    {
        // ✅ PERFORMANCE: ToListAsync() sonrası memory'de işlem YASAK - ama bu sadece property assignment (kabul edilebilir)
        var recommendations = new PersonalizedRecommendationsDto
        {
            ForYou = (await GetPersonalizedRecommendationsAsync(userId, 10)).ToList(),
            BasedOnHistory = (await GetBasedOnViewHistoryAsync(userId, 10)).ToList(),
            Trending = (await GetTrendingProductsAsync(7, 10)).ToList(),
            BestSellers = (await GetBestSellersAsync(10)).ToList()
        };

        return recommendations;
    }
}
