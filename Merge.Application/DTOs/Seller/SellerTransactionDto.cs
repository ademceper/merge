using Merge.Domain.Enums;

namespace Merge.Application.DTOs.Seller;

public class SellerTransactionDto
{
    public Guid Id { get; set; }
    public Guid SellerId { get; set; }
    // ✅ ARCHITECTURE: Enum kullanımı (string TransactionType yerine) - BEST_PRACTICES_ANALIZI.md BOLUM 1.1.6
    public SellerTransactionType TransactionType { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal BalanceBefore { get; set; }
    public decimal BalanceAfter { get; set; }
    public Guid? RelatedEntityId { get; set; } // CommissionId, PayoutId, OrderId
    public string? RelatedEntityType { get; set; }
    public FinanceTransactionStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}
