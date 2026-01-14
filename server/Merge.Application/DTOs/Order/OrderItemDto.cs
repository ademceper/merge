using Merge.Domain.Modules.Ordering;
namespace Merge.Application.DTOs.Order;

public class OrderItemDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductImageUrl { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal TotalPrice { get; set; }
}

