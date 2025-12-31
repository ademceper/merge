namespace Merge.Application.DTOs.Cart;

public class SendRecoveryEmailDto
{
    public Guid CartId { get; set; }
    public string EmailType { get; set; } = "First"; // First, Second, Final
    public bool IncludeCoupon { get; set; } = false;
    public decimal? CouponDiscountPercentage { get; set; }
}
