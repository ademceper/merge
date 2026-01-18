namespace Merge.Application.DTOs.Logistics;

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
