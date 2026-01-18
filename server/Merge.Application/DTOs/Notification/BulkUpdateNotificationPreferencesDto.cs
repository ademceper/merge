using System.ComponentModel.DataAnnotations;
using Merge.Domain.Modules.Notifications;

namespace Merge.Application.DTOs.Notification;


public record BulkUpdateNotificationPreferencesDto(
    [Required]
    [MinLength(1, ErrorMessage = "En az bir tercih belirtilmelidir.")]
    List<CreateNotificationPreferenceDto> Preferences);
