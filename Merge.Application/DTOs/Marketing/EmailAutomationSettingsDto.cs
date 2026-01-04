using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Marketing;

/// <summary>
/// Email automation ayarlari icin typed DTO - Dictionary yerine guvenli
/// </summary>
public class EmailAutomationSettingsDto
{
    /// <summary>
    /// Otomasyon aktif mi
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Trigger tipi
    /// </summary>
    [StringLength(50)]
    public string? TriggerType { get; set; }

    /// <summary>
    /// Gecikme suresi (dakika)
    /// </summary>
    [Range(0, 43200)]
    public int? DelayMinutes { get; set; }

    /// <summary>
    /// Bir kullaniciya maksimum gonderim sayisi
    /// </summary>
    [Range(1, 100)]
    public int? MaxSendsPerUser { get; set; }

    /// <summary>
    /// Gonderimler arasi minimum sure (saat)
    /// </summary>
    [Range(0, 720)]
    public int? MinHoursBetweenSends { get; set; }

    /// <summary>
    /// A/B test aktif mi
    /// </summary>
    public bool AbTestEnabled { get; set; } = false;

    /// <summary>
    /// A/B test yuzdeleri
    /// </summary>
    [Range(0, 100)]
    public int? AbTestPercentage { get; set; }

    /// <summary>
    /// Gonderen adi
    /// </summary>
    [StringLength(100)]
    public string? SenderName { get; set; }

    /// <summary>
    /// Gonderen email
    /// </summary>
    [StringLength(200)]
    [EmailAddress]
    public string? SenderEmail { get; set; }

    /// <summary>
    /// Reply-to email
    /// </summary>
    [StringLength(200)]
    [EmailAddress]
    public string? ReplyToEmail { get; set; }

    /// <summary>
    /// Tracking aktif mi
    /// </summary>
    public bool TrackOpens { get; set; } = true;

    /// <summary>
    /// Click tracking aktif mi
    /// </summary>
    public bool TrackClicks { get; set; } = true;

    /// <summary>
    /// Unsubscribe link zorunlu mu
    /// </summary>
    public bool RequireUnsubscribeLink { get; set; } = true;

    /// <summary>
    /// Hedef segment ID
    /// </summary>
    public Guid? TargetSegmentId { get; set; }

    /// <summary>
    /// Haric tutulan segment ID
    /// </summary>
    public Guid? ExcludedSegmentId { get; set; }

    /// <summary>
    /// Gonderim saati
    /// </summary>
    public TimeSpan? PreferredSendTime { get; set; }

    /// <summary>
    /// Zaman dilimi
    /// </summary>
    [StringLength(50)]
    public string? Timezone { get; set; }

    /// <summary>
    /// Hafta sonu gonderim aktif mi
    /// </summary>
    public bool SendOnWeekends { get; set; } = false;
}
