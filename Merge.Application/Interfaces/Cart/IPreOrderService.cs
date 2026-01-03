using Merge.Application.DTOs.Cart;
using Merge.Application.Common;

namespace Merge.Application.Interfaces.Cart;

// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
public interface IPreOrderService
{
    // Pre-Orders
    Task<PreOrderDto> CreatePreOrderAsync(Guid userId, CreatePreOrderDto dto, CancellationToken cancellationToken = default);
    Task<PreOrderDto?> GetPreOrderAsync(Guid id, CancellationToken cancellationToken = default);
    // ✅ BOLUM 3.4: Pagination (ZORUNLU) - PagedResult döndürüyor
    Task<PagedResult<PreOrderDto>> GetUserPreOrdersAsync(Guid userId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<bool> CancelPreOrderAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    Task<bool> PayDepositAsync(Guid userId, PayPreOrderDepositDto dto, CancellationToken cancellationToken = default);
    Task<bool> ConvertToOrderAsync(Guid preOrderId, CancellationToken cancellationToken = default);
    Task NotifyPreOrderAvailableAsync(Guid preOrderId, CancellationToken cancellationToken = default);

    // Campaigns
    Task<PreOrderCampaignDto> CreateCampaignAsync(CreatePreOrderCampaignDto dto, CancellationToken cancellationToken = default);
    Task<PreOrderCampaignDto?> GetCampaignAsync(Guid id, CancellationToken cancellationToken = default);
    // ✅ BOLUM 3.4: Pagination (ZORUNLU) - PagedResult döndürüyor
    Task<PagedResult<PreOrderCampaignDto>> GetActiveCampaignsAsync(int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    // ✅ BOLUM 3.4: Pagination (ZORUNLU) - PagedResult döndürüyor
    Task<PagedResult<PreOrderCampaignDto>> GetCampaignsByProductAsync(Guid productId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<bool> UpdateCampaignAsync(Guid id, CreatePreOrderCampaignDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeactivateCampaignAsync(Guid id, CancellationToken cancellationToken = default);

    // Stats
    Task<PreOrderStatsDto> GetPreOrderStatsAsync(CancellationToken cancellationToken = default);
    Task ProcessExpiredPreOrdersAsync(CancellationToken cancellationToken = default); // Background job
}
