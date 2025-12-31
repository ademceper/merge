using Merge.Application.DTOs.Seller;

namespace Merge.Application.Interfaces.Seller;

public interface IStoreService
{
    Task<StoreDto> CreateStoreAsync(Guid sellerId, CreateStoreDto dto);
    Task<StoreDto?> GetStoreByIdAsync(Guid storeId);
    Task<StoreDto?> GetStoreBySlugAsync(string slug);
    Task<IEnumerable<StoreDto>> GetSellerStoresAsync(Guid sellerId, string? status = null);
    Task<StoreDto?> GetPrimaryStoreAsync(Guid sellerId);
    Task<bool> UpdateStoreAsync(Guid storeId, UpdateStoreDto dto);
    Task<bool> DeleteStoreAsync(Guid storeId);
    Task<bool> SetPrimaryStoreAsync(Guid sellerId, Guid storeId);
    Task<bool> VerifyStoreAsync(Guid storeId);
    Task<bool> SuspendStoreAsync(Guid storeId, string reason);
    Task<StoreStatsDto> GetStoreStatsAsync(Guid storeId, DateTime? startDate = null, DateTime? endDate = null);
}

