namespace Merge.Application.Configuration;

/// <summary>
/// Content işlemleri için configuration ayarları
/// BOLUM 2.3: Hardcoded Values YASAK (Configuration Kullan)
/// </summary>
public class ContentSettings
{
    public const string SectionName = "ContentSettings";

    /// <summary>
    /// Blog post okuma süresi hesaplama için ortalama okuma hızı (kelime/dakika)
    /// Default: 200 words per minute
    /// </summary>
    public int AverageReadingSpeedWordsPerMinute { get; set; } = 200;

    /// <summary>
    /// Maksimum featured posts sayısı
    /// </summary>
    public int MaxFeaturedPostsCount { get; set; } = 50;

    /// <summary>
    /// Maksimum recent posts sayısı
    /// </summary>
    public int MaxRecentPostsCount { get; set; } = 50;
}

