using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Notification;

public class CreateNotificationFromTemplateDto
{
    [Required]
    public Guid UserId { get; set; }
    
    [Required]
    [StringLength(50)]
    public string TemplateType { get; set; } = string.Empty;
    
    public Dictionary<string, object>? Variables { get; set; }
}
