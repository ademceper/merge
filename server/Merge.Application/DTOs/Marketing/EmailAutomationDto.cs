using Merge.Domain.ValueObjects;
namespace Merge.Application.DTOs.Marketing;


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
