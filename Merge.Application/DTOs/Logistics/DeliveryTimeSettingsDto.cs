using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Logistics;

/// <summary>
/// Delivery time estimation ayarlari icin typed DTO - Dictionary yerine guvenli
/// </summary>
public class DeliveryTimeSettingsDto
{
    /// <summary>
    /// Aktif mi
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Varsayilan teslimat suresi (gun)
    /// </summary>
    [Range(1, 365)]
    public int DefaultDeliveryDays { get; set; } = 3;

    /// <summary>
    /// Minimum teslimat suresi (gun)
    /// </summary>
    [Range(1, 365)]
    public int MinDeliveryDays { get; set; } = 1;

    /// <summary>
    /// Maksimum teslimat suresi (gun)
    /// </summary>
    [Range(1, 365)]
    public int MaxDeliveryDays { get; set; } = 30;

    /// <summary>
    /// Hafta sonu teslimat aktif mi
    /// </summary>
    public bool WeekendDelivery { get; set; } = false;

    /// <summary>
    /// Tatil gunleri teslimat aktif mi
    /// </summary>
    public bool HolidayDelivery { get; set; } = false;

    /// <summary>
    /// Ayni gun teslimat aktif mi
    /// </summary>
    public bool SameDayDelivery { get; set; } = false;

    /// <summary>
    /// Ayni gun teslimat kesim saati
    /// </summary>
    public TimeSpan? SameDayCutoffTime { get; set; }

    /// <summary>
    /// Ekspres teslimat aktif mi
    /// </summary>
    public bool ExpressDelivery { get; set; } = false;

    /// <summary>
    /// Ekspres teslimat ek ucreti
    /// </summary>
    [Range(0, 10000)]
    public decimal? ExpressDeliveryFee { get; set; }

    /// <summary>
    /// Ucretsiz kargo esigi
    /// </summary>
    [Range(0, 100000)]
    public decimal? FreeShippingThreshold { get; set; }

    /// <summary>
    /// Bolgesel farklar aktif mi
    /// </summary>
    public bool RegionalVariationsEnabled { get; set; } = false;
}
