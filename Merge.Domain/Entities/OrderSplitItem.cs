namespace Merge.Domain.Entities;

/// <summary>
/// OrderSplitItem Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class OrderSplitItem : BaseEntity
{
    public Guid OrderSplitId { get; set; }
    public Guid OriginalOrderItemId { get; set; }
    public Guid SplitOrderItemId { get; set; }
    public int Quantity { get; set; } // How many items moved to split order
    
    // Navigation properties
    public OrderSplit OrderSplit { get; set; } = null!;
    public OrderItem OriginalOrderItem { get; set; } = null!;
    public OrderItem SplitOrderItem { get; set; } = null!;
}

