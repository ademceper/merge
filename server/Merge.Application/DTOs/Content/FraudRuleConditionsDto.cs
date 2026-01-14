using System.ComponentModel.DataAnnotations;
using Merge.Domain.ValueObjects;

namespace Merge.Application.DTOs.Content;

/// <summary>
/// Fraud detection rule conditions icin typed DTO - Dictionary yerine guvenli
/// </summary>
public class FraudRuleConditionsDto
{
    /// <summary>
    /// Kural aktif mi
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Minimum islem tutari
    /// </summary>
    [Range(0, 10000000)]
    public decimal? MinTransactionAmount { get; set; }

    /// <summary>
    /// Maksimum islem tutari
    /// </summary>
    [Range(0, 10000000)]
    public decimal? MaxTransactionAmount { get; set; }

    /// <summary>
    /// Gunluk maksimum islem sayisi
    /// </summary>
    [Range(0, 10000)]
    public int? MaxDailyTransactions { get; set; }

    /// <summary>
    /// Saatlik maksimum islem sayisi
    /// </summary>
    [Range(0, 1000)]
    public int? MaxHourlyTransactions { get; set; }

    /// <summary>
    /// Yeni hesap suresi (gun)
    /// </summary>
    [Range(0, 365)]
    public int? NewAccountDays { get; set; }

    /// <summary>
    /// IP bazli kontrol aktif mi
    /// </summary>
    public bool IpCheckEnabled { get; set; } = true;

    /// <summary>
    /// VPN/Proxy engelleme aktif mi
    /// </summary>
    public bool BlockVpnProxy { get; set; } = false;

    /// <summary>
    /// Cihaz parmak izi kontrolu aktif mi
    /// </summary>
    public bool DeviceFingerprintEnabled { get; set; } = false;

    /// <summary>
    /// Yasakli ulkeler (virgul ile ayrilmis)
    /// </summary>
    [StringLength(500)]
    public string? BlockedCountries { get; set; }

    /// <summary>
    /// Yasakli IP araliklari (virgul ile ayrilmis)
    /// </summary>
    [StringLength(2000)]
    public string? BlockedIpRanges { get; set; }

    /// <summary>
    /// Email domain kontrolu aktif mi
    /// </summary>
    public bool EmailDomainCheckEnabled { get; set; } = false;

    /// <summary>
    /// Disposable email engelleme aktif mi
    /// </summary>
    public bool BlockDisposableEmails { get; set; } = true;

    /// <summary>
    /// 3DS zorunlu mu
    /// </summary>
    public bool Require3DS { get; set; } = true;

    /// <summary>
    /// Adres dogrulama zorunlu mu
    /// </summary>
    public bool RequireAddressVerification { get; set; } = false;

    /// <summary>
    /// Risk esigi (0-100)
    /// </summary>
    [Range(0, 100)]
    public int? RiskThreshold { get; set; }

    /// <summary>
    /// Otomatik red aktif mi
    /// </summary>
    public bool AutoDeclineEnabled { get; set; } = false;

    /// <summary>
    /// Manuel inceleme esigi (risk skoru)
    /// </summary>
    [Range(0, 100)]
    public int? ManualReviewThreshold { get; set; }
}
