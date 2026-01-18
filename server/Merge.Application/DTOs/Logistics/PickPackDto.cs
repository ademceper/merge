using Merge.Domain.Enums;

namespace Merge.Application.DTOs.Logistics;

public record PickPackDto(
    Guid Id,
    Guid OrderId,
    string OrderNumber,
    Guid WarehouseId,
    string WarehouseName,
    string PackNumber,
    PickPackStatus Status,
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
