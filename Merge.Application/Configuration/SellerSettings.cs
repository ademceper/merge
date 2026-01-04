namespace Merge.Application.Configuration;

/// <summary>
/// Seller/Store islemleri icin configuration ayarlari
/// </summary>
public class SellerSettings
{
    public const string SectionName = "SellerSettings";

    /// <summary>
    /// Varsayilan komisyon orani (%)
    /// </summary>
    public decimal DefaultCommissionRate { get; set; } = 15;

    /// <summary>
    /// Minimum komisyon orani (%)
    /// </summary>
    public decimal MinCommissionRate { get; set; } = 5;

    /// <summary>
    /// Maksimum komisyon orani (%)
    /// </summary>
    public decimal MaxCommissionRate { get; set; } = 30;

    /// <summary>
    /// Varsayilan istatistik periyodu (gun)
    /// </summary>
    public int DefaultStatsPeriodDays { get; set; } = 30;
}
