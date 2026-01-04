using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Payment;

/// <summary>
/// Payment method ayarlari icin typed DTO - Dictionary yerine guvenli
/// </summary>
public class PaymentMethodSettingsDto
{
    /// <summary>
    /// Metod aktif mi
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Test modu aktif mi
    /// </summary>
    public bool TestMode { get; set; } = false;

    /// <summary>
    /// Minimum islem tutari
    /// </summary>
    [Range(0, 1000000)]
    public decimal? MinAmount { get; set; }

    /// <summary>
    /// Maksimum islem tutari
    /// </summary>
    [Range(0, 10000000)]
    public decimal? MaxAmount { get; set; }

    /// <summary>
    /// Islem ucreti (sabit)
    /// </summary>
    [Range(0, 10000)]
    public decimal? TransactionFee { get; set; }

    /// <summary>
    /// Islem ucreti (yuzde)
    /// </summary>
    [Range(0, 100)]
    public decimal? TransactionFeePercentage { get; set; }

    /// <summary>
    /// Taksit destegi
    /// </summary>
    public bool InstallmentEnabled { get; set; } = false;

    /// <summary>
    /// Maksimum taksit sayisi
    /// </summary>
    [Range(1, 36)]
    public int? MaxInstallments { get; set; }

    /// <summary>
    /// 3D Secure zorunlu mu
    /// </summary>
    public bool Require3DSecure { get; set; } = true;

    /// <summary>
    /// Otomatik capture
    /// </summary>
    public bool AutoCapture { get; set; } = true;

    /// <summary>
    /// Desteklenen para birimleri (virgul ile ayrilmis)
    /// </summary>
    [StringLength(100)]
    public string? SupportedCurrencies { get; set; }

    /// <summary>
    /// Webhook URL
    /// </summary>
    [StringLength(500)]
    [Url]
    public string? WebhookUrl { get; set; }

    /// <summary>
    /// Basarili odeme sonrasi yonlendirme URL
    /// </summary>
    [StringLength(500)]
    [Url]
    public string? SuccessUrl { get; set; }

    /// <summary>
    /// Basarisiz odeme sonrasi yonlendirme URL
    /// </summary>
    [StringLength(500)]
    [Url]
    public string? FailureUrl { get; set; }
}
