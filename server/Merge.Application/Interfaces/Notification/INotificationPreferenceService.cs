using Merge.Application.DTOs.Notification;
using Merge.Domain.Modules.Notifications;

// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
namespace Merge.Application.Interfaces.Notification;

public interface INotificationPreferenceService
{
    Task<NotificationPreferenceDto> CreatePreferenceAsync(Guid userId, CreateNotificationPreferenceDto dto, CancellationToken cancellationToken = default);
    Task<NotificationPreferenceDto?> GetPreferenceAsync(Guid userId, string notificationType, string channel, CancellationToken cancellationToken = default);
    Task<IEnumerable<NotificationPreferenceDto>> GetUserPreferencesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<NotificationPreferenceSummaryDto> GetUserPreferencesSummaryAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<NotificationPreferenceDto> UpdatePreferenceAsync(Guid userId, string notificationType, string channel, UpdateNotificationPreferenceDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeletePreferenceAsync(Guid userId, string notificationType, string channel, CancellationToken cancellationToken = default);
    Task<bool> BulkUpdatePreferencesAsync(Guid userId, BulkUpdateNotificationPreferencesDto dto, CancellationToken cancellationToken = default);
    Task<bool> IsNotificationEnabledAsync(Guid userId, string notificationType, string channel, CancellationToken cancellationToken = default);
    Task<IEnumerable<string>> GetEnabledChannelsAsync(Guid userId, string notificationType, CancellationToken cancellationToken = default);
}

