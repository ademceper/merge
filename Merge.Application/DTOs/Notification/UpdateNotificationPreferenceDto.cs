using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Notification;

public class UpdateNotificationPreferenceDto
{
    public bool? IsEnabled { get; set; }
    
    public Dictionary<string, object>? CustomSettings { get; set; }
}
