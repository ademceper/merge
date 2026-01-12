using Microsoft.Extensions.Options;
using Merge.Application.Configuration;
using Merge.Application.DTOs.Analytics;
using Merge.Domain.Entities;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.SharedKernel;
using Merge.Domain.ValueObjects;

namespace Merge.Application.ML.Helpers;

// ✅ BOLUM 1.1: Clean Architecture - Helper class for shared demand forecasting logic
public class DemandForecastCalculation
{
    public int ForecastedQuantity { get; set; }
    public int MinQuantity { get; set; }
    public int MaxQuantity { get; set; }
    public decimal Confidence { get; set; }
    public List<DailyForecastItem> DailyForecast { get; set; } = new();
    public string Reasoning { get; set; } = string.Empty;
}

// ✅ BOLUM 1.1: Clean Architecture - Helper class for shared demand forecasting logic
public class DemandForecastingHelper
{
    private readonly MLSettings _mlSettings;

    public DemandForecastingHelper(IOptions<MLSettings> mlSettings)
    {
        _mlSettings = mlSettings.Value;
    }

    public DemandForecastCalculation CalculateDemandForecast(ProductEntity product, List<object> historicalSales, int forecastDays)
    {
        // ✅ BOLUM 1.6: Invariant validation - Guard clauses
        Guard.AgainstNull(product, nameof(product));
        Guard.AgainstNull(historicalSales, nameof(historicalSales));
        Guard.AgainstNegativeOrZero(forecastDays, nameof(forecastDays));

        // Basit talep tahmin algoritması
        // Gerçek implementasyonda zaman serisi analizi (ARIMA, LSTM, vb.) kullanılabilir

        if (!historicalSales.Any())
        {
            // Satış geçmişi yoksa, kategori ortalamasına göre tahmin
            // ✅ BOLUM 12.0: Configuration - Magic number'lar configuration'dan alınıyor
            return new DemandForecastCalculation
            {
                ForecastedQuantity = _mlSettings.DemandForecastDefaultQuantity,
                MinQuantity = _mlSettings.DemandForecastMinQuantity,
                MaxQuantity = _mlSettings.DemandForecastMaxQuantity,
                Confidence = _mlSettings.DemandForecastDefaultConfidence,
                // ✅ PERFORMANCE: Enumerable.Range - Business logic için gerekli (DTO oluşturma)
                DailyForecast = Enumerable.Range(0, forecastDays)
                    .Select(i => new DailyForecastItem(
                        DateTime.UtcNow.AddDays(i).Date,
                        _mlSettings.DemandForecastDailyDefaultQuantity
                    ))
                    .ToList(),
                Reasoning = "No historical sales data available. Using default forecast."
            };
        }

        // ✅ PERFORMANCE: Memory'de minimal işlem (business logic için gerekli)
        // Ortalama günlük satış
        var totalQuantity = historicalSales.Sum(s => (int)((dynamic)s).Quantity);
        var daysWithSales = historicalSales.Count;
        var avgDailySales = daysWithSales > 0 ? (decimal)totalQuantity / daysWithSales : 0;

        // Trend analizi (basit)
        // ✅ PERFORMANCE: TakeLast, Skip, Take - List üzerinde işlem (business logic için gerekli)
        // ✅ BOLUM 12.0: Configuration - Magic number'lar configuration'dan alınıyor
        var recentSales = historicalSales.TakeLast(_mlSettings.DemandForecastRecentSalesDays).ToList();
        var olderSales = historicalSales.Skip(Math.Max(0, historicalSales.Count - _mlSettings.DemandForecastOlderSalesDays)).Take(_mlSettings.DemandForecastRecentSalesDays).ToList();

        var recentAvg = recentSales.Any() ? recentSales.Average(s => (decimal)((dynamic)s).Quantity) : 0;
        var olderAvg = olderSales.Any() ? olderSales.Average(s => (decimal)((dynamic)s).Quantity) : 0;

        var trend = recentAvg > 0 && olderAvg > 0 ? (recentAvg - olderAvg) / olderAvg : 0;

        // Mevsimsellik faktörü (basit - hafta içi/hafta sonu)
        // ✅ BOLUM 12.0: Configuration - Magic number'lar configuration'dan alınıyor
        var dayOfWeekFactor = DateTime.UtcNow.DayOfWeek switch
        {
            DayOfWeek.Saturday or DayOfWeek.Sunday => _mlSettings.DemandForecastWeekendFactor,
            _ => _mlSettings.WeekdayFactor
        };

        // Stok durumu faktörü
        // ✅ BOLUM 12.0: Configuration - Magic number'lar configuration'dan alınıyor
        // ✅ FIX: Switch expression'da constant değer bekleniyor, if-else kullanıyoruz
        decimal stockFactor;
        if (product.StockQuantity <= 0)
            stockFactor = _mlSettings.DemandForecastStockFactorZero;
        else if (product.StockQuantity <= _mlSettings.StockThresholdLow)
            stockFactor = _mlSettings.DemandForecastStockFactorLow;
        else
            stockFactor = _mlSettings.DemandForecastStockFactorHigh;

        // Rating faktörü
        // ✅ BOLUM 12.0: Configuration - Magic number'lar configuration'dan alınıyor
        // ✅ FIX: Switch expression'da constant değer bekleniyor, if-else kullanıyoruz
        decimal ratingFactor;
        if (product.Rating >= _mlSettings.RatingThresholdHigh)
            ratingFactor = _mlSettings.DemandForecastRatingFactorHigh;
        else if (product.Rating >= _mlSettings.RatingThresholdMediumHigh)
            ratingFactor = _mlSettings.DemandForecastRatingFactorMediumHigh;
        else if (product.Rating >= _mlSettings.RatingThresholdMedium)
            ratingFactor = _mlSettings.DemandForecastRatingFactorMedium;
        else
            ratingFactor = _mlSettings.DemandForecastRatingFactorLow;

        // Tahmin hesaplama
        var baseForecast = avgDailySales * (1 + trend) * dayOfWeekFactor * stockFactor * ratingFactor;
        var forecastedQuantity = (int)Math.Ceiling(baseForecast * forecastDays);

        // Min/Max aralığı
        // ✅ BOLUM 12.0: Configuration - Magic number'lar configuration'dan alınıyor
        var minQuantity = (int)Math.Floor(forecastedQuantity * _mlSettings.DemandForecastMinFactor);
        var maxQuantity = (int)Math.Ceiling(forecastedQuantity * _mlSettings.DemandForecastMaxFactor);

        // Confidence hesaplama
        var confidence = CalculateForecastConfidence(historicalSales.Count, daysWithSales, trend);

        // ✅ PERFORMANCE: Enumerable.Range - Business logic için gerekli (DTO oluşturma)
        // Günlük tahmin
        var dailyForecast = Enumerable.Range(0, forecastDays)
            .Select(i =>
            {
                var date = DateTime.UtcNow.AddDays(i);
                // ✅ BOLUM 12.0: Configuration - Magic number'lar configuration'dan alınıyor
                var dayFactor = date.DayOfWeek switch
                {
                    DayOfWeek.Saturday or DayOfWeek.Sunday => _mlSettings.DemandForecastWeekendFactor,
                    _ => _mlSettings.WeekdayFactor
                };
                return new DailyForecastItem(
                    date.Date,
                    (int)Math.Ceiling(baseForecast * dayFactor)
                );
            })
            .ToList();

        var reasoning = $"Based on {daysWithSales} days of historical sales data (avg: {avgDailySales:F1} units/day), " +
                       $"trend: {(trend * 100):F1}%, stock: {product.StockQuantity} units, rating: {product.Rating:F1}. " +
                       $"Forecasted demand: {forecastedQuantity} units over {forecastDays} days.";

        return new DemandForecastCalculation
        {
            ForecastedQuantity = forecastedQuantity,
            MinQuantity = minQuantity,
            MaxQuantity = maxQuantity,
            Confidence = confidence,
            DailyForecast = dailyForecast,
            Reasoning = reasoning
        };
    }

