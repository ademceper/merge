namespace Merge.Application.DTOs.Marketing;


public record GiftCardDto(
    Guid Id,
    string Code,
    decimal Amount,
    decimal RemainingAmount,
    Guid? PurchasedByUserId,
    Guid? AssignedToUserId,
    string? Message,
    DateTime ExpiresAt,
    bool IsActive,
    bool IsRedeemed,
    DateTime? RedeemedAt)
{
    public bool IsExpired => DateTime.UtcNow > ExpiresAt;
    public bool IsValid => IsActive && !IsRedeemed && !IsExpired && RemainingAmount > 0;
}
