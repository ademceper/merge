using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Support;

/// <summary>
/// ✅ BOLUM 7.1.5: Records - DTO'lar record olmalı (ZORUNLU)
/// </summary>
public record UpdateKnowledgeBaseArticleDto
{
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Başlık en az 2, en fazla 200 karakter olmalıdır.")]
    public string? Title { get; init; }
    
    [StringLength(50000)]
    public string? Content { get; init; }
    
    [StringLength(500)]
    public string? Excerpt { get; init; }
    
    public Guid? CategoryId { get; init; }
    
    [StringLength(50)]
    public string? Status { get; init; }
    
    public bool? IsFeatured { get; init; }
    
    [Range(0, int.MaxValue)]
    public int? DisplayOrder { get; init; }
    
    public IReadOnlyList<string>? Tags { get; init; }
}
