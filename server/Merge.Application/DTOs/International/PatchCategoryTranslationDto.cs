using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.International;

/// <summary>
/// Partial update DTO for Category Translation (PATCH support)
/// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
/// </summary>
public record PatchCategoryTranslationDto
{
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Kategori adı en az 2, en fazla 200 karakter olmalıdır.")]
    public string? Name { get; init; }
    
    [StringLength(2000)]
    public string? Description { get; init; }
}
