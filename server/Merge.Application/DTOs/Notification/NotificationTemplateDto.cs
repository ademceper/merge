using Merge.Domain.Enums;
using Merge.Domain.Modules.Notifications;

namespace Merge.Application.DTOs.Notification;


public record NotificationTemplateDto(
    Guid Id,
    string Name,
    string Description,
    NotificationType Type,
    string TitleTemplate,
    string MessageTemplate,
    string? LinkTemplate,
    bool IsActive,
    NotificationVariablesDto? Variables,
    NotificationTemplateSettingsDto? DefaultData,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
