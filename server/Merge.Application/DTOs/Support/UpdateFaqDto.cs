using System.ComponentModel.DataAnnotations;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.DTOs.Support;

/// <summary>
/// ✅ BOLUM 7.1.5: Records - DTO'lar record olmalı (ZORUNLU)
/// </summary>
public record UpdateFaqDto
{
    [Required]
    [StringLength(500, MinimumLength = 5, ErrorMessage = "Soru en az 5, en fazla 500 karakter olmalıdır.")]
    public string Question { get; init; } = string.Empty;
    
    [Required]
    // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma - DTO validation matches SupportSettings.MaxFaqAnswerLength=5000
    [StringLength(5000, MinimumLength = 5, ErrorMessage = "Cevap en az 5, en fazla 5000 karakter olmalıdır.")]
    public string Answer { get; init; } = string.Empty;
    
    // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma - DTO validation matches SupportSettings.MaxFaqCategoryLength=50
    [StringLength(50)]
    public string Category { get; init; } = "General";
    
    [Range(0, int.MaxValue)]
    public int SortOrder { get; init; } = 0;
    
    public bool IsPublished { get; init; } = true;
}
