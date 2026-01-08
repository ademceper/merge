namespace Merge.Application.DTOs.Cart;

/// <summary>
/// CartItem DTO - BOLUM 7.1.5: Records (ZORUNLU - DTOs record olmalı)
/// </summary>
public record CartItemDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    string ProductImageUrl,
    int Quantity,
    decimal Price,
    decimal TotalPrice
);

