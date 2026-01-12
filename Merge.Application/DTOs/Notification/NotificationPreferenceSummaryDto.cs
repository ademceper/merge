using Merge.Domain.Enums;
using Merge.Domain.Modules.Notifications;
namespace Merge.Application.DTOs.Notification;

/// <summary>
/// Notification Preference Summary DTO - BOLUM 7.1.5: Records (C# 12 modern features)
/// </summary>
public record NotificationPreferenceSummaryDto(
    Guid UserId,
    Dictionary<string, Dictionary<string, bool>> Preferences, // NotificationType -> Channel -> IsEnabled
    int TotalEnabled,
    int TotalDisabled);
