namespace Merge.Application.DTOs.Logistics;


public record AvailableStockDto(
    Guid ProductId,
    Guid? WarehouseId,
    int AvailableStock
);

