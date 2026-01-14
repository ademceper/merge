using Merge.Domain.Modules.Ordering;
namespace Merge.Application.DTOs.Cart;

/// <summary>
/// Cart DTO - BOLUM 7.1.5: Records (ZORUNLU - DTOs record olmalı)
/// </summary>
public record CartDto(
    Guid Id,
    Guid UserId,
    IReadOnlyList<CartItemDto> CartItems,
    decimal TotalAmount
);

