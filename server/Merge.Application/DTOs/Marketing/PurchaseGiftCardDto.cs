using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Marketing;


public record PurchaseGiftCardDto
{
    [Required(ErrorMessage = "Amount is required")]
    [Range(1, 10000, ErrorMessage = "Amount must be between 1 and 10000")]
    public decimal Amount { get; init; }

    public Guid? AssignedToUserId { get; init; }

    [StringLength(500, ErrorMessage = "Message cannot exceed 500 characters")]
    public string? Message { get; init; }

    [CustomValidation(typeof(PurchaseGiftCardDto), nameof(ValidateExpiresAt))]
    public DateTime? ExpiresAt { get; init; } // Null ise 1 yıl sonra

    public static ValidationResult? ValidateExpiresAt(DateTime? expiresAt, ValidationContext context)
    {
        if (expiresAt.HasValue && expiresAt.Value <= DateTime.UtcNow)
        {
            return new ValidationResult("Son kullanma tarihi gelecekte olmalıdır.", new[] { nameof(ExpiresAt) });
        }
        return ValidationResult.Success;
    }
}
