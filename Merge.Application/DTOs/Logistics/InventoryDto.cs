namespace Merge.Application.DTOs.Logistics;

// ✅ BOLUM 7.1.5: Records (ZORUNLU - DTOs record olmalı)
public record InventoryDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    string ProductSKU,
    Guid WarehouseId,
    string WarehouseName,
    string WarehouseCode,
    int Quantity,
    int ReservedQuantity,
    int AvailableQuantity,
    int MinimumStockLevel,
    int MaximumStockLevel,
    decimal UnitCost,
    string? Location,
    DateTime? LastRestockedAt,
    DateTime? LastCountedAt,
    DateTime CreatedAt
);
