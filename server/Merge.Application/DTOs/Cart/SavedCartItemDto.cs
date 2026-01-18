using Merge.Domain.Modules.Ordering;
namespace Merge.Application.DTOs.Cart;


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
