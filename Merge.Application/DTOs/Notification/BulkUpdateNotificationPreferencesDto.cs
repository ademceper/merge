using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Notification;

public class BulkUpdateNotificationPreferencesDto
{
    [Required]
    [MinLength(1, ErrorMessage = "En az bir tercih belirtilmelidir.")]
    public List<CreateNotificationPreferenceDto> Preferences { get; set; } = new();
}
