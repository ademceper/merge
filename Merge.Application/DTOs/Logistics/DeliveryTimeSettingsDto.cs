using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Logistics;

/// <summary>
/// Delivery time estimation ayarlari icin typed DTO - Dictionary yerine guvenli
/// ✅ BOLUM 7.1.5: Records (ZORUNLU - DTOs record olmalı)
/// </summary>
public record DeliveryTimeSettingsDto(
    /// <summary>
    /// Aktif mi
    /// </summary>
    [Required]
    bool IsActive = true,

    /// <summary>
    /// Varsayilan teslimat suresi (gun)
    /// </summary>
    [Range(1, 365)]
    int DefaultDeliveryDays = 3,

    /// <summary>
    /// Minimum teslimat suresi (gun)
    /// </summary>
    [Range(1, 365)]
    int MinDeliveryDays = 1,

    /// <summary>
    /// Maksimum teslimat suresi (gun)
    /// </summary>
    [Range(1, 365)]
    int MaxDeliveryDays = 30,

    /// <summary>
    /// Hafta sonu teslimat aktif mi
    /// </summary>
    bool WeekendDelivery = false,

    /// <summary>
    /// Tatil gunleri teslimat aktif mi
    /// </summary>
    bool HolidayDelivery = false,

    /// <summary>
    /// Ayni gun teslimat aktif mi
    /// </summary>
    bool SameDayDelivery = false,

    /// <summary>
    /// Ayni gun teslimat kesim saati
    /// </summary>
    TimeSpan? SameDayCutoffTime = null,

    /// <summary>
    /// Ekspres teslimat aktif mi
    /// </summary>
    bool ExpressDelivery = false,

    /// <summary>
    /// Ekspres teslimat ek ucreti
    /// </summary>
    [Range(0, 10000)]
    decimal? ExpressDeliveryFee = null,

    /// <summary>
    /// Ucretsiz kargo esigi
    /// </summary>
    [Range(0, 100000)]
    decimal? FreeShippingThreshold = null,

    /// <summary>
    /// Bolgesel farklar aktif mi
    /// </summary>
    bool RegionalVariationsEnabled = false
);
