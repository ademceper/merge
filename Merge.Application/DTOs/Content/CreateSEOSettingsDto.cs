using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Content;

public class CreateSEOSettingsDto
{
    [Required]
    [StringLength(50)]
    public string PageType { get; set; } = string.Empty;
    
    public Guid? EntityId { get; set; }
    
    [StringLength(200)]
    public string? MetaTitle { get; set; }
    
    [StringLength(500)]
    public string? MetaDescription { get; set; }
    
    [StringLength(200)]
    public string? MetaKeywords { get; set; }
    
    [StringLength(500)]
    [Url(ErrorMessage = "Geçerli bir URL giriniz.")]
    public string? CanonicalUrl { get; set; }
    
    [StringLength(200)]
    public string? OgTitle { get; set; }
    
    [StringLength(500)]
    public string? OgDescription { get; set; }
    
    [StringLength(500)]
    [Url(ErrorMessage = "Geçerli bir URL giriniz.")]
    public string? OgImageUrl { get; set; }
    
    [StringLength(50)]
    public string? TwitterCard { get; set; }
    
    public Dictionary<string, object>? StructuredData { get; set; }
    
    public bool IsIndexed { get; set; } = true;
    
    public bool FollowLinks { get; set; } = true;
    
    [Range(0, 1, ErrorMessage = "Öncelik 0 ile 1 arasında olmalıdır.")]
    public decimal Priority { get; set; } = 0.5m;
    
    [StringLength(50)]
    public string? ChangeFrequency { get; set; } = "weekly";
}
