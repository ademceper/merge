using Merge.Domain.Enums;

namespace Merge.Application.DTOs.Seller;

// ✅ BOLUM 7.1.5: Records - DTO'lar record olarak tanımlanmalı (ZORUNLU)
// ✅ BOLUM 8.0: Over-posting Protection - init-only properties (ZORUNLU)
// ✅ ARCHITECTURE: Enum kullanımı (string Status yerine) - BEST_PRACTICES_ANALIZI.md BOLUM 1.1.6
public record CommissionPayoutDto
{
    public Guid Id { get; init; }
    public Guid SellerId { get; init; }
    public string SellerName { get; init; } = string.Empty;
    public string PayoutNumber { get; init; } = string.Empty;
    public decimal TotalAmount { get; init; }
    public decimal TransactionFee { get; init; }
    public decimal NetAmount { get; init; }
    // ✅ ARCHITECTURE: Enum kullanımı (string Status yerine) - BEST_PRACTICES_ANALIZI.md BOLUM 1.1.6
    public PayoutStatus Status { get; init; }
    public string PaymentMethod { get; init; } = string.Empty;
    public string? TransactionReference { get; init; }
    public DateTime? ProcessedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
    public string? Notes { get; init; }
    public DateTime CreatedAt { get; init; }
    public List<SellerCommissionDto> Commissions { get; init; } = new();
}
