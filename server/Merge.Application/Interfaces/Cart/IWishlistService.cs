using Merge.Application.Common;
using Merge.Application.DTOs.Product;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Interfaces.Cart;

public interface IWishlistService
{
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    Task<IEnumerable<ProductDto>> GetWishlistAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<PagedResult<ProductDto>> GetWishlistAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<bool> AddToWishlistAsync(Guid userId, Guid productId, CancellationToken cancellationToken = default);
    Task<bool> RemoveFromWishlistAsync(Guid userId, Guid productId, CancellationToken cancellationToken = default);
    Task<bool> IsInWishlistAsync(Guid userId, Guid productId, CancellationToken cancellationToken = default);
}

