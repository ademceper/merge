using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.International;

/// <summary>
/// Partial update DTO for Product Translation (PATCH support)
/// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
/// </summary>
public record PatchProductTranslationDto
{
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Ürün adı en az 2, en fazla 200 karakter olmalıdır.")]
    public string? Name { get; init; }
    
    [StringLength(5000)]
    public string? Description { get; init; }
    
    [StringLength(500)]
    public string? ShortDescription { get; init; }
    
    [StringLength(200)]
    public string? MetaTitle { get; init; }
    
    [StringLength(500)]
    public string? MetaDescription { get; init; }
    
    [StringLength(200)]
    public string? MetaKeywords { get; init; }
}
