using System.ComponentModel.DataAnnotations;
using Merge.Domain.Enums;

namespace Merge.Domain.Entities;

/// <summary>
/// OrderSplit Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class OrderSplit : BaseEntity
{
    public Guid OriginalOrderId { get; set; }
    public Guid SplitOrderId { get; set; }
    public string SplitReason { get; set; } = string.Empty; // Different shipping address, Different seller, Stock availability, etc.
    public Guid? NewAddressId { get; set; } // If split due to different address
    // ✅ ARCHITECTURE: Enum kullanımı (string Status yerine)
    public OrderSplitStatus Status { get; set; } = OrderSplitStatus.Pending;

    // ✅ CONCURRENCY: Eşzamanlı güncellemeleri önlemek için
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // Navigation properties
    public Order OriginalOrder { get; set; } = null!;
    public Order SplitOrder { get; set; } = null!;
    public Address? NewAddress { get; set; }
    public ICollection<OrderSplitItem> OrderSplitItems { get; set; } = new List<OrderSplitItem>();
}

