namespace Merge.Application.Configuration;


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

