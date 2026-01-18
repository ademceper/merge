namespace Merge.Application.DTOs.Analytics;

public record InventoryAnalyticsDto(
    int TotalProducts,
    int TotalStock,
    int LowStockCount,
    int OutOfStockCount,
    decimal TotalInventoryValue,
    List<WarehouseStockDto> StockByWarehouse,
    List<LowStockProductDto> LowStockProducts,
    List<StockMovementSummaryDto> RecentMovements
);
