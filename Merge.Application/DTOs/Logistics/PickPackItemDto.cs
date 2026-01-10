namespace Merge.Application.DTOs.Logistics;

// ✅ BOLUM 7.1.5: Records (ZORUNLU - DTOs record olmalı)
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
