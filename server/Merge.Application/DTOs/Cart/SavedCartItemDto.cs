using Merge.Domain.Modules.Ordering;
namespace Merge.Application.DTOs.Cart;

/// <summary>
/// Saved Cart Item DTO - BOLUM 7.1.5: Records (ZORUNLU)
/// </summary>
public record SavedCartItemDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    string ProductImageUrl,
    decimal Price,
    decimal CurrentPrice,
    int Quantity,
    string? Notes,
    bool IsPriceChanged
);
