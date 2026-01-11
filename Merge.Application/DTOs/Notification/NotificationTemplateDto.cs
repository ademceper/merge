using Merge.Domain.Enums;

namespace Merge.Application.DTOs.Notification;

/// <summary>
/// Notification Template DTO - BOLUM 7.1.5: Records (C# 12 modern features)
/// </summary>
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
