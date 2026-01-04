using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.ML;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.ValueObjects;
using Merge.Infrastructure.Data;
using Merge.Infrastructure.Repositories;
using ProductEntity = Merge.Domain.Entities.Product;
using Merge.Application.DTOs.Analytics;


namespace Merge.Application.Services.ML;

public class PriceOptimizationService : IPriceOptimizationService
{
    private readonly ApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PriceOptimizationService> _logger;

    public PriceOptimizationService(ApplicationDbContext context, IUnitOfWork unitOfWork, ILogger<PriceOptimizationService> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<PriceOptimizationDto> OptimizePriceAsync(Guid productId, PriceOptimizationRequestDto? request = null, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !p.IsDeleted (Global Query Filter)
        var product = await _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);

        if (product == null)
        {
            throw new NotFoundException("Ürün", productId);
        }

        // ✅ PERFORMANCE: Batch load similar products (N+1 fix)
        var similarProducts = await _context.Products
            .AsNoTracking()
            .Where(p => p.CategoryId == product.CategoryId && 
                       p.Id != product.Id && 
                       p.IsActive)
            .ToListAsync(cancellationToken);

        // Basit fiyat optimizasyon algoritması
        var recommendation = await CalculateOptimalPriceAsync(product, similarProducts, cancellationToken);

        // Fiyatı güncelle (opsiyonel - sadece öneri döndürmek için kullanılabilir)
        if (request?.ApplyOptimization == true)
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullan
            var oldPrice = product.Price;
            var newPrice = new Money(recommendation.OptimalPrice);
            product.SetPrice(newPrice);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
            _logger.LogInformation(
                "Price optimized for product. ProductId: {ProductId}, OldPrice: {OldPrice}, NewPrice: {NewPrice}",
                productId, oldPrice, recommendation.OptimalPrice);
        }

