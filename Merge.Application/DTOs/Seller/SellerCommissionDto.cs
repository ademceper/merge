using Merge.Domain.Enums;

namespace Merge.Application.DTOs.Seller;

// ✅ BOLUM 7.1.5: Records - DTO'lar record olarak tanımlanmalı (ZORUNLU)
// ✅ BOLUM 8.0: Over-posting Protection - init-only properties (ZORUNLU)
// ✅ ARCHITECTURE: Enum kullanımı (string Status yerine) - BEST_PRACTICES_ANALIZI.md BOLUM 1.1.6
public record SellerCommissionDto
{
    public Guid Id { get; init; }
    public Guid SellerId { get; init; }
    public string SellerName { get; init; } = string.Empty;
    public Guid OrderId { get; init; }
    public string OrderNumber { get; init; } = string.Empty;
    public decimal OrderAmount { get; init; }
    public decimal CommissionRate { get; init; }
    public decimal CommissionAmount { get; init; }
    public decimal PlatformFee { get; init; }
    public decimal NetAmount { get; init; }
    // ✅ ARCHITECTURE: Enum kullanımı (string Status yerine) - BEST_PRACTICES_ANALIZI.md BOLUM 1.1.6
    public CommissionStatus Status { get; init; }
    public DateTime? ApprovedAt { get; init; }
    public DateTime? PaidAt { get; init; }
    public string? PaymentReference { get; init; }
    public DateTime CreatedAt { get; init; }
}
