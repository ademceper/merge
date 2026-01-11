using System.ComponentModel.DataAnnotations;
using Merge.Domain.Enums;

namespace Merge.Application.DTOs.Notification;

/// <summary>
/// Create Notification Preference DTO - BOLUM 7.1.5: Records (C# 12 modern features)
/// </summary>
public record CreateNotificationPreferenceDto(
    [Required] NotificationType NotificationType,
    [Required] NotificationChannel Channel,
    bool IsEnabled = true,
    NotificationPreferenceSettingsDto? CustomSettings = null);
