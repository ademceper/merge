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

    // ✅ BOLUM 4.1: Validation Attributes (ZORUNLU) - FutureDate validation
    [CustomValidation(typeof(PurchaseGiftCardDto), nameof(ValidateExpiresAt))]
    public DateTime? ExpiresAt { get; set; } // Null ise 1 yıl sonra

    // ✅ BOLUM 4.1: Custom Validation - ExpiresAt gelecekte olmalı
    public static ValidationResult? ValidateExpiresAt(DateTime? expiresAt, ValidationContext context)
    {
        if (expiresAt.HasValue && expiresAt.Value <= DateTime.UtcNow)
        {
            return new ValidationResult("Son kullanma tarihi gelecekte olmalıdır.", new[] { nameof(ExpiresAt) });
        }
        return ValidationResult.Success;
    }
}
