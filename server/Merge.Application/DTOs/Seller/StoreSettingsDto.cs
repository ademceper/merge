using System.ComponentModel.DataAnnotations;
using Merge.Domain.Modules.Marketplace;
using Merge.Domain.ValueObjects;

namespace Merge.Application.DTOs.Seller;

/// <summary>
/// Store ayarlari icin typed DTO - Dictionary yerine guvenli
/// </summary>
public record StoreSettingsDto
{
    /// <summary>
    /// Magaza acik mi
    /// </summary>
    public bool IsOpen { get; init; } = true;

    /// <summary>
    /// Otomatik siparis onaylama aktif mi
    /// </summary>
    public bool AutoAcceptOrders { get; init; } = true;

    /// <summary>
    /// Minimum siparis tutari
    /// </summary>
    [Range(0, 100000)]
    public decimal? MinimumOrderAmount { get; init; }

    /// <summary>
    /// Ucretsiz kargo icin minimum tutar
    /// </summary>
    [Range(0, 100000)]
    public decimal? FreeShippingThreshold { get; init; }

    /// <summary>
    /// Varsayilan kargo maliyeti
    /// </summary>
    [Range(0, 10000)]
    public decimal? DefaultShippingCost { get; init; }

    /// <summary>
    /// Iade kabul suresi (gun)
    /// </summary>
    [Range(0, 365)]
    public int? ReturnPeriodDays { get; init; }

    /// <summary>
    /// Stok uyari esigi
    /// </summary>
    [Range(0, 10000)]
    public int? LowStockThreshold { get; init; }

    /// <summary>
    /// Email bildirimleri aktif mi
    /// </summary>
    public bool EmailNotificationsEnabled { get; init; } = true;

    /// <summary>
    /// SMS bildirimleri aktif mi
    /// </summary>
    public bool SmsNotificationsEnabled { get; init; } = false;

    /// <summary>
    /// Varsayilan dil kodu
    /// </summary>
    [StringLength(10)]
    public string? DefaultLanguage { get; init; }

    /// <summary>
    /// Varsayilan para birimi
    /// </summary>
    [StringLength(3)]
    public string? DefaultCurrency { get; init; }

    /// <summary>
    /// Calisma saatleri baslangic
    /// </summary>
    public TimeSpan? WorkingHoursStart { get; init; }

    /// <summary>
    /// Calisma saatleri bitis
    /// </summary>
    public TimeSpan? WorkingHoursEnd { get; init; }

    /// <summary>
    /// Hafta sonu calisma aktif mi
    /// </summary>
    public bool WorkOnWeekends { get; init; } = false;
}
