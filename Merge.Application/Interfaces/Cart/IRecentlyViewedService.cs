using Merge.Application.DTOs.Product;

namespace Merge.Application.Interfaces.Cart;

public interface IRecentlyViewedService
{
    Task<IEnumerable<ProductDto>> GetRecentlyViewedAsync(Guid userId, int count = 20);
    Task AddToRecentlyViewedAsync(Guid userId, Guid productId);
    Task ClearRecentlyViewedAsync(Guid userId);
}

