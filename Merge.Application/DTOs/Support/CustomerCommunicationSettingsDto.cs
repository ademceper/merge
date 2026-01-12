using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Support;

/// <summary>
/// Customer communication ayarlari icin typed DTO - Dictionary yerine guvenli
/// ✅ BOLUM 7.1.5: Records - DTO'lar record olmalı (ZORUNLU)
/// </summary>
public record CustomerCommunicationSettingsDto
{
    /// <summary>
    /// Iletisim kanali
    /// </summary>
    [StringLength(50)]
    public string? Channel { get; init; }

    /// <summary>
    /// Oncelik seviyesi
    /// </summary>
    [StringLength(20)]
    public string? Priority { get; init; }

    /// <summary>
    /// Otomatik yanit aktif mi
    /// </summary>
    public bool AutoReplyEnabled { get; init; } = false;

    /// <summary>
    /// Otomatik yanit sablon ID
    /// </summary>
    public Guid? AutoReplyTemplateId { get; init; }

    /// <summary>
    /// SLA suresi (saat)
    /// </summary>
    [Range(0, 720)]
    public int? SlaHours { get; init; }

    /// <summary>
    /// Eskalasyon suresi (saat)
    /// </summary>
    [Range(0, 720)]
    public int? EscalationHours { get; init; }

    /// <summary>
    /// Eskalasyon email adresi
    /// </summary>
    [StringLength(200)]
    [EmailAddress]
    public string? EscalationEmail { get; init; }

    /// <summary>
    /// Kategori
    /// </summary>
    [StringLength(100)]
    public string? Category { get; init; }

    /// <summary>
    /// Alt kategori
    /// </summary>
    [StringLength(100)]
    public string? SubCategory { get; init; }

    /// <summary>
    /// Atanan ekip
    /// </summary>
    [StringLength(100)]
    public string? AssignedTeam { get; init; }

    /// <summary>
    /// Etiketler (virgul ile ayrilmis)
    /// </summary>
    [StringLength(500)]
    public string? Tags { get; init; }

    /// <summary>
    /// Dahili notlar
    /// </summary>
    [StringLength(2000)]
    public string? InternalNotes { get; init; }

    /// <summary>
    /// Musteriye bildirim gonder
    /// </summary>
    public bool NotifyCustomer { get; init; } = true;

    /// <summary>
    /// Takip gerekli mi
    /// </summary>
    public bool RequiresFollowUp { get; init; } = false;

    /// <summary>
    /// Takip tarihi
    /// </summary>
    public DateTime? FollowUpDate { get; init; }
}
