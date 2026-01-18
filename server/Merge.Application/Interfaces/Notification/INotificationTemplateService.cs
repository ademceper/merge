using Merge.Application.DTOs.Notification;
using Merge.Domain.Modules.Notifications;

namespace Merge.Application.Interfaces.Notification;

public interface INotificationTemplateService
{
    Task<NotificationTemplateDto> CreateTemplateAsync(CreateNotificationTemplateDto dto, CancellationToken cancellationToken = default);
    Task<NotificationTemplateDto?> GetTemplateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<NotificationTemplateDto?> GetTemplateByTypeAsync(string type, CancellationToken cancellationToken = default);
    Task<IEnumerable<NotificationTemplateDto>> GetTemplatesAsync(string? type = null, CancellationToken cancellationToken = default);
    Task<NotificationTemplateDto> UpdateTemplateAsync(Guid id, UpdateNotificationTemplateDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteTemplateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<NotificationDto> CreateNotificationFromTemplateAsync(Guid userId, string templateType, Dictionary<string, object>? variables = null, CancellationToken cancellationToken = default);
}

