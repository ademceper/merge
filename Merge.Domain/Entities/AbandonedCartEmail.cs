namespace Merge.Domain.Entities;

public class AbandonedCartEmail : BaseEntity
{
    public Guid CartId { get; set; }
    public Guid UserId { get; set; }
    public string EmailType { get; set; } = string.Empty; // First, Second, Final
    public DateTime SentAt { get; set; }
    public bool WasOpened { get; set; } = false;
    public bool WasClicked { get; set; } = false;
    public bool ResultedInPurchase { get; set; } = false;
    public Guid? CouponId { get; set; } // If we included a discount coupon

    // Navigation properties
    public Cart Cart { get; set; } = null!;
    public User User { get; set; } = null!;
    public Coupon? Coupon { get; set; }
}
