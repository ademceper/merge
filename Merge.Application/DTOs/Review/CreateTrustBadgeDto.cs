using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Review;

public class CreateTrustBadgeDto
{
    [Required]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "İsim en az 2, en fazla 100 karakter olmalıdır.")]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;
    
    [StringLength(500)]
    [Url(ErrorMessage = "Geçerli bir URL giriniz.")]
    public string IconUrl { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50)]
    public string BadgeType { get; set; } = string.Empty;
    
    public Dictionary<string, object>? Criteria { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    [Range(0, int.MaxValue)]
    public int DisplayOrder { get; set; } = 0;
    
    [StringLength(20)]
    public string? Color { get; set; }
}
