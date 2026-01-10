namespace Merge.Application.DTOs.Marketing;

/// <summary>
/// Email Automation DTO - BOLUM 1.0: DTO Dosya Organizasyonu (ZORUNLU)
/// </summary>
public record EmailAutomationDto(
    Guid Id,
    string Name,
    string Description,
    string Type,
    bool IsActive,
    Guid TemplateId,
    string TemplateName,
    int DelayHours,
    int TotalTriggered,
    int TotalSent,
    DateTime? LastTriggeredAt);
