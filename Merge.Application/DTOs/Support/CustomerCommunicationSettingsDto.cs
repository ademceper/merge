using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Support;

/// <summary>
/// Customer communication ayarlari icin typed DTO - Dictionary yerine guvenli
/// </summary>
public class CustomerCommunicationSettingsDto
{
    /// <summary>
    /// Iletisim kanali
    /// </summary>
    [StringLength(50)]
    public string? Channel { get; set; }

    /// <summary>
    /// Oncelik seviyesi
    /// </summary>
    [StringLength(20)]
    public string? Priority { get; set; }

    /// <summary>
    /// Otomatik yanit aktif mi
    /// </summary>
    public bool AutoReplyEnabled { get; set; } = false;

    /// <summary>
    /// Otomatik yanit sablon ID
    /// </summary>
    public Guid? AutoReplyTemplateId { get; set; }

    /// <summary>
    /// SLA suresi (saat)
    /// </summary>
    [Range(0, 720)]
    public int? SlaHours { get; set; }

    /// <summary>
    /// Eskalasyon suresi (saat)
    /// </summary>
    [Range(0, 720)]
    public int? EscalationHours { get; set; }

    /// <summary>
    /// Eskalasyon email adresi
    /// </summary>
    [StringLength(200)]
    [EmailAddress]
    public string? EscalationEmail { get; set; }

    /// <summary>
    /// Kategori
    /// </summary>
    [StringLength(100)]
    public string? Category { get; set; }

    /// <summary>
    /// Alt kategori
    /// </summary>
    [StringLength(100)]
    public string? SubCategory { get; set; }

    /// <summary>
    /// Atanan ekip
    /// </summary>
    [StringLength(100)]
    public string? AssignedTeam { get; set; }

    /// <summary>
    /// Etiketler (virgul ile ayrilmis)
    /// </summary>
    [StringLength(500)]
    public string? Tags { get; set; }

    /// <summary>
    /// Dahili notlar
    /// </summary>
    [StringLength(2000)]
    public string? InternalNotes { get; set; }

    /// <summary>
    /// Musteriye bildirim gonder
    /// </summary>
    public bool NotifyCustomer { get; set; } = true;

    /// <summary>
    /// Takip gerekli mi
    /// </summary>
    public bool RequiresFollowUp { get; set; } = false;

    /// <summary>
    /// Takip tarihi
    /// </summary>
    public DateTime? FollowUpDate { get; set; }
}
