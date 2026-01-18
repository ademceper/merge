using Merge.Domain.Enums;

namespace Merge.Application.DTOs.Seller;

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
