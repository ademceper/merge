using Merge.Application.DTOs.Product;
using Merge.Application.Common;

namespace Merge.Application.Interfaces.Cart;

// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
public interface IRecentlyViewedService
{
    // ✅ BOLUM 3.4: Pagination (ZORUNLU) - PagedResult döndürüyor
    Task<PagedResult<ProductDto>> GetRecentlyViewedAsync(Guid userId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task AddToRecentlyViewedAsync(Guid userId, Guid productId, CancellationToken cancellationToken = default);
    Task ClearRecentlyViewedAsync(Guid userId, CancellationToken cancellationToken = default);
}

