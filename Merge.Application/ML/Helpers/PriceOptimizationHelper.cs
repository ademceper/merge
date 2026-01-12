using Microsoft.Extensions.Options;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using Merge.Application.DTOs.Analytics;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.SharedKernel;
using Merge.Domain.ValueObjects;

namespace Merge.Application.ML.Helpers;

// ✅ BOLUM 1.1: Clean Architecture - Helper class for shared price optimization logic
public class PriceOptimizationHelper
{
    private readonly MLSettings _mlSettings;

    public PriceOptimizationHelper(IOptions<MLSettings> mlSettings)
    {
        _mlSettings = mlSettings.Value;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public Task<PriceRecommendationDto> CalculateOptimalPriceAsync(ProductEntity product, List<ProductEntity>? similarProducts = null, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 1.6: Invariant validation - Guard clauses
        Guard.AgainstNull(product, nameof(product));

        // Basit fiyat optimizasyon algoritması
        // Gerçek implementasyonda ML modeli kullanılabilir

        var currentPrice = product.DiscountPrice ?? product.Price;
        var basePrice = product.Price;

        // ✅ PERFORMANCE: Memory'de minimal işlem (business logic için gerekli)
        var competitorPrices = similarProducts?
            .Select(p => p.DiscountPrice ?? p.Price)
            .Where(p => p > 0)
            .ToList() ?? new List<decimal>();

        var avgCompetitorPrice = competitorPrices.Any() ? competitorPrices.Average() : currentPrice;
        var minCompetitorPrice = competitorPrices.Any() ? competitorPrices.Min() : currentPrice;
        var maxCompetitorPrice = competitorPrices.Any() ? competitorPrices.Max() : currentPrice;

        // Stok durumuna göre fiyatlandırma
        // ✅ BOLUM 12.0: Configuration - Magic number'lar configuration'dan alınıyor
        // ✅ FIX: Switch expression'da constant değer bekleniyor, if-else kullanıyoruz
        decimal stockFactor;
        if (product.StockQuantity <= 0)
            stockFactor = _mlSettings.PriceOptimizationStockFactorZero;
        else if (product.StockQuantity <= _mlSettings.StockThresholdLow)
            stockFactor = _mlSettings.PriceOptimizationStockFactorLow;
        else
            stockFactor = _mlSettings.PriceOptimizationStockFactorHigh;

        // Rating'e göre fiyatlandırma
        // ✅ BOLUM 12.0: Configuration - Magic number'lar configuration'dan alınıyor
        // ✅ FIX: Switch expression'da constant değer bekleniyor, if-else kullanıyoruz
        decimal ratingFactor;
        if (product.Rating >= _mlSettings.RatingThresholdHigh)
            ratingFactor = _mlSettings.PriceOptimizationRatingFactorHigh;
        else if (product.Rating >= _mlSettings.RatingThresholdMediumHigh)
            ratingFactor = _mlSettings.PriceOptimizationRatingFactorMediumHigh;
        else if (product.Rating >= _mlSettings.RatingThresholdMedium)
            ratingFactor = _mlSettings.PriceOptimizationRatingFactorMedium;
        else
            ratingFactor = _mlSettings.PriceOptimizationRatingFactorLow;

        // Satış hacmine göre fiyatlandırma
        // ✅ BOLUM 12.0: Configuration - Magic number'lar configuration'dan alınıyor
        // ✅ FIX: Switch expression'da constant değer bekleniyor, if-else kullanıyoruz
        decimal salesFactor;
        if (product.ReviewCount >= _mlSettings.ReviewCountThresholdHigh)
            salesFactor = _mlSettings.PriceOptimizationSalesFactorHigh;
        else if (product.ReviewCount >= _mlSettings.ReviewCountThresholdMedium)
            salesFactor = _mlSettings.PriceOptimizationSalesFactorMedium;
        else
            salesFactor = _mlSettings.PriceOptimizationSalesFactorLow;

        // Optimal fiyat hesaplama
        var optimalPrice = avgCompetitorPrice * stockFactor * ratingFactor * salesFactor;
        
        // Min ve Max fiyat aralığı
        // ✅ BOLUM 12.0: Configuration - Magic number'lar configuration'dan alınıyor
        var minPrice = Math.Max(basePrice * _mlSettings.PriceOptimizationBasePriceMinFactor, minCompetitorPrice * _mlSettings.PriceOptimizationCompetitorPriceMinFactor);
        var maxPrice = Math.Min(basePrice * _mlSettings.PriceOptimizationBasePriceMaxFactor, maxCompetitorPrice * _mlSettings.PriceOptimizationCompetitorPriceMaxFactor);
        
        optimalPrice = Math.Max(minPrice, Math.Min(maxPrice, optimalPrice));

        // Beklenen değişiklikler
        var priceChange = optimalPrice - currentPrice;
        var priceChangePercent = currentPrice > 0 ? (priceChange / currentPrice) * 100 : 0;
        
        // ✅ BOLUM 12.0: Configuration - Magic number'lar configuration'dan alınıyor
        var expectedSalesChange = priceChangePercent switch
        {
            < -10 => _mlSettings.PriceOptimizationSalesChangeDiscountHigh,
            < -5 => _mlSettings.PriceOptimizationSalesChangeDiscountMedium,
            < 0 => _mlSettings.PriceOptimizationSalesChangeDiscountLow,
            < 5 => _mlSettings.PriceOptimizationSalesChangeIncreaseLow,
            < 10 => _mlSettings.PriceOptimizationSalesChangeIncreaseMedium,
            _ => _mlSettings.PriceOptimizationSalesChangeIncreaseHigh
        };

        var expectedRevenueChange = (priceChangePercent + expectedSalesChange) / 2; // Basit hesaplama

        var confidence = CalculateConfidence(product, competitorPrices.Count);

        var reasoning = $"Based on competitor analysis ({competitorPrices.Count} similar products), " +
                       $"stock level ({product.StockQuantity} units), " +
                       $"rating ({product.Rating:F1}), and sales volume ({product.ReviewCount} reviews). " +
                       $"Optimal price calculated: {optimalPrice:C} (current: {currentPrice:C}).";

        return Task.FromResult(new PriceRecommendationDto(
            Math.Round(optimalPrice, 2),
            Math.Round(minPrice, 2),
            Math.Round(maxPrice, 2),
            confidence,
            Math.Round(expectedRevenueChange, 2),
            expectedSalesChange,
            reasoning
        ));
    }

    private decimal CalculateConfidence(ProductEntity product, int competitorCount)
    {
        // ✅ BOLUM 12.0: Configuration - Magic number'lar configuration'dan alınıyor
        var confidence = _mlSettings.PriceOptimizationBaseConfidence;
        if (competitorCount > _mlSettings.PriceOptimizationCompetitorCountThreshold1) confidence += _mlSettings.PriceOptimizationCompetitorCountIncrease1;
        if (competitorCount > _mlSettings.PriceOptimizationCompetitorCountThreshold2) confidence += _mlSettings.PriceOptimizationCompetitorCountIncrease2;

        // Rating varsa confidence artar
        // ✅ BOLUM 12.0: Configuration - Magic number'lar configuration'dan alınıyor
        if (product.Rating > 0) confidence += _mlSettings.PriceOptimizationBaseRatingConfidence;
        if (product.ReviewCount > _mlSettings.HighConfidenceMinReviews) confidence += _mlSettings.PriceOptimizationReviewCountConfidence;

        return Math.Min(confidence, _mlSettings.PriceOptimizationMaxConfidence);
    }
}
