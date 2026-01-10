using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Marketing;

/// <summary>
/// Purchase Gift Card DTO - BOLUM 1.0: DTO Dosya Organizasyonu (ZORUNLU)
/// </summary>
public record PurchaseGiftCardDto
{
    [Required(ErrorMessage = "Amount is required")]
    [Range(1, 10000, ErrorMessage = "Amount must be between 1 and 10000")]
    public decimal Amount { get; init; }

    public Guid? AssignedToUserId { get; init; }

    [StringLength(500, ErrorMessage = "Message cannot exceed 500 characters")]
    public string? Message { get; init; }

    // ✅ BOLUM 4.1: Validation Attributes (ZORUNLU) - FutureDate validation
    [CustomValidation(typeof(PurchaseGiftCardDto), nameof(ValidateExpiresAt))]
    public DateTime? ExpiresAt { get; init; } // Null ise 1 yıl sonra

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
