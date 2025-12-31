namespace Merge.Application.DTOs.Seller;

public class SellerTransactionDto
{
    public Guid Id { get; set; }
    public Guid SellerId { get; set; }
    public string TransactionType { get; set; } = string.Empty; // Commission, Payout, Refund, Adjustment
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal BalanceBefore { get; set; }
    public decimal BalanceAfter { get; set; }
    public Guid? RelatedEntityId { get; set; } // CommissionId, PayoutId, OrderId
    public string? RelatedEntityType { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
