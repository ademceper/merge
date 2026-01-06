namespace Merge.Application.Configuration;

/// <summary>
/// Analytics Settings - BOLUM 2.3: Hardcoded Values YASAK (Configuration Kullan)
/// </summary>
public class AnalyticsSettings
{
    public const string SectionName = "AnalyticsSettings";

    /// <summary>
    /// Product cost percentage (default: 60%)
    /// </summary>
    public decimal ProductCostPercentage { get; set; } = 0.60m;

    /// <summary>
    /// Shipping cost percentage (default: 80%)
    /// </summary>
    public decimal ShippingCostPercentage { get; set; } = 0.80m;

    /// <summary>
    /// Platform fee percentage (default: 2%)
    /// </summary>
    public decimal PlatformFeePercentage { get; set; } = 0.02m;

    /// <summary>
    /// Default cost percentage for simplified calculations (default: 40%)
    /// </summary>
    public decimal DefaultCostPercentage { get; set; } = 0.40m;

    /// <summary>
    /// Default profit percentage for simplified calculations (default: 60%)
    /// </summary>
    public decimal DefaultProfitPercentage { get; set; } = 0.60m;

    /// <summary>
    /// Default dashboard period in days (default: 30)
    /// </summary>
    public int DefaultDashboardPeriodDays { get; set; } = 30;

    /// <summary>
    /// Max limit for top products/customers queries (default: 100)
    /// </summary>
    public int MaxQueryLimit { get; set; } = 100;

    /// <summary>
    /// Low stock threshold for products (default: 10)
    /// </summary>
    public int LowStockThreshold { get; set; } = 10;

    /// <summary>
    /// Default period in days for analytics queries (default: 30)
    /// </summary>
    public int DefaultPeriodDays { get; set; } = 30;

    /// <summary>
    /// Top products limit for queries (default: 50)
    /// </summary>
    public int TopProductsLimit { get; set; } = 50;

    /// <summary>
    /// Default limit for queries (default: 10)
    /// </summary>
    public int DefaultLimit { get; set; } = 10;

    /// <summary>
    /// Default page size for pagination (default: 20)
    /// </summary>
    public int DefaultPageSize { get; set; } = 20;

    /// <summary>
    /// Maximum days for date range queries (default: 365)
    /// </summary>
    public int MaxDateRangeDays { get; set; } = 365;

    /// <summary>
    /// Active session threshold in hours (default: 24)
    /// </summary>
    public int ActiveSessionThresholdHours { get; set; } = 24;

    /// <summary>
    /// Default threshold for low stock products (default: 10)
    /// </summary>
    public int DefaultLowStockThreshold { get; set; } = 10;

    /// <summary>
    /// Maximum number of low stock alerts to return in overview (default: 5)
    /// </summary>
    public int MaxLowStockAlertsInOverview { get; set; } = 5;
}

