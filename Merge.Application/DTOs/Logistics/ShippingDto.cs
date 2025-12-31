namespace Merge.Application.DTOs.Logistics;

public class ShippingDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string ShippingProvider { get; set; } = string.Empty;
    public string TrackingNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? ShippedDate { get; set; }
    public DateTime? EstimatedDeliveryDate { get; set; }
    public DateTime? DeliveredDate { get; set; }
    public decimal ShippingCost { get; set; }
    public string? ShippingLabelUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}
