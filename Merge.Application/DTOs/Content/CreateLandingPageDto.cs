using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Content;

public class CreateLandingPageDto
{
    [Required]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "İsim en az 2, en fazla 200 karakter olmalıdır.")]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Başlık en az 2, en fazla 200 karakter olmalıdır.")]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    [StringLength(10000, MinimumLength = 10, ErrorMessage = "İçerik en az 10, en fazla 10000 karakter olmalıdır.")]
    public string Content { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string? Template { get; set; }
    
    [StringLength(50)]
    public string Status { get; set; } = "Draft";
    
    public DateTime? StartDate { get; set; }
    
    public DateTime? EndDate { get; set; }
    
    [StringLength(200)]
    public string? MetaTitle { get; set; }
    
    [StringLength(500)]
    public string? MetaDescription { get; set; }
    
    [StringLength(500)]
    [Url(ErrorMessage = "Geçerli bir URL giriniz.")]
    public string? OgImageUrl { get; set; }
    
    public bool EnableABTesting { get; set; } = false;
    
    public Guid? VariantOfId { get; set; }
    
    [Range(0, 100, ErrorMessage = "Trafik bölünmesi 0 ile 100 arasında olmalıdır.")]
    public int TrafficSplit { get; set; } = 50;
}
