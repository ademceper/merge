using Merge.Application.DTOs.Logistics;

namespace Merge.Application.DTOs.Analytics;

// ✅ BOLUM 7.1: Records kullanımı (immutable DTOs) (ZORUNLU)
public record InventoryOverviewDto(
    int TotalWarehouses,
    int TotalInventoryItems,
    decimal TotalInventoryValue,
    int LowStockCount,
    IEnumerable<LowStockAlertDto> LowStockAlerts,
    int TotalStockQuantity,
    int ReservedStockQuantity
);
