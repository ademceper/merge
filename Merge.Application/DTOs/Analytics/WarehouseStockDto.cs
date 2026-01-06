namespace Merge.Application.DTOs.Analytics;

// ✅ BOLUM 7.1: Records kullanımı (immutable DTOs) (ZORUNLU)
public record WarehouseStockDto(
    Guid WarehouseId,
    string WarehouseName,
    int TotalProducts,
    int TotalQuantity,
    decimal TotalValue
);
