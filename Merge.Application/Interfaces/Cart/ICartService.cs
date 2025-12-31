using Merge.Application.DTOs.Cart;

namespace Merge.Application.Interfaces.Cart;

public interface ICartService
{
    Task<CartDto> GetCartByUserIdAsync(Guid userId);
    Task<CartDto?> GetCartByCartItemIdAsync(Guid cartItemId);
    Task<CartItemDto> AddItemToCartAsync(Guid userId, Guid productId, int quantity);
    Task<CartItemDto?> GetCartItemByIdAsync(Guid cartItemId);
    Task<bool> UpdateCartItemQuantityAsync(Guid cartItemId, int quantity);
    Task<bool> RemoveItemFromCartAsync(Guid cartItemId);
    Task<bool> ClearCartAsync(Guid userId);
}

