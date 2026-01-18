using System.ComponentModel.DataAnnotations;
using Merge.Domain.Enums;
using Merge.Domain.Modules.Notifications;

namespace Merge.Application.DTOs.Notification;


public record CreateNotificationTemplateDto(
    [Required]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "İsim en az 2, en fazla 100 karakter olmalıdır.")]
    string Name,
    [Required] NotificationType Type,
    [Required]
    [StringLength(200)]
    string TitleTemplate,
    [Required]
    [StringLength(2000)]
    string MessageTemplate,
    [StringLength(500)] string? Description = null,
    [StringLength(500)] string? LinkTemplate = null,
    bool IsActive = true,
    NotificationVariablesDto? Variables = null,
    NotificationTemplateSettingsDto? DefaultData = null);
