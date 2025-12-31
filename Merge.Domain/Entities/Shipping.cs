namespace Merge.Domain.Entities;

public class Shipping : BaseEntity
{
    public Guid OrderId { get; set; }
    public string ShippingProvider { get; set; } = string.Empty; // Yurtiçi Kargo, Aras Kargo, MNG vb.
    public string TrackingNumber { get; set; } = string.Empty;
    public string Status { get; set; } = "Preparing"; // Preparing, Shipped, InTransit, Delivered, Returned
    public DateTime? ShippedDate { get; set; }
    public DateTime? EstimatedDeliveryDate { get; set; }
    public DateTime? DeliveredDate { get; set; }
    public decimal ShippingCost { get; set; }
    public string? ShippingLabelUrl { get; set; }
    
    // Navigation properties
    public Order Order { get; set; } = null!;
}

