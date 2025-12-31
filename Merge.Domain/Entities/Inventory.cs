using System.ComponentModel.DataAnnotations;

namespace Merge.Domain.Entities;

public class Inventory : BaseEntity
{
    public Guid ProductId { get; set; }
    public Guid WarehouseId { get; set; }
    public int Quantity { get; set; } = 0; // Current stock quantity
    public int ReservedQuantity { get; set; } = 0; // Reserved for pending orders
    public int AvailableQuantity => Quantity - ReservedQuantity; // Calculated property
    public int MinimumStockLevel { get; set; } = 0; // Alert threshold
    public int MaximumStockLevel { get; set; } = 0; // Maximum capacity
    public decimal UnitCost { get; set; } = 0; // Cost per unit (for valuation)
    public string? Location { get; set; } // Shelf/Bin location within warehouse (e.g., "A-12-3")
    public DateTime? LastRestockedAt { get; set; }
    public DateTime? LastCountedAt { get; set; } // Last physical inventory count date

    // Concurrency control - prevents race conditions during stock updates
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // Navigation properties
    public Product Product { get; set; } = null!;
    public Warehouse Warehouse { get; set; } = null!;
    public ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
}
