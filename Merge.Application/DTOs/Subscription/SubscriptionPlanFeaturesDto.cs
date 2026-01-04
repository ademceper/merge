using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Subscription;

/// <summary>
/// Subscription plan ozellikleri icin typed DTO - Dictionary yerine guvenli
/// </summary>
public class SubscriptionPlanFeaturesDto
{
    /// <summary>
    /// Maksimum urun sayisi
    /// </summary>
    [Range(0, 1000000)]
    public int? MaxProducts { get; set; }

    /// <summary>
    /// Maksimum siparis sayisi (aylik)
    /// </summary>
    [Range(0, 1000000)]
    public int? MaxOrdersPerMonth { get; set; }

    /// <summary>
    /// Maksimum depolama alani (MB)
    /// </summary>
    [Range(0, 1000000)]
    public int? MaxStorageMB { get; set; }

    /// <summary>
    /// Maksimum kullanici sayisi
    /// </summary>
    [Range(1, 1000)]
    public int? MaxUsers { get; set; }

    /// <summary>
    /// Maksimum magaza sayisi
    /// </summary>
    [Range(1, 100)]
    public int? MaxStores { get; set; }

    /// <summary>
    /// API erisimi
    /// </summary>
    public bool ApiAccess { get; set; } = false;

    /// <summary>
    /// Analitik erisimi
    /// </summary>
    public bool AnalyticsAccess { get; set; } = false;

    /// <summary>
    /// Gelismis raporlar
    /// </summary>
    public bool AdvancedReports { get; set; } = false;

    /// <summary>
    /// Oncelikli destek
    /// </summary>
    public bool PrioritySupport { get; set; } = false;

    /// <summary>
    /// 7/24 destek
    /// </summary>
    public bool Support24x7 { get; set; } = false;

    /// <summary>
    /// Ozel domain destegi
    /// </summary>
    public bool CustomDomain { get; set; } = false;

    /// <summary>
    /// SSL sertifikasi dahil
    /// </summary>
    public bool SslIncluded { get; set; } = false;

    /// <summary>
    /// Beyaz etiket destegi
    /// </summary>
    public bool WhiteLabel { get; set; } = false;

    /// <summary>
    /// Entegrasyon sayisi limiti
    /// </summary>
    [Range(0, 100)]
    public int? MaxIntegrations { get; set; }

    /// <summary>
    /// Email kampanya limiti (aylik)
    /// </summary>
    [Range(0, 1000000)]
    public int? MaxEmailsPerMonth { get; set; }

    /// <summary>
    /// SMS limiti (aylik)
    /// </summary>
    [Range(0, 100000)]
    public int? MaxSmsPerMonth { get; set; }

    /// <summary>
    /// Komisyon orani indirim yuzdesi
    /// </summary>
    [Range(0, 100)]
    public decimal? CommissionDiscountPercentage { get; set; }
}
