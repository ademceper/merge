using Merge.Application.DTOs.User;

namespace Merge.Application.DTOs.Order;

public class OrderSplitDto
{
    public Guid Id { get; set; }
    public Guid OriginalOrderId { get; set; }
    public string OriginalOrderNumber { get; set; } = string.Empty;
    public Guid SplitOrderId { get; set; }
    public string SplitOrderNumber { get; set; } = string.Empty;
    public string SplitReason { get; set; } = string.Empty;
    public Guid? NewAddressId { get; set; }
    public AddressDto? NewAddress { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<OrderSplitItemDto> SplitItems { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}
