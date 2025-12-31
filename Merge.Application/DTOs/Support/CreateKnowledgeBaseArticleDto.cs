using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Support;

public class CreateKnowledgeBaseArticleDto
{
    [Required]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Başlık en az 2, en fazla 200 karakter olmalıdır.")]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50000, MinimumLength = 10, ErrorMessage = "İçerik en az 10, en fazla 50000 karakter olmalıdır.")]
    public string Content { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Excerpt { get; set; }
    
    public Guid? CategoryId { get; set; }
    
    [StringLength(50)]
    public string Status { get; set; } = "Draft";
    
    public bool IsFeatured { get; set; } = false;
    
    [Range(0, int.MaxValue)]
    public int DisplayOrder { get; set; } = 0;
    
    public List<string>? Tags { get; set; }
}
