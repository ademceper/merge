namespace Merge.Application.DTOs.Analytics;

public record WarehouseStockDto(
    Guid WarehouseId,
    string WarehouseName,
    int TotalProducts,
    int TotalQuantity,
    decimal TotalValue
);
