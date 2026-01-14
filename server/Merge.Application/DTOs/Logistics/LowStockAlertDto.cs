namespace Merge.Application.DTOs.Logistics;

// ✅ BOLUM 7.1.5: Records (ZORUNLU - DTOs record olmalı)
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
