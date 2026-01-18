namespace Merge.Application.Configuration;


public class SearchSettings
{
    public const string SectionName = "SearchSettings";

    /// <summary>
    /// Maksimum sayfa boyutu
    /// </summary>
    public int MaxPageSize { get; set; } = 100;

    /// <summary>
    /// Varsayılan sayfa boyutu
    /// </summary>
    public int DefaultPageSize { get; set; } = 20;

    /// <summary>
    /// Maksimum autocomplete sonuç sayısı
    /// </summary>
    public int MaxAutocompleteResults { get; set; } = 10;

    /// <summary>
    /// Maksimum recommendation sonuç sayısı
    /// </summary>
    public int MaxRecommendationResults { get; set; } = 10;

    /// <summary>
    /// Maksimum trending days
    /// </summary>
    public int MaxTrendingDays { get; set; } = 365;

    /// <summary>
    /// Varsayılan trending days
    /// </summary>
    public int DefaultTrendingDays { get; set; } = 7;

    /// <summary>
    /// Varsayılan new arrivals days
    /// </summary>
    public int DefaultNewArrivalsDays { get; set; } = 30;

    /// <summary>
    /// Minimum autocomplete query length
    /// </summary>
    public int MinAutocompleteQueryLength { get; set; } = 2;

    /// <summary>
    /// Cache expiration (minutes) - Search results
    /// </summary>
    public int SearchCacheExpirationMinutes { get; set; } = 5;

    /// <summary>
    /// Cache expiration (minutes) - Recommendations
    /// </summary>
    public int RecommendationCacheExpirationMinutes { get; set; } = 30;

    /// <summary>
    /// Similar products price range multiplier (min)
    /// </summary>
    public decimal SimilarProductsPriceRangeMin { get; set; } = 0.7m;

    /// <summary>
    /// Similar products price range multiplier (max)
    /// </summary>
    public decimal SimilarProductsPriceRangeMax { get; set; } = 1.3m;

    /// <summary>
    /// Minimum rating for personalized recommendations
    /// </summary>
    public decimal MinRatingForPersonalizedRecommendations { get; set; } = 4.0m;
}
