using Merge.Domain.Enums;

namespace Merge.Application.DTOs.Cart;

/// <summary>
/// PreOrder DTO - BOLUM 7.1.5: Records (ZORUNLU - DTOs record olmalı)
/// BOLUM 1.2: Enum Kullanimi (ZORUNLU - String Status YASAK)
/// </summary>
public record PreOrderDto(
    Guid Id,
    Guid UserId,
    Guid ProductId,
    string ProductName,
    string ProductImage,
    int Quantity,
    decimal Price,
    decimal DepositAmount,
    decimal DepositPaid,
    PreOrderStatus Status, // ✅ BOLUM 1.2: Enum Kullanimi (ZORUNLU - String Status YASAK)
    DateTime ExpectedAvailabilityDate,
    DateTime? ActualAvailabilityDate,
    DateTime ExpiresAt,
    string? Notes,
    DateTime CreatedAt
);
