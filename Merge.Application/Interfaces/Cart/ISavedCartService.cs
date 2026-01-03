using Merge.Application.DTOs.Cart;
using Merge.Application.Common;

namespace Merge.Application.Interfaces.Cart;

// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
public interface ISavedCartService
{
    // ✅ BOLUM 3.4: Pagination (ZORUNLU) - PagedResult döndürüyor
    Task<PagedResult<SavedCartItemDto>> GetSavedItemsAsync(Guid userId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<SavedCartItemDto> SaveItemAsync(Guid userId, SaveItemDto dto, CancellationToken cancellationToken = default);
    Task<bool> RemoveSavedItemAsync(Guid userId, Guid itemId, CancellationToken cancellationToken = default);
    Task<bool> MoveToCartAsync(Guid userId, Guid itemId, CancellationToken cancellationToken = default);
    Task<bool> ClearSavedItemsAsync(Guid userId, CancellationToken cancellationToken = default);
}

