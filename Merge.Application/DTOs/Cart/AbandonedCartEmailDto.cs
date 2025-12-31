namespace Merge.Application.DTOs.Cart;

public class AbandonedCartEmailDto
{
    public Guid Id { get; set; }
    public Guid CartId { get; set; }
    public Guid UserId { get; set; }
    public string EmailType { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    public bool WasOpened { get; set; }
    public bool WasClicked { get; set; }
    public bool ResultedInPurchase { get; set; }
    public string? CouponCode { get; set; }
}
