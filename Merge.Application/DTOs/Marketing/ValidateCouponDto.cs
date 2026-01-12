using System.ComponentModel.DataAnnotations;
using Merge.Domain.Modules.Marketing;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.DTOs.Marketing;

/// <summary>
/// Validate Coupon DTO - BOLUM 1.0: DTO Dosya Organizasyonu (ZORUNLU)
/// </summary>
public record ValidateCouponDto
{
    [Required(ErrorMessage = "Coupon code is required")]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "Coupon code must be between 1 and 50 characters")]
    public string Code { get; init; } = string.Empty;

    [Required(ErrorMessage = "Order amount is required")]
    [Range(0.01, 999999999.99, ErrorMessage = "Order amount must be a positive value")]
    public decimal OrderAmount { get; init; }

    public Guid? UserId { get; init; }

    public List<Guid>? ProductIds { get; init; }
}
