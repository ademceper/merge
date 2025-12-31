using Merge.Application.DTOs.Notification;

namespace Merge.Application.Interfaces.Notification;

public interface INotificationPreferenceService
{
    Task<NotificationPreferenceDto> CreatePreferenceAsync(Guid userId, CreateNotificationPreferenceDto dto);
    Task<NotificationPreferenceDto?> GetPreferenceAsync(Guid userId, string notificationType, string channel);
    Task<IEnumerable<NotificationPreferenceDto>> GetUserPreferencesAsync(Guid userId);
    Task<NotificationPreferenceSummaryDto> GetUserPreferencesSummaryAsync(Guid userId);
    Task<NotificationPreferenceDto> UpdatePreferenceAsync(Guid userId, string notificationType, string channel, UpdateNotificationPreferenceDto dto);
    Task<bool> DeletePreferenceAsync(Guid userId, string notificationType, string channel);
    Task<bool> BulkUpdatePreferencesAsync(Guid userId, BulkUpdateNotificationPreferencesDto dto);
    Task<bool> IsNotificationEnabledAsync(Guid userId, string notificationType, string channel);
    Task<IEnumerable<string>> GetEnabledChannelsAsync(Guid userId, string notificationType);
}

