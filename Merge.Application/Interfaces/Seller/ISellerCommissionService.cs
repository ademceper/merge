using Merge.Application.DTOs.Seller;

namespace Merge.Application.Interfaces.Seller;

public interface ISellerCommissionService
{
    // Commissions
    Task<SellerCommissionDto> CalculateAndRecordCommissionAsync(Guid orderId, Guid orderItemId);
    Task<SellerCommissionDto?> GetCommissionAsync(Guid commissionId);
    Task<IEnumerable<SellerCommissionDto>> GetSellerCommissionsAsync(Guid sellerId, string? status = null);
    Task<IEnumerable<SellerCommissionDto>> GetAllCommissionsAsync(string? status = null, int page = 1, int pageSize = 20);
    Task<bool> ApproveCommissionAsync(Guid commissionId);
    Task<bool> CancelCommissionAsync(Guid commissionId);

    // Commission Tiers
    Task<CommissionTierDto> CreateTierAsync(CreateCommissionTierDto dto);
    Task<IEnumerable<CommissionTierDto>> GetAllTiersAsync();
    Task<CommissionTierDto?> GetTierForSalesAsync(decimal totalSales);
    Task<bool> UpdateTierAsync(Guid tierId, CreateCommissionTierDto dto);
    Task<bool> DeleteTierAsync(Guid tierId);

    // Seller Settings
    Task<SellerCommissionSettingsDto?> GetSellerSettingsAsync(Guid sellerId);
    Task<SellerCommissionSettingsDto> UpdateSellerSettingsAsync(Guid sellerId, UpdateCommissionSettingsDto dto);

    // Payouts
    Task<CommissionPayoutDto> RequestPayoutAsync(Guid sellerId, RequestPayoutDto dto);
    Task<CommissionPayoutDto?> GetPayoutAsync(Guid payoutId);
    Task<IEnumerable<CommissionPayoutDto>> GetSellerPayoutsAsync(Guid sellerId);
    Task<IEnumerable<CommissionPayoutDto>> GetAllPayoutsAsync(string? status = null, int page = 1, int pageSize = 20);
    Task<bool> ProcessPayoutAsync(Guid payoutId, string transactionReference);
    Task<bool> CompletePayoutAsync(Guid payoutId);
    Task<bool> FailPayoutAsync(Guid payoutId, string reason);

    // Stats
    Task<CommissionStatsDto> GetCommissionStatsAsync(Guid? sellerId = null);
    Task<decimal> GetAvailablePayoutAmountAsync(Guid sellerId);
}
