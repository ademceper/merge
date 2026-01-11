using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Seller;

// ✅ BOLUM 7.1.5: Records - DTO'lar record olarak tanımlanmalı (ZORUNLU)
// ✅ BOLUM 8.0: Over-posting Protection - init-only properties (ZORUNLU)
public record UpdateCommissionSettingsDto
{
    [Range(0, 100, ErrorMessage = "Komisyon oranı 0 ile 100 arasında olmalıdır.")]
    public decimal? CustomCommissionRate { get; init; }
    
    public bool? UseCustomRate { get; init; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Minimum ödeme tutarı 0 veya daha büyük olmalıdır.")]
    public decimal? MinimumPayoutAmount { get; init; }
    
    [StringLength(50)]
    public string? PaymentMethod { get; init; }
    
    [StringLength(500)]
    public string? PaymentDetails { get; init; }
}
