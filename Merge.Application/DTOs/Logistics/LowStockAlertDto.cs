using System.ComponentModel.DataAnnotations;
namespace Merge.Application.DTOs.Logistics;

public class LowStockAlertDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductSKU { get; set; } = string.Empty;
    public Guid WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public int CurrentQuantity { get; set; }
    public int MinimumStockLevel { get; set; }
    public int QuantityNeeded { get; set; }
}
