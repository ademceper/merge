using Merge.Domain.Enums;

namespace Merge.Application.DTOs.Seller;

// ✅ BOLUM 7.1.5: Records - DTO'lar record olarak tanımlanmalı (ZORUNLU)
// ✅ BOLUM 8.0: Over-posting Protection - init-only properties (ZORUNLU)
// ✅ ARCHITECTURE: Enum kullanımı (string Status yerine) - BEST_PRACTICES_ANALIZI.md BOLUM 1.1.6
public record SellerInvoiceDto
{
    public Guid Id { get; init; }
    public Guid SellerId { get; init; }
    public string SellerName { get; init; } = string.Empty;
    public string InvoiceNumber { get; init; } = string.Empty;
    public DateTime InvoiceDate { get; init; }
    public DateTime PeriodStart { get; init; }
    public DateTime PeriodEnd { get; init; }
    public decimal TotalEarnings { get; init; }
    public decimal TotalCommissions { get; init; }
    public decimal TotalPayouts { get; init; }
    public decimal PlatformFees { get; init; }
    public decimal NetAmount { get; init; }
    // ✅ ARCHITECTURE: Enum kullanımı (string Status yerine) - BEST_PRACTICES_ANALIZI.md BOLUM 1.1.6
    public SellerInvoiceStatus Status { get; init; }
    public DateTime? PaidAt { get; init; }
    public List<InvoiceItemDto> Items { get; set; } = new();
    public DateTime CreatedAt { get; init; }
}
