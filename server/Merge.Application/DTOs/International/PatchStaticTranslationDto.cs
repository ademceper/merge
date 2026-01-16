using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.International;

/// <summary>
/// Partial update DTO for Static Translation (PATCH support)
/// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
/// </summary>
public record PatchStaticTranslationDto
{
    [StringLength(5000, MinimumLength = 1, ErrorMessage = "Değer gereklidir ve en fazla 5000 karakter olmalıdır.")]
    public string? Value { get; init; }
    
    [StringLength(100)]
    public string? Category { get; init; }
}
