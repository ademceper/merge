using Merge.Domain.Modules.Catalog;
using Merge.Domain.ValueObjects;
namespace Merge.Application.Configuration;

/// <summary>
/// ML (Machine Learning) islemleri icin configuration ayarlari
/// </summary>
public class MLSettings
{
    public const string SectionName = "MLSettings";

    /// <summary>
    /// Varsayilan analiz periyodu (gun)
    /// </summary>
    public int DefaultAnalysisPeriodDays { get; set; } = 30;

    /// <summary>
    /// Fiyat optimizasyonu minimum veri sayisi
    /// </summary>
    public int PriceOptimizationMinDataPoints { get; set; } = 100;

    /// <summary>
    /// Talep tahmini egitim periyodu (gun)
    /// </summary>
    public int DemandForecastTrainingDays { get; set; } = 90;

    /// <summary>
    /// Talep tahmini maksimum gün sayısı
    /// </summary>
    public int MaxForecastDays { get; set; } = 365;

    /// <summary>
    /// Talep tahmini minimum gün sayısı
    /// </summary>
    public int MinForecastDays { get; set; } = 1;

    /// <summary>
    /// Varsayılan talep tahmini gün sayısı
    /// </summary>
    public int DefaultForecastDays { get; set; } = 30;

    /// <summary>
    /// Pagination maksimum sayfa boyutu
    /// </summary>
    public int MaxPageSize { get; set; } = 100;

    /// <summary>
    /// Fraud detection risk score maksimum değeri
    /// </summary>
    public int MaxRiskScore { get; set; } = 100;

    /// <summary>
    /// Fraud detection yeni hesap kontrolü için gün sayısı
    /// </summary>
    public int NewAccountCheckDays { get; set; } = 7;

    /// <summary>
    /// Talep tahmini confidence hesaplama için minimum gün sayısı (yüksek confidence)
    /// </summary>
    public int HighConfidenceMinDays { get; set; } = 30;

    /// <summary>
    /// Talep tahmini confidence hesaplama için minimum gün sayısı (çok yüksek confidence)
    /// </summary>
    public int VeryHighConfidenceMinDays { get; set; } = 90;

    /// <summary>
    /// Talep tahmini için son günler (trend analizi)
    /// </summary>
    public int RecentDaysForTrend { get; set; } = 7;

    /// <summary>
    /// Talep tahmini için eski günler (trend analizi)
    /// </summary>
    public int OlderDaysForTrend { get; set; } = 14;

    /// <summary>
    /// Fiyat optimizasyonu için minimum rakip sayısı (yüksek confidence)
    /// </summary>
    public int HighConfidenceMinCompetitors { get; set; } = 5;

    /// <summary>
    /// Fiyat optimizasyonu için minimum review sayısı (yüksek confidence)
    /// </summary>
    public int HighConfidenceMinReviews { get; set; } = 10;

    // Demand Forecasting Settings
    /// <summary>
    /// Talep tahmini için varsayılan miktar (satış geçmişi yoksa)
    /// </summary>
    public int DemandForecastDefaultQuantity { get; set; } = 10;

    /// <summary>
    /// Talep tahmini için minimum miktar
    /// </summary>
    public int DemandForecastMinQuantity { get; set; } = 5;

    /// <summary>
    /// Talep tahmini için maksimum miktar
    /// </summary>
    public int DemandForecastMaxQuantity { get; set; } = 20;

    /// <summary>
    /// Talep tahmini için varsayılan confidence
    /// </summary>
    public decimal DemandForecastDefaultConfidence { get; set; } = 30;

    /// <summary>
    /// Talep tahmini için günlük varsayılan miktar
    /// </summary>
    public int DemandForecastDailyDefaultQuantity { get; set; } = 1;

    /// <summary>
    /// Talep tahmini için son satış günleri (trend analizi)
    /// </summary>
    public int DemandForecastRecentSalesDays { get; set; } = 7;

    /// <summary>
    /// Talep tahmini için eski satış günleri (trend analizi)
    /// </summary>
    public int DemandForecastOlderSalesDays { get; set; } = 14;

