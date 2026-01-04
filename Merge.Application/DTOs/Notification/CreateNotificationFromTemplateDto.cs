using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Notification;

public class CreateNotificationFromTemplateDto
{
    [Required]
    public Guid UserId { get; set; }
    
    [Required]
    [StringLength(50)]
    public string TemplateType { get; set; } = string.Empty;

    /// <summary>
    /// Template degiskenleri - Typed DTO (Over-posting korumasi)
    /// </summary>
    public NotificationVariablesDto? Variables { get; set; }
}
