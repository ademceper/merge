namespace Merge.Application.DTOs.Analytics;

public class InventoryAnalyticsDto
{
    public int TotalProducts { get; set; }
    public int TotalStock { get; set; }
    public int LowStockCount { get; set; }
    public int OutOfStockCount { get; set; }
    public decimal TotalInventoryValue { get; set; }
    public List<WarehouseStockDto> StockByWarehouse { get; set; } = new();
    public List<LowStockProductDto> LowStockProducts { get; set; } = new();
    public List<StockMovementSummaryDto> RecentMovements { get; set; } = new();
}
