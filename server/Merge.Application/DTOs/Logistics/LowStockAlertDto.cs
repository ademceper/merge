namespace Merge.Application.DTOs.Logistics;

public record LowStockAlertDto(
    Guid ProductId,
    string ProductName,
    string ProductSKU,
    Guid WarehouseId,
    string WarehouseName,
    int CurrentQuantity,
    int MinimumStockLevel,
    int QuantityNeeded
);
