using Merge.Domain.Modules.Ordering;
namespace Merge.Application.DTOs.Cart;


public record CartDto(
    Guid Id,
    Guid UserId,
    IReadOnlyList<CartItemDto> CartItems,
    decimal TotalAmount
);

