namespace Merge.Application.DTOs.Seller;

public class CommissionPayoutDto
{
    public Guid Id { get; set; }
    public Guid SellerId { get; set; }
    public string SellerName { get; set; } = string.Empty;
    public string PayoutNumber { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal TransactionFee { get; set; }
    public decimal NetAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string? TransactionReference { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<SellerCommissionDto> Commissions { get; set; } = new();
}
