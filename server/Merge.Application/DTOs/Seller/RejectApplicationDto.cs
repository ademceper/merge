using System.ComponentModel.DataAnnotations;
using Merge.Domain.Entities;

namespace Merge.Application.DTOs.Seller;

// ✅ BOLUM 7.1.5: Records - DTO'lar record olarak tanımlanmalı (ZORUNLU)
// ✅ BOLUM 8.0: Over-posting Protection - init-only properties (ZORUNLU)
public record RejectApplicationDto
{
    [Required]
    [StringLength(1000, MinimumLength = 5, ErrorMessage = "Red nedeni en az 5, en fazla 1000 karakter olmalıdır.")]
    public string Reason { get; init; } = string.Empty;
}
