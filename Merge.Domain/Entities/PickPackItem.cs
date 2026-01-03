namespace Merge.Domain.Entities;

/// <summary>
/// PickPackItem Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class PickPackItem : BaseEntity
{
    public Guid PickPackId { get; set; }
    public Guid OrderItemId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public bool IsPicked { get; set; } = false;
    public bool IsPacked { get; set; } = false;
    public DateTime? PickedAt { get; set; }
    public DateTime? PackedAt { get; set; }
    public string? Location { get; set; } // Warehouse location (Aisle-Shelf-Bin)
    
    // Navigation properties
    public PickPack PickPack { get; set; } = null!;
    public OrderItem OrderItem { get; set; } = null!;
    public Product Product { get; set; } = null!;
}

