namespace Merge.Application.Configuration;

/// <summary>
/// Pagination ayarları - Magic number'ları config'e taşıma (BOLUM 12.0)
/// </summary>
public class PaginationSettings
{
    public const string SectionName = "PaginationSettings";

    /// <summary>
    /// Maksimum sayfa boyutu (default: 100)
    /// </summary>
    public int MaxPageSize { get; set; } = 100;

    /// <summary>
    /// Varsayılan sayfa boyutu (default: 20)
    /// </summary>
    public int DefaultPageSize { get; set; } = 20;
}

