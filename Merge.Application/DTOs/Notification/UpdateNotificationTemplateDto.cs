using System.ComponentModel.DataAnnotations;
using Merge.Domain.Enums;

namespace Merge.Application.DTOs.Notification;

/// <summary>
/// Update Notification Template DTO - BOLUM 7.1.5: Records (C# 12 modern features)
/// </summary>
public record UpdateNotificationTemplateDto(
    [StringLength(100, MinimumLength = 2, ErrorMessage = "İsim en az 2, en fazla 100 karakter olmalıdır.")]
    string? Name = null,
    [StringLength(500)] string? Description = null,
    NotificationType? Type = null,
    [StringLength(200)] string? TitleTemplate = null,
    [StringLength(2000)] string? MessageTemplate = null,
    [StringLength(500)] string? LinkTemplate = null,
    bool? IsActive = null,
    NotificationVariablesDto? Variables = null,
    NotificationTemplateSettingsDto? DefaultData = null);
