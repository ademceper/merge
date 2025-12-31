using Merge.Application.DTOs.Notification;

namespace Merge.Application.Interfaces.Notification;

public interface INotificationTemplateService
{
    Task<NotificationTemplateDto> CreateTemplateAsync(CreateNotificationTemplateDto dto);
    Task<NotificationTemplateDto?> GetTemplateAsync(Guid id);
    Task<NotificationTemplateDto?> GetTemplateByTypeAsync(string type);
    Task<IEnumerable<NotificationTemplateDto>> GetTemplatesAsync(string? type = null);
    Task<NotificationTemplateDto> UpdateTemplateAsync(Guid id, UpdateNotificationTemplateDto dto);
    Task<bool> DeleteTemplateAsync(Guid id);
    Task<NotificationDto> CreateNotificationFromTemplateAsync(Guid userId, string templateType, Dictionary<string, object>? variables = null);
}

