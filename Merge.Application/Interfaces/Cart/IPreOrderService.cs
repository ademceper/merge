using Merge.Application.DTOs.Cart;

namespace Merge.Application.Interfaces.Cart;

public interface IPreOrderService
{
    // Pre-Orders
    Task<PreOrderDto> CreatePreOrderAsync(Guid userId, CreatePreOrderDto dto);
    Task<PreOrderDto?> GetPreOrderAsync(Guid id);
    Task<IEnumerable<PreOrderDto>> GetUserPreOrdersAsync(Guid userId);
    Task<bool> CancelPreOrderAsync(Guid id, Guid userId);
    Task<bool> PayDepositAsync(Guid userId, PayPreOrderDepositDto dto);
    Task<bool> ConvertToOrderAsync(Guid preOrderId);
    Task NotifyPreOrderAvailableAsync(Guid preOrderId);

    // Campaigns
    Task<PreOrderCampaignDto> CreateCampaignAsync(CreatePreOrderCampaignDto dto);
    Task<PreOrderCampaignDto?> GetCampaignAsync(Guid id);
    Task<IEnumerable<PreOrderCampaignDto>> GetActiveCampaignsAsync();
    Task<IEnumerable<PreOrderCampaignDto>> GetCampaignsByProductAsync(Guid productId);
    Task<bool> UpdateCampaignAsync(Guid id, CreatePreOrderCampaignDto dto);
    Task<bool> DeactivateCampaignAsync(Guid id);

    // Stats
    Task<PreOrderStatsDto> GetPreOrderStatsAsync();
    Task ProcessExpiredPreOrdersAsync(); // Background job
}
