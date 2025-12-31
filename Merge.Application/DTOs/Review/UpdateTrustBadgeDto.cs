using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Review;

public class UpdateTrustBadgeDto
{
    [StringLength(100, MinimumLength = 2, ErrorMessage = "İsim en az 2, en fazla 100 karakter olmalıdır.")]
    public string? Name { get; set; }
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [StringLength(500)]
    [Url(ErrorMessage = "Geçerli bir URL giriniz.")]
    public string? IconUrl { get; set; }
    
    [StringLength(50)]
    public string? BadgeType { get; set; }
    
    public Dictionary<string, object>? Criteria { get; set; }
    
    public bool? IsActive { get; set; }
    
    [Range(0, int.MaxValue)]
    public int? DisplayOrder { get; set; }
    
    [StringLength(20)]
    public string? Color { get; set; }
}
