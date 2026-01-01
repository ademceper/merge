using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Marketing;

public class ValidateCouponDto
{
    [Required(ErrorMessage = "Coupon code is required")]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "Coupon code must be between 1 and 50 characters")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Order amount is required")]
    [Range(0.01, 999999999.99, ErrorMessage = "Order amount must be a positive value")]
    public decimal OrderAmount { get; set; }

    public Guid? UserId { get; set; }

    public List<Guid>? ProductIds { get; set; }
}
