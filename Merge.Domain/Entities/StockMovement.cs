using Merge.Domain.Enums;

namespace Merge.Domain.Entities;

/// <summary>
/// StockMovement Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class StockMovement : BaseEntity
{
    public Guid InventoryId { get; set; }
    public Guid ProductId { get; set; }
    public Guid WarehouseId { get; set; }
    public StockMovementType MovementType { get; set; }
    public int Quantity { get; set; } // Positive for additions, negative for reductions
    public int QuantityBefore { get; set; } // Quantity before this movement
    public int QuantityAfter { get; set; } // Quantity after this movement
    public string? ReferenceNumber { get; set; } // PO number, Order number, etc.
    public Guid? ReferenceId { get; set; } // Reference to Order, Purchase Order, etc.
    public string? Notes { get; set; }
    public Guid? PerformedBy { get; set; } // User who performed the movement
    public Guid? FromWarehouseId { get; set; } // For transfers
    public Guid? ToWarehouseId { get; set; } // For transfers

    // Navigation properties
    public Inventory Inventory { get; set; } = null!;
    public Product Product { get; set; } = null!;
    public Warehouse Warehouse { get; set; } = null!;
    public User? User { get; set; }
    public Warehouse? FromWarehouse { get; set; }
    public Warehouse? ToWarehouse { get; set; }
}

