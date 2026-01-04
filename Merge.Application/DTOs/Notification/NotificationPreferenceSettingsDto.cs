using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Notification;

/// <summary>
/// Notification preference ayarlari icin typed DTO - Dictionary yerine guvenli
/// </summary>
public class NotificationPreferenceSettingsDto
{
    /// <summary>
    /// Email bildirimleri aktif mi
    /// </summary>
    public bool EmailEnabled { get; set; } = true;

    /// <summary>
    /// SMS bildirimleri aktif mi
    /// </summary>
    public bool SmsEnabled { get; set; } = false;

    /// <summary>
    /// Push bildirimleri aktif mi
    /// </summary>
    public bool PushEnabled { get; set; } = true;

    /// <summary>
    /// Siparis bildirimleri
    /// </summary>
    public bool OrderNotifications { get; set; } = true;

    /// <summary>
    /// Promosyon bildirimleri
    /// </summary>
    public bool PromotionNotifications { get; set; } = true;

    /// <summary>
    /// Stok bildirimleri
    /// </summary>
    public bool StockNotifications { get; set; } = true;

    /// <summary>
    /// Fiyat degisiklik bildirimleri
    /// </summary>
    public bool PriceChangeNotifications { get; set; } = false;

    /// <summary>
    /// Haftalik ozet bildirimleri
    /// </summary>
    public bool WeeklySummary { get; set; } = true;

    /// <summary>
    /// Sessiz saatler baslangic
    /// </summary>
    public TimeSpan? QuietHoursStart { get; set; }

    /// <summary>
    /// Sessiz saatler bitis
    /// </summary>
    public TimeSpan? QuietHoursEnd { get; set; }

    /// <summary>
    /// Tercih edilen dil
    /// </summary>
    [StringLength(10)]
    public string? PreferredLanguage { get; set; }

    /// <summary>
    /// Zaman dilimi
    /// </summary>
    [StringLength(50)]
    public string? Timezone { get; set; }
}
