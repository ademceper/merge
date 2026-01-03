using Merge.Domain.Enums;

namespace Merge.Domain.Entities;

/// <summary>
/// SellerTransaction Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class SellerTransaction : BaseEntity
{
    public Guid SellerId { get; set; }
    public string TransactionType { get; set; } = string.Empty; // Commission, Payout, Refund, Adjustment
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal BalanceBefore { get; set; }
    public decimal BalanceAfter { get; set; }
    public Guid? RelatedEntityId { get; set; } // CommissionId, PayoutId, OrderId
    public string? RelatedEntityType { get; set; }
    public FinanceTransactionStatus Status { get; set; } = FinanceTransactionStatus.Completed;

    // Navigation properties
    public User Seller { get; set; } = null!;
}

