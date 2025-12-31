namespace Merge.Application.DTOs.Logistics;

public class PickPackItemDto
{
    public Guid Id { get; set; }
    public Guid OrderItemId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductSKU { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public bool IsPicked { get; set; }
    public bool IsPacked { get; set; }
    public DateTime? PickedAt { get; set; }
    public DateTime? PackedAt { get; set; }
    public string? Location { get; set; }
}
