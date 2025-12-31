using Merge.Application.DTOs.Cart;

namespace Merge.Application.Interfaces.Cart;

public interface ISavedCartService
{
    Task<IEnumerable<SavedCartItemDto>> GetSavedItemsAsync(Guid userId);
    Task<SavedCartItemDto> SaveItemAsync(Guid userId, SaveItemDto dto);
    Task<bool> RemoveSavedItemAsync(Guid userId, Guid itemId);
    Task<bool> MoveToCartAsync(Guid userId, Guid itemId);
    Task<bool> ClearSavedItemsAsync(Guid userId);
}