    /// <summary>
    /// Talep tahmini için minimum faktör (min quantity hesaplama)
    /// </summary>
    public decimal DemandForecastMinFactor { get; set; } = 0.7m;

    /// <summary>
    /// Talep tahmini için maksimum faktör (max quantity hesaplama)
    /// </summary>
    public decimal DemandForecastMaxFactor { get; set; } = 1.3m;

    /// <summary>
    /// Talep tahmini için base confidence
    /// </summary>
    public decimal DemandForecastBaseConfidence { get; set; } = 50;

    /// <summary>
    /// Talep tahmini için confidence threshold 1
    /// </summary>
    public int DemandForecastConfidenceThreshold1 { get; set; } = 30;

    /// <summary>
    /// Talep tahmini için confidence threshold 2
    /// </summary>
    public int DemandForecastConfidenceThreshold2 { get; set; } = 90;

    /// <summary>
    /// Talep tahmini için confidence increase 1
    /// </summary>
    public decimal DemandForecastConfidenceIncrease1 { get; set; } = 20;

    /// <summary>
    /// Talep tahmini için confidence increase 2
    /// </summary>
    public decimal DemandForecastConfidenceIncrease2 { get; set; } = 10;

    /// <summary>
    /// Talep tahmini için satış sıklığı threshold 1
    /// </summary>
    public decimal DemandForecastSalesFrequencyThreshold1 { get; set; } = 0.5m;

    /// <summary>
    /// Talep tahmini için satış sıklığı threshold 2
    /// </summary>
    public decimal DemandForecastSalesFrequencyThreshold2 { get; set; } = 0.8m;

    /// <summary>
    /// Talep tahmini için satış sıklığı increase 1
    /// </summary>
    public decimal DemandForecastSalesFrequencyIncrease1 { get; set; } = 10;

    /// <summary>
    /// Talep tahmini için satış sıklığı increase 2
    /// </summary>
    public decimal DemandForecastSalesFrequencyIncrease2 { get; set; } = 10;

    /// <summary>
    /// Talep tahmini için trend stabilite threshold
    /// </summary>
    public decimal DemandForecastTrendStabilityThreshold { get; set; } = 0.1m;

    /// <summary>
    /// Talep tahmini için trend stabilite increase
    /// </summary>
    public decimal DemandForecastTrendStabilityIncrease { get; set; } = 10;

    /// <summary>
    /// Talep tahmini maksimum gün sayısı (unbounded query koruması)
    /// </summary>
    public int DemandForecastMaxDays { get; set; } = 365;

    /// <summary>
    /// Talep tahmini minimum gün sayısı (unbounded query koruması)
    /// </summary>
    public int DemandForecastMinDays { get; set; } = 1;

    // Price Optimization Settings
    /// <summary>
    /// Fiyat optimizasyonu için stok faktörü (stok = 0)
    /// </summary>
    public decimal PriceOptimizationStockFactorZero { get; set; } = 1.0m;

    /// <summary>
    /// Fiyat optimizasyonu için stok faktörü (stok <= 10)
    /// </summary>
    public decimal PriceOptimizationStockFactorLow { get; set; } = 0.95m;

    /// <summary>
    /// Fiyat optimizasyonu için stok faktörü (stok > 10)
    /// </summary>
    public decimal PriceOptimizationStockFactorHigh { get; set; } = 0.90m;

    /// <summary>
    /// Fiyat optimizasyonu için rating faktörü (rating >= 4.5)
    /// </summary>
    public decimal PriceOptimizationRatingFactorHigh { get; set; } = 1.05m;

    /// <summary>
    /// Fiyat optimizasyonu için rating faktörü (rating >= 4.0)
    /// </summary>
    public decimal PriceOptimizationRatingFactorMediumHigh { get; set; } = 1.0m;

    /// <summary>
    /// Fiyat optimizasyonu için rating faktörü (rating >= 3.5)
    /// </summary>
    public decimal PriceOptimizationRatingFactorMedium { get; set; } = 0.95m;

