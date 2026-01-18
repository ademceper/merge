using Merge.Domain.Enums;

namespace Merge.Application.DTOs.Seller;

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
    public CommissionStatus Status { get; init; }
    public DateTime? ApprovedAt { get; init; }
    public DateTime? PaidAt { get; init; }
    public string? PaymentReference { get; init; }
    public DateTime CreatedAt { get; init; }
}
