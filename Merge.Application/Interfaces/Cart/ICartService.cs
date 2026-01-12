using Merge.Application.DTOs.Cart;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Interfaces.Cart;

public interface ICartService
{
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    Task<CartDto> GetCartByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<CartDto?> GetCartByCartItemIdAsync(Guid cartItemId, CancellationToken cancellationToken = default);
    Task<CartItemDto> AddItemToCartAsync(Guid userId, Guid productId, int quantity, CancellationToken cancellationToken = default);
    Task<CartItemDto?> GetCartItemByIdAsync(Guid cartItemId, CancellationToken cancellationToken = default);
    Task<bool> UpdateCartItemQuantityAsync(Guid cartItemId, int quantity, CancellationToken cancellationToken = default);
    Task<bool> RemoveItemFromCartAsync(Guid cartItemId, CancellationToken cancellationToken = default);
    Task<bool> ClearCartAsync(Guid userId, CancellationToken cancellationToken = default);
}

