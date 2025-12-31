namespace Merge.Application.DTOs.Marketing;

public class GiftCardDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal RemainingAmount { get; set; }
    public Guid? PurchasedByUserId { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public string? Message { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsActive { get; set; }
    public bool IsRedeemed { get; set; }
    public DateTime? RedeemedAt { get; set; }
    public bool IsExpired => DateTime.UtcNow > ExpiresAt;
    public bool IsValid => IsActive && !IsRedeemed && !IsExpired && RemainingAmount > 0;
}
