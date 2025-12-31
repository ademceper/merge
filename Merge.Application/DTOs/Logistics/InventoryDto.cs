using System.ComponentModel.DataAnnotations;
namespace Merge.Application.DTOs.Logistics;

public class InventoryDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductSKU { get; set; } = string.Empty;
    public Guid WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public string WarehouseCode { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int ReservedQuantity { get; set; }
    public int AvailableQuantity { get; set; }
    public int MinimumStockLevel { get; set; }
    public int MaximumStockLevel { get; set; }
    public decimal UnitCost { get; set; }
    public string? Location { get; set; }
    public DateTime? LastRestockedAt { get; set; }
    public DateTime? LastCountedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
