using Merge.Domain.Enums;
using Merge.Domain.Modules.Notifications;
namespace Merge.Application.DTOs.Notification;


public record NotificationPreferenceSummaryDto(
    Guid UserId,
    Dictionary<string, Dictionary<string, bool>> Preferences, // NotificationType -> Channel -> IsEnabled
    int TotalEnabled,
    int TotalDisabled);
