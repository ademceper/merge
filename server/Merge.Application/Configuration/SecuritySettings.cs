using Merge.Domain.Modules.Ordering;
using Merge.Domain.Modules.Payment;
namespace Merge.Application.Configuration;

/// <summary>
/// Guvenlik islemleri icin configuration ayarlari
/// </summary>
public class SecuritySettings
{
    public const string SectionName = "SecuritySettings";

    /// <summary>
    /// Varsayilan istatistik periyodu (gun)
    /// </summary>
    public int DefaultStatsPeriodDays { get; set; } = 30;

    /// <summary>
    /// Maksimum basarisiz giris denemesi
    /// </summary>
    public int MaxFailedLoginAttempts { get; set; } = 5;

    /// <summary>
    /// Hesap kilitleme suresi (dakika)
    /// </summary>
    public int AccountLockoutMinutes { get; set; } = 15;

    /// <summary>
    /// Yeni kullanıcılar için varsayılan rol adı
    /// </summary>
    public string DefaultUserRole { get; set; } = "Customer";

    /// <summary>
    /// Order verification için yüksek risk skoru eşiği (0-100)
    /// </summary>
    public int OrderVerificationHighRiskThreshold { get; set; } = 70;

    /// <summary>
    /// Order verification için manuel inceleme gerektiren risk skoru eşiği (0-100)
    /// </summary>
    public int OrderVerificationManualReviewThreshold { get; set; } = 70;

    /// <summary>
    /// Payment fraud check için yüksek risk skoru eşiği (0-100)
    /// </summary>
    public int PaymentFraudHighRiskThreshold { get; set; } = 70;

    /// <summary>
    /// Payment fraud check için orta risk skoru eşiği (0-100)
    /// </summary>
    public int PaymentFraudMediumRiskThreshold { get; set; } = 50;

    /// <summary>
    /// Yüksek değerli sipariş eşiği (TL)
    /// </summary>
    public decimal HighValueOrderThreshold { get; set; } = 10000;

    /// <summary>
    /// Yüksek değerli ödeme eşiği (TL)
    /// </summary>
    public decimal HighValuePaymentThreshold { get; set; } = 5000;

    /// <summary>
    /// Yeni kullanıcı için risk skoru (gün)
    /// </summary>
    public int NewUserRiskDays { get; set; } = 7;

    /// <summary>
    /// Çoklu sipariş item sayısı eşiği
    /// </summary>
    public int MultipleItemsThreshold { get; set; } = 10;

    /// <summary>
    /// Yüksek miktar eşiği
    /// </summary>
    public int HighQuantityThreshold { get; set; } = 20;

    /// <summary>
    /// Kısa sürede aynı IP'den ödeme sayısı eşiği
    /// </summary>
    public int RecentPaymentsFromSameIpThreshold { get; set; } = 3;

    /// <summary>
    /// Kısa sürede aynı IP'den ödeme kontrolü için saat cinsinden süre
    /// </summary>
    public int RecentPaymentsTimeWindowHours { get; set; } = 1;

    /// <summary>
    /// Security summary için gösterilecek kritik alert sayısı
    /// </summary>
    public int RecentCriticalAlertsLimit { get; set; } = 10;

    /// <summary>
    /// Risk score maksimum değeri (0-100)
    /// </summary>
    public int MaxRiskScore { get; set; } = 100;

    /// <summary>
    /// Yüksek değerli sipariş için risk skoru ağırlığı
    /// </summary>
    public int HighValueOrderRiskWeight { get; set; } = 30;

    /// <summary>
    /// Yeni kullanıcı için risk skoru ağırlığı
    /// </summary>
    public int NewUserRiskWeight { get; set; } = 20;

    /// <summary>
    /// Çoklu item için risk skoru ağırlığı
    /// </summary>
    public int MultipleItemsRiskWeight { get; set; } = 15;

    /// <summary>
    /// Yüksek miktar için risk skoru ağırlığı
    /// </summary>
    public int HighQuantityRiskWeight { get; set; } = 15;

    /// <summary>
    /// Yüksek değerli ödeme için risk skoru ağırlığı
    /// </summary>
    public int HighValuePaymentRiskWeight { get; set; } = 25;

    /// <summary>
    /// Aynı IP'den çoklu ödeme için risk skoru ağırlığı
    /// </summary>
    public int MultiplePaymentsFromSameIpRiskWeight { get; set; } = 30;

    /// <summary>
    /// Device fingerprint eksikliği için risk skoru ağırlığı
    /// </summary>
    public int MissingDeviceFingerprintRiskWeight { get; set; } = 15;
}