    private decimal CalculateForecastConfidence(int totalDays, int daysWithSales, decimal trend)
    {
        // ✅ BOLUM 12.0: Configuration - Magic number'lar configuration'dan alınıyor
        var confidence = _mlSettings.DemandForecastBaseConfidence;
        if (totalDays > _mlSettings.DemandForecastConfidenceThreshold1) confidence += _mlSettings.DemandForecastConfidenceIncrease1;
        if (totalDays > _mlSettings.DemandForecastConfidenceThreshold2) confidence += _mlSettings.DemandForecastConfidenceIncrease2;

        // Satış sıklığı
        var salesFrequency = totalDays > 0 ? (decimal)daysWithSales / totalDays : 0;
        if (salesFrequency > _mlSettings.DemandForecastSalesFrequencyThreshold1) confidence += _mlSettings.DemandForecastSalesFrequencyIncrease1;
        if (salesFrequency > _mlSettings.DemandForecastSalesFrequencyThreshold2) confidence += _mlSettings.DemandForecastSalesFrequencyIncrease2;

        // Trend stabilitesi
        if (Math.Abs(trend) < _mlSettings.DemandForecastTrendStabilityThreshold) confidence += _mlSettings.DemandForecastTrendStabilityIncrease;

        // ✅ BOLUM 12.0: Configuration - Magic number'lar configuration'dan alınıyor
        return Math.Min(confidence, _mlSettings.MaxRiskScore);
    }
}
