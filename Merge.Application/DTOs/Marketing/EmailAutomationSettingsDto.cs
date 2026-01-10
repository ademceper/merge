using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Marketing;

/// <summary>
/// Email automation ayarlari icin typed DTO - Dictionary yerine guvenli
/// BOLUM 1.0: DTO Dosya Organizasyonu (ZORUNLU)
/// </summary>
public record EmailAutomationSettingsDto
{
    /// <summary>
    /// Otomasyon aktif mi
    /// </summary>
    public bool IsActive { get; init; } = true;

    /// <summary>
    /// Trigger tipi
    /// </summary>
    [StringLength(50)]
    public string? TriggerType { get; init; }

    /// <summary>
    /// Gecikme suresi (dakika)
    /// </summary>
    [Range(0, 43200)]
    public int? DelayMinutes { get; init; }

    /// <summary>
    /// Bir kullaniciya maksimum gonderim sayisi
    /// </summary>
    [Range(1, 100)]
    public int? MaxSendsPerUser { get; init; }

    /// <summary>
    /// Gonderimler arasi minimum sure (saat)
    /// </summary>
    [Range(0, 720)]
    public int? MinHoursBetweenSends { get; init; }

    /// <summary>
    /// A/B test aktif mi
    /// </summary>
    public bool AbTestEnabled { get; init; } = false;

    /// <summary>
    /// A/B test yuzdeleri
    /// </summary>
    [Range(0, 100)]
    public int? AbTestPercentage { get; init; }

    /// <summary>
    /// Gonderen adi
    /// </summary>
    [StringLength(100)]
    public string? SenderName { get; init; }

    /// <summary>
    /// Gonderen email
    /// </summary>
    [StringLength(200)]
    [EmailAddress]
    public string? SenderEmail { get; init; }

    /// <summary>
    /// Reply-to email
    /// </summary>
    [StringLength(200)]
    [EmailAddress]
    public string? ReplyToEmail { get; init; }

    /// <summary>
    /// Tracking aktif mi
    /// </summary>
    public bool TrackOpens { get; init; } = true;

    /// <summary>
    /// Click tracking aktif mi
    /// </summary>
    public bool TrackClicks { get; init; } = true;

    /// <summary>
    /// Unsubscribe link zorunlu mu
    /// </summary>
    public bool RequireUnsubscribeLink { get; init; } = true;

    /// <summary>
    /// Hedef segment ID
    /// </summary>
    public Guid? TargetSegmentId { get; init; }

    /// <summary>
    /// Haric tutulan segment ID
    /// </summary>
    public Guid? ExcludedSegmentId { get; init; }

    /// <summary>
    /// Gonderim saati
    /// </summary>
    public TimeSpan? PreferredSendTime { get; init; }

    /// <summary>
    /// Zaman dilimi
    /// </summary>
    [StringLength(50)]
    public string? Timezone { get; init; }

    /// <summary>
    /// Hafta sonu gonderim aktif mi
    /// </summary>
    public bool SendOnWeekends { get; init; } = false;
}
