using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Security;

public class CreateSecurityAlertDto
{
    public Guid? UserId { get; set; }
    
    [Required]
    [StringLength(100)]
    public string AlertType { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50)]
    public string Severity { get; set; } = "Medium";
    
    [Required]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Başlık en az 2, en fazla 200 karakter olmalıdır.")]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    [StringLength(2000, MinimumLength = 5, ErrorMessage = "Açıklama en az 5, en fazla 2000 karakter olmalıdır.")]
    public string Description { get; set; } = string.Empty;
    
    public Dictionary<string, object>? Metadata { get; set; }
}
