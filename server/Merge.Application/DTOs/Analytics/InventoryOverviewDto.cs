using Merge.Application.DTOs.Logistics;

namespace Merge.Application.DTOs.Analytics;

public record InventoryOverviewDto(
    int TotalWarehouses,
    int TotalInventoryItems,
    decimal TotalInventoryValue,
    int LowStockCount,
    IEnumerable<LowStockAlertDto> LowStockAlerts,
    int TotalStockQuantity,
    int ReservedStockQuantity
);
