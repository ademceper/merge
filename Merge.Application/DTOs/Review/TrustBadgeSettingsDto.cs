using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Review;

/// <summary>
/// Trust badge ayarlari icin typed DTO - Dictionary yerine guvenli
/// </summary>
public class TrustBadgeSettingsDto
{
    /// <summary>
    /// Badge aktif mi
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Badge tipi
    /// </summary>
    [StringLength(50)]
    public string? BadgeType { get; set; }

    /// <summary>
    /// Ikon URL
    /// </summary>
    [StringLength(500)]
    [Url]
    public string? IconUrl { get; set; }

    /// <summary>
    /// Arka plan rengi
    /// </summary>
    [StringLength(20)]
    public string? BackgroundColor { get; set; }

    /// <summary>
    /// Metin rengi
    /// </summary>
    [StringLength(20)]
    public string? TextColor { get; set; }

    /// <summary>
    /// Gosterim onceligi
    /// </summary>
    [Range(0, 100)]
    public int DisplayPriority { get; set; } = 0;

    /// <summary>
    /// Minimum review sayisi
    /// </summary>
    [Range(0, 10000)]
    public int? MinReviewCount { get; set; }

    /// <summary>
    /// Minimum ortalama puan
    /// </summary>
    [Range(0, 5)]
    public decimal? MinAverageRating { get; set; }

    /// <summary>
    /// Minimum satici puani
    /// </summary>
    [Range(0, 100)]
    public decimal? MinSellerScore { get; set; }

    /// <summary>
    /// Gosterim baslangic tarihi
    /// </summary>
    public DateTime? DisplayStartDate { get; set; }

    /// <summary>
    /// Gosterim bitis tarihi
    /// </summary>
    public DateTime? DisplayEndDate { get; set; }

    /// <summary>
    /// Mobilde goster
    /// </summary>
    public bool ShowOnMobile { get; set; } = true;

    /// <summary>
    /// Desktop'ta goster
    /// </summary>
    public bool ShowOnDesktop { get; set; } = true;

    /// <summary>
    /// Urun sayfasinda goster
    /// </summary>
    public bool ShowOnProductPage { get; set; } = true;

    /// <summary>
    /// Checkout'ta goster
    /// </summary>
    public bool ShowOnCheckout { get; set; } = true;
}
