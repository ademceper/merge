using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Notification;

public class UpdateNotificationTemplateDto
{
    [StringLength(100, MinimumLength = 2, ErrorMessage = "İsim en az 2, en fazla 100 karakter olmalıdır.")]
    public string? Name { get; set; }
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [StringLength(50)]
    public string? Type { get; set; }
    
    [StringLength(200)]
    public string? TitleTemplate { get; set; }
    
    [StringLength(2000)]
    public string? MessageTemplate { get; set; }
    
    [StringLength(500)]
    public string? LinkTemplate { get; set; }
    
    public bool? IsActive { get; set; }

    /// <summary>
    /// Template degiskenleri - Typed DTO (Over-posting korumasi)
    /// </summary>
    public NotificationVariablesDto? Variables { get; set; }

    /// <summary>
    /// Template ayarlari - Typed DTO (Over-posting korumasi)
    /// </summary>
    public NotificationTemplateSettingsDto? DefaultData { get; set; }
}