    /// <summary>
    /// Fiyat optimizasyonu için rating faktörü (rating < 3.5)
    /// </summary>
    public decimal PriceOptimizationRatingFactorLow { get; set; } = 0.90m;

    /// <summary>
    /// Fiyat optimizasyonu için satış faktörü (review >= 100)
    /// </summary>
    public decimal PriceOptimizationSalesFactorHigh { get; set; } = 1.02m;

    /// <summary>
    /// Fiyat optimizasyonu için satış faktörü (review >= 50)
    /// </summary>
    public decimal PriceOptimizationSalesFactorMedium { get; set; } = 1.0m;

    /// <summary>
    /// Fiyat optimizasyonu için satış faktörü (review < 50)
    /// </summary>
    public decimal PriceOptimizationSalesFactorLow { get; set; } = 0.98m;

    /// <summary>
    /// Fiyat optimizasyonu için base price minimum faktör
    /// </summary>
    public decimal PriceOptimizationBasePriceMinFactor { get; set; } = 0.7m;

    /// <summary>
    /// Fiyat optimizasyonu için base price maksimum faktör
    /// </summary>
    public decimal PriceOptimizationBasePriceMaxFactor { get; set; } = 1.3m;

    /// <summary>
    /// Fiyat optimizasyonu için competitor price minimum faktör
    /// </summary>
    public decimal PriceOptimizationCompetitorPriceMinFactor { get; set; } = 0.9m;

    /// <summary>
    /// Fiyat optimizasyonu için competitor price maksimum faktör
    /// </summary>
    public decimal PriceOptimizationCompetitorPriceMaxFactor { get; set; } = 1.1m;

    /// <summary>
    /// Fiyat optimizasyonu için satış değişimi (indirim >= %10)
    /// </summary>
    public int PriceOptimizationSalesChangeDiscountHigh { get; set; } = 15;

    /// <summary>
    /// Fiyat optimizasyonu için satış değişimi (indirim >= %5)
    /// </summary>
    public int PriceOptimizationSalesChangeDiscountMedium { get; set; } = 10;

    /// <summary>
    /// Fiyat optimizasyonu için satış değişimi (indirim > 0)
    /// </summary>
    public int PriceOptimizationSalesChangeDiscountLow { get; set; } = 5;

    /// <summary>
    /// Fiyat optimizasyonu için satış değişimi (artış < %5)
    /// </summary>
    public int PriceOptimizationSalesChangeIncreaseLow { get; set; } = -5;

    /// <summary>
    /// Fiyat optimizasyonu için satış değişimi (artış < %10)
    /// </summary>
    public int PriceOptimizationSalesChangeIncreaseMedium { get; set; } = -10;

    /// <summary>
    /// Fiyat optimizasyonu için satış değişimi (artış >= %10)
    /// </summary>
    public int PriceOptimizationSalesChangeIncreaseHigh { get; set; } = -15;

    /// <summary>
    /// Fiyat optimizasyonu için base confidence
    /// </summary>
    public decimal PriceOptimizationBaseConfidence { get; set; } = 50;

    /// <summary>
    /// Fiyat optimizasyonu için competitor count threshold 1
    /// </summary>
    public int PriceOptimizationCompetitorCountThreshold1 { get; set; } = 0;

    /// <summary>
    /// Fiyat optimizasyonu için competitor count threshold 2
    /// </summary>
    public int PriceOptimizationCompetitorCountThreshold2 { get; set; } = 5;

    /// <summary>
    /// Fiyat optimizasyonu için competitor count increase 1
    /// </summary>
    public decimal PriceOptimizationCompetitorCountIncrease1 { get; set; } = 20;

    /// <summary>
    /// Fiyat optimizasyonu için competitor count increase 2
    /// </summary>
    public decimal PriceOptimizationCompetitorCountIncrease2 { get; set; } = 10;

    // Fraud Detection Settings
    /// <summary>
    /// Fraud detection risk score cap (maksimum risk score)
    /// </summary>
    public int FraudDetectionRiskScoreCap { get; set; } = 100;

