using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Notification;

public class CreateNotificationTemplateDto
{
    [Required]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "İsim en az 2, en fazla 100 karakter olmalıdır.")]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50)]
    public string Type { get; set; } = string.Empty;
    
    [Required]
    [StringLength(200)]
    public string TitleTemplate { get; set; } = string.Empty;
    
    [Required]
    [StringLength(2000)]
    public string MessageTemplate { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? LinkTemplate { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public Dictionary<string, string>? Variables { get; set; }
    
    public Dictionary<string, object>? DefaultData { get; set; }
}
