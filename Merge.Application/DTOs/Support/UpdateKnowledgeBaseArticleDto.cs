using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Support;

public class UpdateKnowledgeBaseArticleDto
{
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Başlık en az 2, en fazla 200 karakter olmalıdır.")]
    public string? Title { get; set; }
    
    [StringLength(50000)]
    public string? Content { get; set; }
    
    [StringLength(500)]
    public string? Excerpt { get; set; }
    
    public Guid? CategoryId { get; set; }
    
    [StringLength(50)]
    public string? Status { get; set; }
    
    public bool? IsFeatured { get; set; }
    
    [Range(0, int.MaxValue)]
    public int? DisplayOrder { get; set; }
    
    public List<string>? Tags { get; set; }
}
