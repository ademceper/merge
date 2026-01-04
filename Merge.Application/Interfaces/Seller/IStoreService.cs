using Merge.Application.DTOs.Seller;
using Merge.Application.Common;

// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
namespace Merge.Application.Interfaces.Seller;

public interface IStoreService
{
    Task<StoreDto> CreateStoreAsync(Guid sellerId, CreateStoreDto dto, CancellationToken cancellationToken = default);
    Task<StoreDto?> GetStoreByIdAsync(Guid storeId, CancellationToken cancellationToken = default);
    Task<StoreDto?> GetStoreBySlugAsync(string slug, CancellationToken cancellationToken = default);
    // ✅ BOLUM 3.4: Pagination (ZORUNLU)
    Task<PagedResult<StoreDto>> GetSellerStoresAsync(Guid sellerId, string? status = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<StoreDto?> GetPrimaryStoreAsync(Guid sellerId, CancellationToken cancellationToken = default);
    Task<bool> UpdateStoreAsync(Guid storeId, UpdateStoreDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteStoreAsync(Guid storeId, CancellationToken cancellationToken = default);
    Task<bool> SetPrimaryStoreAsync(Guid sellerId, Guid storeId, CancellationToken cancellationToken = default);
    Task<bool> VerifyStoreAsync(Guid storeId, CancellationToken cancellationToken = default);
    Task<bool> SuspendStoreAsync(Guid storeId, string reason, CancellationToken cancellationToken = default);
    Task<StoreStatsDto> GetStoreStatsAsync(Guid storeId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
}

