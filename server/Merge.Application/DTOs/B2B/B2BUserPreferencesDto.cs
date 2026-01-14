using System.ComponentModel.DataAnnotations;
using Merge.Domain.Modules.Identity;
using Merge.Domain.ValueObjects;

namespace Merge.Application.DTOs.B2B;

/// <summary>
/// B2B User ayarlari icin typed DTO - Dictionary yerine guvenli
/// </summary>
public class B2BUserPreferencesDto
{
    /// <summary>
    /// Varsayilan dil
    /// </summary>
    [StringLength(10)]
    public string? DefaultLanguage { get; set; }

    /// <summary>
    /// Varsayilan para birimi
    /// </summary>
    [StringLength(3)]
    public string? DefaultCurrency { get; set; }

    /// <summary>
    /// Zaman dilimi
    /// </summary>
    [StringLength(50)]
    public string? Timezone { get; set; }

    /// <summary>
    /// Email bildirimleri aktif mi
    /// </summary>
    public bool EmailNotifications { get; set; } = true;

    /// <summary>
    /// Siparis email bildirimleri
    /// </summary>
    public bool OrderEmailNotifications { get; set; } = true;

    /// <summary>
    /// Fiyat degisiklik bildirimleri
    /// </summary>
    public bool PriceChangeNotifications { get; set; } = true;

    /// <summary>
    /// Stok uyari bildirimleri
    /// </summary>
    public bool StockAlertNotifications { get; set; } = true;

    /// <summary>
    /// Haftalik rapor bildirimleri
    /// </summary>
    public bool WeeklyReportNotifications { get; set; } = true;

    /// <summary>
    /// Tercih edilen odeme yontemi
    /// </summary>
    [StringLength(50)]
    public string? PreferredPaymentMethod { get; set; }

    /// <summary>
    /// Tercih edilen kargo yontemi
    /// </summary>
    [StringLength(50)]
    public string? PreferredShippingMethod { get; set; }

    /// <summary>
    /// Otomatik yeniden siparis aktif mi
    /// </summary>
    public bool AutoReorderEnabled { get; set; } = false;

    /// <summary>
    /// Otomatik siparis esigi (stok)
    /// </summary>
    [Range(0, 10000)]
    public int? AutoReorderThreshold { get; set; }

    /// <summary>
    /// Toplu fiyatlandirma goster
    /// </summary>
    public bool ShowBulkPricing { get; set; } = true;

    /// <summary>
    /// Kredi limiti goster
    /// </summary>
    public bool ShowCreditLimit { get; set; } = true;
}
