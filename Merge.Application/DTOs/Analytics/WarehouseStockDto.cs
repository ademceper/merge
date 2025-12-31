namespace Merge.Application.DTOs.Analytics;

public class WarehouseStockDto
{
    public Guid WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public int TotalProducts { get; set; }
    public int TotalQuantity { get; set; }
    public decimal TotalValue { get; set; }
}
