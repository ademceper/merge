using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Support;

/// <summary>
/// ✅ BOLUM 7.1.5: Records - DTO'lar record olmalı (ZORUNLU)
/// </summary>
public record CreateKnowledgeBaseArticleDto
{
    [Required]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Başlık en az 2, en fazla 200 karakter olmalıdır.")]
    public string Title { get; init; } = string.Empty;
    
    [Required]
    [StringLength(50000, MinimumLength = 10, ErrorMessage = "İçerik en az 10, en fazla 50000 karakter olmalıdır.")]
    public string Content { get; init; } = string.Empty;
    
    [StringLength(500)]
    public string? Excerpt { get; init; }
    
    public Guid? CategoryId { get; init; }
    
    [StringLength(50)]
    public string Status { get; init; } = "Draft";
    
    public bool IsFeatured { get; init; } = false;
    
    [Range(0, int.MaxValue)]
    public int DisplayOrder { get; init; } = 0;
    
    public IReadOnlyList<string>? Tags { get; init; }
}
