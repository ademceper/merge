using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Seller;

/// <summary>
/// Store ayarlari icin typed DTO - Dictionary yerine guvenli
/// </summary>
public class StoreSettingsDto
{
    /// <summary>
    /// Magaza acik mi
    /// </summary>
    public bool IsOpen { get; set; } = true;

    /// <summary>
    /// Otomatik siparis onaylama aktif mi
    /// </summary>
    public bool AutoAcceptOrders { get; set; } = true;

    /// <summary>
    /// Minimum siparis tutari
    /// </summary>
    [Range(0, 100000)]
    public decimal? MinimumOrderAmount { get; set; }

    /// <summary>
    /// Ucretsiz kargo icin minimum tutar
    /// </summary>
    [Range(0, 100000)]
    public decimal? FreeShippingThreshold { get; set; }

    /// <summary>
    /// Varsayilan kargo maliyeti
    /// </summary>
    [Range(0, 10000)]
    public decimal? DefaultShippingCost { get; set; }

    /// <summary>
    /// Iade kabul suresi (gun)
    /// </summary>
    [Range(0, 365)]
    public int? ReturnPeriodDays { get; set; }

    /// <summary>
    /// Stok uyari esigi
    /// </summary>
    [Range(0, 10000)]
    public int? LowStockThreshold { get; set; }

    /// <summary>
    /// Email bildirimleri aktif mi
    /// </summary>
    public bool EmailNotificationsEnabled { get; set; } = true;

    /// <summary>
    /// SMS bildirimleri aktif mi
    /// </summary>
    public bool SmsNotificationsEnabled { get; set; } = false;

    /// <summary>
    /// Varsayilan dil kodu
    /// </summary>
    [StringLength(10)]
    public string? DefaultLanguage { get; set; }

    /// <summary>
    /// Varsayilan para birimi
    /// </summary>
    [StringLength(3)]
    public string? DefaultCurrency { get; set; }

    /// <summary>
    /// Calisma saatleri baslangic
    /// </summary>
    public TimeSpan? WorkingHoursStart { get; set; }

    /// <summary>
    /// Calisma saatleri bitis
    /// </summary>
    public TimeSpan? WorkingHoursEnd { get; set; }

    /// <summary>
    /// Hafta sonu calisma aktif mi
    /// </summary>
    public bool WorkOnWeekends { get; set; } = false;
}
