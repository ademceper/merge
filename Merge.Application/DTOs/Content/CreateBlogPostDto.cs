using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Content;

public class CreateBlogPostDto
{
    [Required]
    public Guid CategoryId { get; set; }
    
    [Required]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Başlık en az 2, en fazla 200 karakter olmalıdır.")]
    public string Title { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string Excerpt { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50000, MinimumLength = 10, ErrorMessage = "İçerik en az 10, en fazla 50000 karakter olmalıdır.")]
    public string Content { get; set; } = string.Empty;
    
    [StringLength(500)]
    [Url(ErrorMessage = "Geçerli bir URL giriniz.")]
    public string? FeaturedImageUrl { get; set; }
    
    [StringLength(50)]
    public string Status { get; set; } = "Draft";
    
    public List<string>? Tags { get; set; }
    
    public bool IsFeatured { get; set; } = false;
    
    public bool AllowComments { get; set; } = true;
    
    [StringLength(200)]
    public string? MetaTitle { get; set; }
    
    [StringLength(500)]
    public string? MetaDescription { get; set; }
    
    [StringLength(200)]
    public string? MetaKeywords { get; set; }
    
    [StringLength(500)]
    [Url(ErrorMessage = "Geçerli bir URL giriniz.")]
    public string? OgImageUrl { get; set; }
}
