namespace Merge.Application.Configuration;

/// <summary>
/// Size recommendation settings - BOLUM 12.0: Magic Number'ları Configuration'a Taşıma (Clean Architecture)
/// </summary>
public class RecommendationSettings
{
    public const string SectionName = "RecommendationSettings";

    // ✅ BOLUM 12.0: Magic Number'ları Constants'a Taşıma (Clean Architecture)
    // Size recommendation algorithm thresholds
    public decimal AlternativeSizeScoreThreshold { get; set; } = 20; // Alternative sizes için maksimum score threshold
    public decimal HighConfidenceScoreThreshold { get; set; } = 5; // High confidence için maksimum score threshold
    public decimal MediumConfidenceScoreThreshold { get; set; } = 10; // Medium confidence için maksimum score threshold
    public int MaxAlternativeSizesCount { get; set; } = 3; // Maksimum alternatif beden sayısı
}
