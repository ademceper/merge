using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Support;

/// <summary>
/// ✅ BOLUM 7.1.5: Records - DTO'lar record olmalı (ZORUNLU)
/// </summary>
public record CreateKnowledgeBaseCategoryDto
{
    [Required]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Kategori adı en az 2, en fazla 100 karakter olmalıdır.")]
    public string Name { get; init; } = string.Empty;
    
    // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma - DTO validation matches SupportSettings.MaxCategoryDescriptionLength=1000
    [StringLength(1000)]
    public string? Description { get; init; }
    
    public Guid? ParentCategoryId { get; init; }
    
    [Range(0, int.MaxValue)]
    public int DisplayOrder { get; init; } = 0;
    
    public bool IsActive { get; init; } = true;
    
    [StringLength(500)]
    [Url(ErrorMessage = "Geçerli bir URL giriniz.")]
    public string? IconUrl { get; init; }
}
