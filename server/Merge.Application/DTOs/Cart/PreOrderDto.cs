using Merge.Domain.Enums;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.DTOs.Cart;


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
