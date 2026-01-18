using System.ComponentModel.DataAnnotations;
using Merge.Domain.Enums;
using Merge.Domain.Modules.Notifications;

namespace Merge.Application.DTOs.Notification;


public record CreateNotificationFromTemplateDto(
    [Required] Guid UserId,
    [Required] NotificationType TemplateType,
    NotificationVariablesDto? Variables = null);
