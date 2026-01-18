using Merge.Domain.Enums;

namespace Merge.Application.DTOs.Logistics;

public record StockMovementDto(
    Guid Id,
    Guid InventoryId,
    Guid ProductId,
    string ProductName,
    string ProductSKU,
    Guid WarehouseId,
    string WarehouseName,
    StockMovementType MovementType,
    string MovementTypeName,
    int Quantity,
    int QuantityBefore,
    int QuantityAfter,
    string? ReferenceNumber,
    Guid? ReferenceId,
    string? Notes,
    Guid? PerformedBy,
    string? PerformedByName,
    Guid? FromWarehouseId,
    string? FromWarehouseName,
    Guid? ToWarehouseId,
    string? ToWarehouseName,
    DateTime CreatedAt
);
