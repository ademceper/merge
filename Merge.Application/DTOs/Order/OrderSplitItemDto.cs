using Merge.Domain.Modules.Ordering;
namespace Merge.Application.DTOs.Order;

public class OrderSplitItemDto
{
    public Guid Id { get; set; }
    public Guid OriginalOrderItemId { get; set; }
    public Guid SplitOrderItemId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
}
