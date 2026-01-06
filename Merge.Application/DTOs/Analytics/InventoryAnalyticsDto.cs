namespace Merge.Application.DTOs.Analytics;

// ✅ BOLUM 7.1: Records kullanımı (immutable DTOs) (ZORUNLU)
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
