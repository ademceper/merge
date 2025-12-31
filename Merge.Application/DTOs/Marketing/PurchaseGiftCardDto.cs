namespace Merge.Application.DTOs.Marketing;

public class PurchaseGiftCardDto
{
    public decimal Amount { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public string? Message { get; set; }
    public DateTime? ExpiresAt { get; set; } // Null ise 1 yıl sonra
}
