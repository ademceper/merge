using Merge.Application.DTOs.Product;

namespace Merge.Application.Interfaces.Cart;

public interface IWishlistService
{
    Task<IEnumerable<ProductDto>> GetWishlistAsync(Guid userId);
    Task<bool> AddToWishlistAsync(Guid userId, Guid productId);
    Task<bool> RemoveFromWishlistAsync(Guid userId, Guid productId);
    Task<bool> IsInWishlistAsync(Guid userId, Guid productId);
}

