using System.ComponentModel.DataAnnotations;
using Merge.Domain.Enums;

namespace Merge.Domain.Entities;

public class Shipping : BaseEntity
{
    public Guid OrderId { get; set; }
    public string ShippingProvider { get; set; } = string.Empty; // Yurtiçi Kargo, Aras Kargo, MNG vb.
    public string TrackingNumber { get; set; } = string.Empty;
    // ✅ ARCHITECTURE: Enum kullanımı (string Status yerine)
    public ShippingStatus Status { get; set; } = ShippingStatus.Preparing;
    public DateTime? ShippedDate { get; set; }
    public DateTime? EstimatedDeliveryDate { get; set; }
    public DateTime? DeliveredDate { get; set; }
    public decimal ShippingCost { get; set; }
    public string? ShippingLabelUrl { get; set; }

    // ✅ CONCURRENCY: Eşzamanlı güncellemeleri önlemek için
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // Navigation properties
    public Order Order { get; set; } = null!;
}

