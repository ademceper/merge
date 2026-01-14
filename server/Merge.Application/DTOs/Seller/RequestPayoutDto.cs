using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Seller;

// ✅ BOLUM 7.1.5: Records - DTO'lar record olarak tanımlanmalı (ZORUNLU)
// ✅ BOLUM 8.0: Over-posting Protection - init-only properties (ZORUNLU)
public record RequestPayoutDto
{
    [Required]
    [MinLength(1, ErrorMessage = "En az bir komisyon seçilmelidir.")]
    public List<Guid> CommissionIds { get; init; } = new();
}
