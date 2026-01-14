namespace Merge.Application.Configuration;

/// <summary>
/// Service layer ayarları - Magic number'ları config'e taşıma (BOLUM 12.0)
/// </summary>
public class ServiceSettings
{
    public const string SectionName = "ServiceSettings";

    /// <summary>
    /// Varsayılan tarih aralığı (gün cinsinden) - Örnek: -30 gün
    /// </summary>
    public int DefaultDateRangeDays { get; set; } = 30;

    /// <summary>
    /// Kısa tarih aralığı (gün cinsinden) - Örnek: -7 gün
    /// </summary>
    public int ShortDateRangeDays { get; set; } = 7;

    /// <summary>
    /// Uzun tarih aralığı (gün cinsinden) - Örnek: -90 gün
    /// </summary>
    public int LongDateRangeDays { get; set; } = 90;

    /// <summary>
    /// Cache süresi (dakika cinsinden) - Örnek: 30 dakika
    /// </summary>
    public int DefaultCacheMinutes { get; set; } = 30;

    /// <summary>
    /// Kısa cache süresi (dakika cinsinden) - Örnek: 5 dakika
    /// </summary>
    public int ShortCacheMinutes { get; set; } = 5;

    /// <summary>
    /// Uzun cache süresi (dakika cinsinden) - Örnek: 60 dakika
    /// </summary>
    public int LongCacheMinutes { get; set; } = 60;
}

