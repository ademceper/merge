namespace Merge.Application.DTOs.Logistics;

public record PickPackItemDto(
    Guid Id,
    Guid OrderItemId,
    Guid ProductId,
    string ProductName,
    string ProductSKU,
    int Quantity,
    bool IsPicked,
    bool IsPacked,
    DateTime? PickedAt,
    DateTime? PackedAt,
    string? Location
);
