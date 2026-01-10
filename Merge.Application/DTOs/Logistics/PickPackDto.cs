namespace Merge.Application.DTOs.Logistics;

// ✅ BOLUM 7.1.5: Records (ZORUNLU - DTOs record olmalı)
public record PickPackDto(
    Guid Id,
    Guid OrderId,
    string OrderNumber,
    Guid WarehouseId,
    string WarehouseName,
    string PackNumber,
    string Status,
    Guid? PickedByUserId,
    string? PickedByName,
    Guid? PackedByUserId,
    string? PackedByName,
    DateTime? PickedAt,
    DateTime? PackedAt,
    DateTime? ShippedAt,
    string? Notes,
    decimal Weight,
    string? Dimensions,
    int PackageCount,
    IReadOnlyList<PickPackItemDto> Items,
    DateTime CreatedAt
);
