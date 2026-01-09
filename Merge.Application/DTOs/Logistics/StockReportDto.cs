namespace Merge.Application.DTOs.Logistics;

// ✅ BOLUM 7.1.5: Records (ZORUNLU - DTOs record olmalı)
public record StockReportDto(
    Guid ProductId,
    string ProductName,
    string ProductSKU,
    int TotalQuantity,
    int TotalReserved,
    int TotalAvailable,
    decimal TotalValue,
    List<InventoryDto> WarehouseBreakdown
);
