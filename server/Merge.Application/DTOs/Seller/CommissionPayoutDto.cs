using Merge.Domain.Enums;
using Merge.Domain.Modules.Payment;

namespace Merge.Application.DTOs.Seller;

public record CommissionPayoutDto
{
    public Guid Id { get; init; }
    public Guid SellerId { get; init; }
    public string SellerName { get; init; } = string.Empty;
    public string PayoutNumber { get; init; } = string.Empty;
    public decimal TotalAmount { get; init; }
    public decimal TransactionFee { get; init; }
    public decimal NetAmount { get; init; }
    public PayoutStatus Status { get; init; }
    public string PaymentMethod { get; init; } = string.Empty;
    public string? TransactionReference { get; init; }
    public DateTime? ProcessedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
    public string? Notes { get; init; }
    public DateTime CreatedAt { get; init; }
    public List<SellerCommissionDto> Commissions { get; init; } = new();
}
