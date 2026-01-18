namespace Merge.Application.Configuration;


public class B2BSettings
{
    public const string SectionName = "B2BSettings";

    /// <summary>
    /// Default tax rate for purchase orders (0-1 arası, örn: 0.20 = %20)
    /// </summary>
    public decimal DefaultTaxRate { get; set; } = 0.20m;

    /// <summary>
    /// Maximum page size for pagination
    /// </summary>
    public int MaxPageSize { get; set; } = 100;
}

