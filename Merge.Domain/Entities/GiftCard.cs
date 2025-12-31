namespace Merge.Domain.Entities;

public class GiftCard : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal RemainingAmount { get; set; }
    public Guid? PurchasedByUserId { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public string? Message { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsRedeemed { get; set; } = false;
    public DateTime? RedeemedAt { get; set; }
    
    // Navigation properties
    public User? PurchasedBy { get; set; }
    public User? AssignedTo { get; set; }
}

