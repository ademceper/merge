using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Marketing;

public class PurchaseGiftCardDto
{
    [Required(ErrorMessage = "Amount is required")]
    [Range(1, 10000, ErrorMessage = "Amount must be between 1 and 10000")]
    public decimal Amount { get; set; }

    public Guid? AssignedToUserId { get; set; }

    [StringLength(500, ErrorMessage = "Message cannot exceed 500 characters")]
    public string? Message { get; set; }

    public DateTime? ExpiresAt { get; set; } // Null ise 1 yıl sonra
}
