namespace Merge.Application.Configuration;

/// <summary>
/// Review domain için configuration ayarları
/// Magic number'ları buraya taşıyoruz (BOLUM 12.0)
/// </summary>
public class ReviewSettings
{
    public const string SectionName = "ReviewSettings";

    /// <summary>
    /// Pagination için maksimum sayfa boyutu (default: 100)
    /// </summary>
    public int MaxPageSize { get; set; } = 100;

    /// <summary>
    /// Varsayılan sayfa boyutu (default: 20)
    /// </summary>
    public int DefaultPageSize { get; set; } = 20;

    /// <summary>
    /// Most helpful reviews için maksimum limit (default: 100)
    /// </summary>
    public int MaxHelpfulReviewsLimit { get; set; } = 100;

    /// <summary>
    /// Most helpful reviews için varsayılan limit (default: 10)
    /// </summary>
    public int DefaultHelpfulReviewsLimit { get; set; } = 10;
}
