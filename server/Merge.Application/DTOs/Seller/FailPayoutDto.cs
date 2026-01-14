using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Seller;

// ✅ BOLUM 7.1.5: Records - DTO'lar record olarak tanımlanmalı (ZORUNLU)
// ✅ BOLUM 8.0: Over-posting Protection - init-only properties (ZORUNLU)
public record FailPayoutDto
{
    [Required]
    [StringLength(1000, MinimumLength = 5, ErrorMessage = "Başarısızlık nedeni en az 5, en fazla 1000 karakter olmalıdır.")]
    public string Reason { get; init; } = string.Empty;
}
