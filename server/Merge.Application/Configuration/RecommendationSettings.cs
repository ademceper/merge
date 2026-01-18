namespace Merge.Application.Configuration;


public class RecommendationSettings
{
    public const string SectionName = "RecommendationSettings";

    // Size recommendation algorithm thresholds
    public decimal AlternativeSizeScoreThreshold { get; set; } = 20; // Alternative sizes için maksimum score threshold
    public decimal HighConfidenceScoreThreshold { get; set; } = 5; // High confidence için maksimum score threshold
    public decimal MediumConfidenceScoreThreshold { get; set; } = 10; // Medium confidence için maksimum score threshold
    public int MaxAlternativeSizesCount { get; set; } = 3; // Maksimum alternatif beden sayısı
}