        return new PriceOptimizationDto
        {
            ProductId = productId,
            ProductName = product.Name,
            CurrentPrice = product.Price,
            RecommendedPrice = recommendation.OptimalPrice,
            MinPrice = recommendation.MinPrice,
            MaxPrice = recommendation.MaxPrice,
            Confidence = recommendation.Confidence,
            ExpectedRevenueChange = recommendation.ExpectedRevenueChange,
            ExpectedSalesChange = recommendation.ExpectedSalesChange,
            Reasoning = recommendation.Reasoning,
            OptimizedAt = DateTime.UtcNow
        };
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<IEnumerable<PriceOptimizationDto>> OptimizePricesForCategoryAsync(Guid categoryId, PriceOptimizationRequestDto? request = null, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !p.IsDeleted (Global Query Filter)
        var products = await _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Where(p => p.CategoryId == categoryId && p.IsActive)
            .ToListAsync(cancellationToken);

        // ✅ PERFORMANCE: Batch load competitor prices (N+1 fix)
        // ✅ PERFORMANCE: ToListAsync() sonrası Select() ve Distinct() YASAK - Database'de Select ve Distinct yap
        var categoryIds = await _context.Products
            .AsNoTracking()
            .Where(p => p.CategoryId == categoryId && p.IsActive)
            .Select(p => p.CategoryId)
            .Distinct()
            .ToListAsync(cancellationToken);
        
        var allSimilarProducts = await _context.Products
            .AsNoTracking()
            .Where(p => categoryIds.Contains(p.CategoryId) && p.IsActive)
            .ToListAsync(cancellationToken);

        // ✅ PERFORMANCE: ToListAsync() sonrası GroupBy() ve ToDictionary() YASAK
        // Not: Bu durumda entity grouping yapılıyor, database'de ToDictionaryAsync yapılamaz
        // Ancak bu minimal bir işlem ve business logic için gerekli (ML algoritması için grouping)
        var similarProductsByCategory = allSimilarProducts
            .GroupBy(p => p.CategoryId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var results = new List<PriceOptimizationDto>();

        foreach (var product in products)
        {
            // ✅ PERFORMANCE: Memory'den similar products al (N+1 fix)
            // ✅ PERFORMANCE: ToListAsync() sonrası Where() ve ToList() YASAK
            // Not: Bu durumda `similar` zaten memory'de (dictionary'den geliyor), bu yüzden bu minimal bir işlem
            // Ancak business logic için gerekli (aynı product'ı exclude etmek için)
            var similarProducts = similarProductsByCategory.TryGetValue(product.CategoryId, out var similar) 
                ? similar.Where(p => p.Id != product.Id).ToList() 
                : new List<ProductEntity>();
            
            var recommendation = await CalculateOptimalPriceAsync(product, similarProducts, cancellationToken);
            results.Add(new PriceOptimizationDto
            {
                ProductId = product.Id,
                ProductName = product.Name,
                CurrentPrice = product.Price,
                RecommendedPrice = recommendation.OptimalPrice,
                MinPrice = recommendation.MinPrice,
                MaxPrice = recommendation.MaxPrice,
                Confidence = recommendation.Confidence,
                ExpectedRevenueChange = recommendation.ExpectedRevenueChange,
                ExpectedSalesChange = recommendation.ExpectedSalesChange,
                Reasoning = recommendation.Reasoning,
                OptimizedAt = DateTime.UtcNow
            });
        }

        // ✅ PERFORMANCE: ToListAsync() sonrası OrderByDescending() YASAK
        // Not: Bu durumda `results` zaten memory'de (List), bu yüzden bu minimal bir işlem
        // Ancak business logic için gerekli (sıralama için)
        return results.OrderByDescending(r => r.ExpectedRevenueChange);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<PriceRecommendationDto> GetPriceRecommendationAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !p.IsDeleted (Global Query Filter)
        var product = await _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);

        if (product == null)
        {
            throw new NotFoundException("Ürün", productId);
        }

        // ✅ PERFORMANCE: Batch load similar products (N+1 fix)
        var similarProducts = await _context.Products
            .AsNoTracking()
            .Where(p => p.CategoryId == product.CategoryId && 
                       p.Id != product.Id && 
                       p.IsActive)
            .ToListAsync(cancellationToken);

        return await CalculateOptimalPriceAsync(product, similarProducts, cancellationToken);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<IEnumerable<PriceRecommendationDto>> GetPriceRecommendationsAsync(Guid productId, int count = 5, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !p.IsDeleted (Global Query Filter)
        var product = await _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);

        if (product == null)
        {
            throw new NotFoundException("Ürün", productId);
        }

        // ✅ PERFORMANCE: Batch load similar products (N+1 fix)
        var similarProducts = await _context.Products
            .AsNoTracking()
            .Where(p => p.CategoryId == product.CategoryId && 
                       p.Id != product.Id && 
                       p.IsActive)
            .ToListAsync(cancellationToken);

        var recommendation = await CalculateOptimalPriceAsync(product, similarProducts, cancellationToken);
        return new[] { recommendation };
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<PriceOptimizationStatsDto> GetOptimizationStatsAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        var start = startDate ?? DateTime.UtcNow.AddDays(-30);
        var end = endDate ?? DateTime.UtcNow;

        // Bu basit implementasyonda, gerçek optimizasyon istatistikleri tutulmuyor
        // Gerçek implementasyonda bir PriceOptimizationHistory tablosu olmalı

        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        var totalProducts = await _context.Products
            .CountAsync(p => p.IsActive, cancellationToken);

        var productsWithDiscount = await _context.Products
            .CountAsync(p => p.IsActive && p.DiscountPrice.HasValue, cancellationToken);

        return new PriceOptimizationStatsDto
        {
            TotalOptimizations = productsWithDiscount,
            AverageRevenueIncrease = 0, // Gerçek implementasyonda hesaplanmalı
            ProductsOptimized = productsWithDiscount,
            PeriodStart = start,
            PeriodEnd = end
        };
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    private async Task<PriceRecommendationDto> CalculateOptimalPriceAsync(ProductEntity product, List<ProductEntity>? similarProducts = null, CancellationToken cancellationToken = default)
    {
        // Basit fiyat optimizasyon algoritması
        // Gerçek implementasyonda ML modeli kullanılabilir

        var currentPrice = product.DiscountPrice ?? product.Price;
        var basePrice = product.Price;

        // ✅ PERFORMANCE: Similar products parametre olarak geliyor (N+1 fix)
        if (similarProducts == null)
        {
            // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
            similarProducts = await _context.Products
                .AsNoTracking()
                .Where(p => p.CategoryId == product.CategoryId && 
                           p.Id != product.Id && 
                           p.IsActive)
                .ToListAsync(cancellationToken);
        }

        // ✅ PERFORMANCE: Memory'de minimal işlem (business logic için gerekli)
        var competitorPrices = similarProducts
            .Select(p => p.DiscountPrice ?? p.Price)
            .Where(p => p > 0)
            .ToList();

        var avgCompetitorPrice = competitorPrices.Any() ? competitorPrices.Average() : currentPrice;
        var minCompetitorPrice = competitorPrices.Any() ? competitorPrices.Min() : currentPrice;
        var maxCompetitorPrice = competitorPrices.Any() ? competitorPrices.Max() : currentPrice;

        // Stok durumuna göre fiyatlandırma
        var stockFactor = product.StockQuantity switch
        {
            <= 0 => 1.0m, // Stok yoksa fiyatı artır
            <= 10 => 0.95m, // Düşük stok, biraz indirim
            _ => 0.90m // Yüksek stok, daha fazla indirim
        };

        // Rating'e göre fiyatlandırma
        var ratingFactor = product.Rating switch
        {
            >= 4.5m => 1.05m, // Yüksek rating, premium fiyat
            >= 4.0m => 1.0m,
            >= 3.5m => 0.95m,
            _ => 0.90m // Düşük rating, indirim
        };

        // Satış hacmine göre fiyatlandırma
        var salesFactor = product.ReviewCount switch
        {
            >= 100 => 1.02m, // Popüler ürün, biraz artır
            >= 50 => 1.0m,
            _ => 0.98m // Yeni ürün, biraz indirim
        };

        // Optimal fiyat hesaplama
        var optimalPrice = avgCompetitorPrice * stockFactor * ratingFactor * salesFactor;
        
        // Min ve Max fiyat aralığı
        var minPrice = Math.Max(basePrice * 0.7m, minCompetitorPrice * 0.9m);
        var maxPrice = Math.Min(basePrice * 1.3m, maxCompetitorPrice * 1.1m);
        
        optimalPrice = Math.Max(minPrice, Math.Min(maxPrice, optimalPrice));

        // Beklenen değişiklikler
        var priceChange = optimalPrice - currentPrice;
        var priceChangePercent = currentPrice > 0 ? (priceChange / currentPrice) * 100 : 0;
        
        var expectedSalesChange = priceChangePercent switch
        {
            < -10 => 15, // %10+ indirim -> %15 satış artışı
            < -5 => 10,
            < 0 => 5,
            < 5 => -5,
            < 10 => -10,
            _ => -15 // %10+ artış -> %15 satış düşüşü
        };

        var expectedRevenueChange = (priceChangePercent + expectedSalesChange) / 2; // Basit hesaplama

        var confidence = CalculateConfidence(product, competitorPrices.Count);

        var reasoning = $"Based on competitor analysis ({competitorPrices.Count} similar products), " +
                       $"stock level ({product.StockQuantity} units), " +
                       $"rating ({product.Rating:F1}), and sales volume ({product.ReviewCount} reviews). " +
                       $"Optimal price calculated: {optimalPrice:C} (current: {currentPrice:C}).";

        return new PriceRecommendationDto
        {
            OptimalPrice = Math.Round(optimalPrice, 2),
            MinPrice = Math.Round(minPrice, 2),
            MaxPrice = Math.Round(maxPrice, 2),
            Confidence = confidence,
            ExpectedRevenueChange = Math.Round(expectedRevenueChange, 2),
            ExpectedSalesChange = expectedSalesChange,
            Reasoning = reasoning
        };
    }

    private decimal CalculateConfidence(ProductEntity product, int competitorCount)
    {
        var confidence = 50m; // Base confidence

        // Rakipler varsa confidence artar
        if (competitorCount > 0) confidence += 20;
        if (competitorCount > 5) confidence += 10;

        // Rating varsa confidence artar
        if (product.Rating > 0) confidence += 10;
        if (product.ReviewCount > 10) confidence += 10;

        return Math.Min(confidence, 100);
    }
}


