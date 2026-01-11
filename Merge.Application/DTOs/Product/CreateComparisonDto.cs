using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Product;

// ✅ BOLUM 7.1.5: Records - DTO'lar record olmalı (ZORUNLU)
public record CreateComparisonDto(
    [StringLength(200)] string? Name,
    [Required]
    [MinLength(2, ErrorMessage = "En az 2 ürün karşılaştırılmalıdır.")]
    IReadOnlyList<Guid> ProductIds
);