    /// <summary>
    /// Fraud detection yeni hesap kontrolü için gün sayısı
    /// </summary>
    public int FraudDetectionNewAccountDays { get; set; } = 7;

    /// <summary>
    /// Fraud detection yüksek risk threshold (risk score >= bu değer)
    /// </summary>
    public int FraudDetectionHighRiskThreshold { get; set; } = 70;

    /// <summary>
    /// Fraud detection yüksek risk alert limit (analytics için)
    /// </summary>
    public int FraudDetectionHighRiskAlertsLimit { get; set; } = 10;

    // Demand Forecasting Helper Magic Numbers
    /// <summary>
    /// Talep tahmini hafta sonu faktörü
    /// </summary>
    public decimal DemandForecastWeekendFactor { get; set; } = 1.2m;

    /// <summary>
    /// Talep tahmini stok faktörü (stok = 0)
    /// </summary>
    public decimal DemandForecastStockFactorZero { get; set; } = 0.5m;

    /// <summary>
    /// Talep tahmini stok faktörü (stok <= 10)
    /// </summary>
    public decimal DemandForecastStockFactorLow { get; set; } = 0.8m;

    /// <summary>
    /// Talep tahmini stok faktörü (stok > 10)
    /// </summary>
    public decimal DemandForecastStockFactorHigh { get; set; } = 1.0m;

    /// <summary>
    /// Talep tahmini rating faktörü (rating >= 4.5)
    /// </summary>
    public decimal DemandForecastRatingFactorHigh { get; set; } = 1.3m;

    /// <summary>
    /// Talep tahmini rating faktörü (rating >= 4.0)
    /// </summary>
    public decimal DemandForecastRatingFactorMediumHigh { get; set; } = 1.1m;

    /// <summary>
    /// Talep tahmini rating faktörü (rating >= 3.5)
    /// </summary>
    public decimal DemandForecastRatingFactorMedium { get; set; } = 1.0m;

    /// <summary>
    /// Talep tahmini rating faktörü (rating < 3.5)
    /// </summary>
    public decimal DemandForecastRatingFactorLow { get; set; } = 0.9m;

    // Price Optimization Helper Magic Numbers
    /// <summary>
    /// Fiyat optimizasyonu maksimum confidence değeri
    /// </summary>
    public decimal PriceOptimizationMaxConfidence { get; set; } = 100;

    /// <summary>
    /// Fiyat optimizasyonu base rating confidence artışı
    /// </summary>
    public decimal PriceOptimizationBaseRatingConfidence { get; set; } = 10;

    /// <summary>
    /// Fiyat optimizasyonu review count confidence artışı
    /// </summary>
    public decimal PriceOptimizationReviewCountConfidence { get; set; } = 10;

    // Rating Thresholds (Demand Forecasting & Price Optimization)
    /// <summary>
    /// Yüksek rating threshold (>= bu değer)
    /// </summary>
    public decimal RatingThresholdHigh { get; set; } = 4.5m;

    /// <summary>
    /// Orta-yüksek rating threshold (>= bu değer)
    /// </summary>
    public decimal RatingThresholdMediumHigh { get; set; } = 4.0m;

    /// <summary>
    /// Orta rating threshold (>= bu değer)
    /// </summary>
    public decimal RatingThresholdMedium { get; set; } = 3.5m;

    // Stock Thresholds
    /// <summary>
    /// Düşük stok threshold (<= bu değer)
    /// </summary>
    public int StockThresholdLow { get; set; } = 10;

    // Review Count Thresholds (Price Optimization)
    /// <summary>
    /// Yüksek review count threshold (>= bu değer)
    /// </summary>
    public int ReviewCountThresholdHigh { get; set; } = 100;

    /// <summary>
    /// Orta review count threshold (>= bu değer)
    /// </summary>
    public int ReviewCountThresholdMedium { get; set; } = 50;

    // Day of Week Factors
    /// <summary>
    /// Hafta içi faktörü (default: 1.0m - neutral)
    /// </summary>
    public decimal WeekdayFactor { get; set; } = 1.0m;
}
