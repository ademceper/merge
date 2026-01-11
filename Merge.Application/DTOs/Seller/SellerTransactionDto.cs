using Merge.Domain.Enums;

namespace Merge.Application.DTOs.Seller;

// ✅ BOLUM 7.1.5: Records - DTO'lar record olarak tanımlanmalı (ZORUNLU)
// ✅ BOLUM 8.0: Over-posting Protection - init-only properties (ZORUNLU)
// ✅ ARCHITECTURE: Enum kullanımı (string TransactionType yerine) - BEST_PRACTICES_ANALIZI.md BOLUM 1.1.6
public record SellerTransactionDto
{
    public Guid Id { get; init; }
    public Guid SellerId { get; init; }
    public SellerTransactionType TransactionType { get; init; }
    public string Description { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public decimal BalanceBefore { get; init; }
    public decimal BalanceAfter { get; init; }
    public Guid? RelatedEntityId { get; init; } // CommissionId, PayoutId, OrderId
    public string? RelatedEntityType { get; init; }
    public FinanceTransactionStatus Status { get; init; }
    public DateTime CreatedAt { get; init; }
}
