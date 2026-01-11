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

    /// <summary>
    /// Varsayilan platform fee orani (%)
    /// </summary>
    public decimal DefaultPlatformFeeRate { get; set; } = 2;

    /// <summary>
    /// Varsayilan komisyon orani (tier yoksa kullanilir) (%)
    /// </summary>
    public decimal DefaultCommissionRateWhenNoTier { get; set; } = 10;

    /// <summary>
    /// Payout transaction fee orani (%)
    /// </summary>
    public decimal PayoutTransactionFeeRate { get; set; } = 1;

    /// <summary>
    /// Varsayilan minimum payout tutari
    /// </summary>
    public decimal DefaultMinimumPayoutAmount { get; set; } = 100;

    /// <summary>
    /// Varsayilan odeme yontemi
    /// </summary>
    public string DefaultPaymentMethod { get; set; } = "Bank Transfer";

    /// <summary>
    /// Top product listesi için limit (performance metrics)
    /// </summary>
    public int TopProductsLimit { get; set; } = 10;

    /// <summary>
    /// Low stock threshold (stok uyarısı için minimum miktar)
    /// </summary>
    public int LowStockThreshold { get; set; } = 10;

    /// <summary>
    /// Finance summary için recent transactions ve invoices limiti
    /// </summary>
    public int RecentItemsLimit { get; set; } = 10;
}
