using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Order;

public class CreateOrderDto
{
    [Required]
    public Guid AddressId { get; set; }

    [StringLength(50)]
    public string? CouponCode { get; set; }
}

