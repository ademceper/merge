using Microsoft.Extensions.Options;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using Merge.Application.DTOs.Analytics;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.SharedKernel;
using Merge.Domain.ValueObjects;

namespace Merge.Application.ML.Helpers;

public class PriceOptimizationHelper(IOptions<MLSettings> mlSettings)
{
    private readonly MLSettings config = mlSettings.Value;

    public Task<PriceRecommendationDto> CalculateOptimalPriceAsync(ProductEntity product, List<ProductEntity>? similarProducts = null, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(product, nameof(product));

        // Basit fiyat optimizasyon algoritması
        // Gerçek implementasyonda ML modeli kullanılabilir

        var currentPrice = product.DiscountPrice ?? product.Price;
        var basePrice = product.Price;

        var competitorPrices = similarProducts?
            .Select(p => p.DiscountPrice ?? p.Price)
            .Where(p => p > 0)
            .ToList() ?? new List<decimal>();

        var avgCompetitorPrice = competitorPrices.Any() ? competitorPrices.Average() : currentPrice;
        var minCompetitorPrice = competitorPrices.Any() ? competitorPrices.Min() : currentPrice;
        var maxCompetitorPrice = competitorPrices.Any() ? competitorPrices.Max() : currentPrice;

        // Stok durumuna göre fiyatlandırma
        decimal stockFactor;
        if (product.StockQuantity <= 0)
            stockFactor = config.PriceOptimizationStockFactorZero;
        else if (product.StockQuantity <= config.StockThresholdLow)
            stockFactor = config.PriceOptimizationStockFactorLow;
        else
            stockFactor = config.PriceOptimizationStockFactorHigh;

        // Rating'e göre fiyatlandırma
        decimal ratingFactor;
        if (product.Rating >= config.RatingThresholdHigh)
            ratingFactor = config.PriceOptimizationRatingFactorHigh;
        else if (product.Rating >= config.RatingThresholdMediumHigh)
            ratingFactor = config.PriceOptimizationRatingFactorMediumHigh;
        else if (product.Rating >= config.RatingThresholdMedium)
            ratingFactor = config.PriceOptimizationRatingFactorMedium;
        else
            ratingFactor = config.PriceOptimizationRatingFactorLow;

        // Satış hacmine göre fiyatlandırma
        decimal salesFactor;
        if (product.ReviewCount >= config.ReviewCountThresholdHigh)
            salesFactor = config.PriceOptimizationSalesFactorHigh;
        else if (product.ReviewCount >= config.ReviewCountThresholdMedium)
            salesFactor = config.PriceOptimizationSalesFactorMedium;
        else
            salesFactor = config.PriceOptimizationSalesFactorLow;

        // Optimal fiyat hesaplama
        var optimalPrice = avgCompetitorPrice * stockFactor * ratingFactor * salesFactor;
        
        // Min ve Max fiyat aralığı
        var minPrice = Math.Max(basePrice * config.PriceOptimizationBasePriceMinFactor, minCompetitorPrice * config.PriceOptimizationCompetitorPriceMinFactor);
        var maxPrice = Math.Min(basePrice * config.PriceOptimizationBasePriceMaxFactor, maxCompetitorPrice * config.PriceOptimizationCompetitorPriceMaxFactor);
        
        optimalPrice = Math.Max(minPrice, Math.Min(maxPrice, optimalPrice));

        // Beklenen değişiklikler
        var priceChange = optimalPrice - currentPrice;
        var priceChangePercent = currentPrice > 0 ? (priceChange / currentPrice) * 100 : 0;
        
        var expectedSalesChange = priceChangePercent switch
        {
            < -10 => config.PriceOptimizationSalesChangeDiscountHigh,
            < -5 => config.PriceOptimizationSalesChangeDiscountMedium,
            < 0 => config.PriceOptimizationSalesChangeDiscountLow,
            < 5 => config.PriceOptimizationSalesChangeIncreaseLow,
            < 10 => config.PriceOptimizationSalesChangeIncreaseMedium,
            _ => config.PriceOptimizationSalesChangeIncreaseHigh
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
        var confidence = config.PriceOptimizationBaseConfidence;
        if (competitorCount > config.PriceOptimizationCompetitorCountThreshold1) confidence += config.PriceOptimizationCompetitorCountIncrease1;
        if (competitorCount > config.PriceOptimizationCompetitorCountThreshold2) confidence += config.PriceOptimizationCompetitorCountIncrease2;

        // Rating varsa confidence artar
        if (product.Rating > 0) confidence += config.PriceOptimizationBaseRatingConfidence;
        if (product.ReviewCount > config.HighConfidenceMinReviews) confidence += config.PriceOptimizationReviewCountConfidence;

        return Math.Min(confidence, config.PriceOptimizationMaxConfidence);
    }
}
