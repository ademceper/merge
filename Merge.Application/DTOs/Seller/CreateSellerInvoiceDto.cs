using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Seller;

// ✅ BOLUM 7.1.5: Records - DTO'lar record olarak tanımlanmalı (ZORUNLU)
// ✅ BOLUM 8.0: Over-posting Protection - init-only properties (ZORUNLU)
public record CreateSellerInvoiceDto
{
    [Required]
    public Guid SellerId { get; init; }
    
    [Required]
    public DateTime PeriodStart { get; init; }
    
    [Required]
    public DateTime PeriodEnd { get; init; }
    
    [StringLength(2000)]
    public string? Notes { get; init; }
}
