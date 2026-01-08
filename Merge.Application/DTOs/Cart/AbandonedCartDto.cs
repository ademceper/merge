namespace Merge.Application.DTOs.Cart;

/// <summary>
/// AbandonedCart DTO - BOLUM 7.1.5: Records (ZORUNLU - DTOs record olmalı)
/// </summary>
public record AbandonedCartDto(
    Guid CartId,
    Guid UserId,
    string UserEmail,
    string UserName,
    int ItemCount,
    decimal TotalValue,
    DateTime LastModified,
    int HoursSinceAbandonment,
    IReadOnlyList<CartItemDto> Items,
    bool HasReceivedEmail,
    int EmailsSentCount,
    DateTime? LastEmailSent
);
