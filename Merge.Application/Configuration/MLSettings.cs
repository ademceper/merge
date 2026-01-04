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
}
