using Merge.Domain.Enums;

namespace Merge.Domain.Entities;

/// <summary>
/// EmailAutomation Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class EmailAutomation : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public EmailAutomationType Type { get; set; } = EmailAutomationType.WelcomeSeries;
    public bool IsActive { get; set; } = true;
    public Guid TemplateId { get; set; }
    public EmailTemplate Template { get; set; } = null!;
    public int DelayHours { get; set; } = 0; // Delay before sending
    public string? TriggerConditions { get; set; } // JSON object defining when to trigger
    public int TotalTriggered { get; set; } = 0;
    public int TotalSent { get; set; } = 0;
    public DateTime? LastTriggeredAt { get; set; }
}

