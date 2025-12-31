namespace Merge.Domain.Entities;

public class GiftCardTransaction : BaseEntity
{
    public Guid GiftCardId { get; set; }
    public Guid? OrderId { get; set; }
    public decimal Amount { get; set; }
    public string TransactionType { get; set; } = string.Empty; // Purchase, Redeem, Refund
    public string? Notes { get; set; }
    
    // Navigation properties
    public GiftCard GiftCard { get; set; } = null!;
    public Order? Order { get; set; }
}

