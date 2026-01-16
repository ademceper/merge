using Merge.Domain.Enums;
using Merge.Domain.Modules.Notifications;

namespace Merge.Application.DTOs.Notification;

/// <summary>
/// Partial update DTO for Notification Template (PATCH support)
/// All fields are optional for partial updates
/// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
/// </summary>
public record PatchNotificationTemplateDto
{
    public string? Name { get; init; }
    public string? Description { get; init; }
    public NotificationType? Type { get; init; }
    public string? TitleTemplate { get; init; }
    public string? MessageTemplate { get; init; }
    public string? LinkTemplate { get; init; }
    public bool? IsActive { get; init; }
    public NotificationVariablesDto? Variables { get; init; }
    public NotificationTemplateSettingsDto? DefaultData { get; init; }
}
