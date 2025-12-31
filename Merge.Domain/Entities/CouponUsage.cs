namespace Merge.Domain.Entities;

public class CouponUsage : BaseEntity
{
    public Guid CouponId { get; set; }
    public Guid UserId { get; set; }
    public Guid OrderId { get; set; }
    public decimal DiscountAmount { get; set; }
    
    // Navigation properties
    public Coupon Coupon { get; set; } = null!;
    public User User { get; set; } = null!;
    public Order Order { get; set; } = null!;
}

