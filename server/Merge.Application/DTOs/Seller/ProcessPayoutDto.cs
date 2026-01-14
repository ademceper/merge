using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Seller;

// ✅ BOLUM 7.1.5: Records - DTO'lar record olarak tanımlanmalı (ZORUNLU)
// ✅ BOLUM 8.0: Over-posting Protection - init-only properties (ZORUNLU)
public record ProcessPayoutDto
{
    [Required]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "İşlem referansı gereklidir.")]
    public string TransactionReference { get; init; } = string.Empty;
}
