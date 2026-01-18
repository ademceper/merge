using Merge.Domain.Modules.Ordering;
namespace Merge.Application.DTOs.Cart;


public record CartItemDto(
    Guid Id,
    Guid ProductId,
    Guid? ProductVariantId,
    string ProductName,
    string ProductImageUrl,
    int Quantity,
    decimal Price,
    decimal TotalPrice
);

