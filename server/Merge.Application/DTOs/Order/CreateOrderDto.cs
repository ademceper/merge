using System.ComponentModel.DataAnnotations;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Marketing;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.DTOs.Order;

public class CreateOrderDto
{
    [Required(ErrorMessage = "Shipping address is required")]
    public Guid ShippingAddressId { get; set; }

    [Required(ErrorMessage = "At least one item is required")]
    [MinLength(1, ErrorMessage = "Order must contain at least one item")]
    public List<CreateOrderItemDto> Items { get; set; } = new();

    [StringLength(50, ErrorMessage = "Coupon code cannot exceed 50 characters")]
    public string? CouponCode { get; set; }
}

public class CreateOrderItemDto
{
    [Required(ErrorMessage = "Product ID is required")]
    public Guid ProductId { get; set; }

    [Required(ErrorMessage = "Quantity is required")]
    [Range(1, 100, ErrorMessage = "Quantity must be between 1 and 100")]
    public int Quantity { get; set; }
}

