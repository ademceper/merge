using Merge.Application.DTOs.Seller;
using Merge.Application.Common;
using Merge.Domain.Enums;

// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
namespace Merge.Application.Interfaces.Seller;

public interface ISellerCommissionService
{
    // Commissions
    Task<SellerCommissionDto> CalculateAndRecordCommissionAsync(Guid orderId, Guid orderItemId, CancellationToken cancellationToken = default);
    Task<SellerCommissionDto?> GetCommissionAsync(Guid commissionId, CancellationToken cancellationToken = default);
    // ✅ ARCHITECTURE: Enum kullanımı (string Status yerine) - BEST_PRACTICES_ANALIZI.md BOLUM 1.1.6
    Task<IEnumerable<SellerCommissionDto>> GetSellerCommissionsAsync(Guid sellerId, CommissionStatus? status = null, CancellationToken cancellationToken = default);
    Task<PagedResult<SellerCommissionDto>> GetAllCommissionsAsync(CommissionStatus? status = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<bool> ApproveCommissionAsync(Guid commissionId, CancellationToken cancellationToken = default);
    Task<bool> CancelCommissionAsync(Guid commissionId, CancellationToken cancellationToken = default);

    // Commission Tiers
    Task<CommissionTierDto> CreateTierAsync(CreateCommissionTierDto dto, CancellationToken cancellationToken = default);
    Task<IEnumerable<CommissionTierDto>> GetAllTiersAsync(CancellationToken cancellationToken = default);
    Task<CommissionTierDto?> GetTierForSalesAsync(decimal totalSales, CancellationToken cancellationToken = default);
    Task<bool> UpdateTierAsync(Guid tierId, CreateCommissionTierDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteTierAsync(Guid tierId, CancellationToken cancellationToken = default);

    // Seller Settings
    Task<SellerCommissionSettingsDto?> GetSellerSettingsAsync(Guid sellerId, CancellationToken cancellationToken = default);
    Task<SellerCommissionSettingsDto> UpdateSellerSettingsAsync(Guid sellerId, UpdateCommissionSettingsDto dto, CancellationToken cancellationToken = default);

    // Payouts
    Task<CommissionPayoutDto> RequestPayoutAsync(Guid sellerId, RequestPayoutDto dto, CancellationToken cancellationToken = default);
    Task<CommissionPayoutDto?> GetPayoutAsync(Guid payoutId, CancellationToken cancellationToken = default);
    Task<IEnumerable<CommissionPayoutDto>> GetSellerPayoutsAsync(Guid sellerId, CancellationToken cancellationToken = default);
    // ✅ ARCHITECTURE: Enum kullanımı (string Status yerine) - BEST_PRACTICES_ANALIZI.md BOLUM 1.1.6
    Task<PagedResult<CommissionPayoutDto>> GetAllPayoutsAsync(PayoutStatus? status = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<bool> ProcessPayoutAsync(Guid payoutId, string transactionReference, CancellationToken cancellationToken = default);
    Task<bool> CompletePayoutAsync(Guid payoutId, CancellationToken cancellationToken = default);
    Task<bool> FailPayoutAsync(Guid payoutId, string reason, CancellationToken cancellationToken = default);

    // Stats
    Task<CommissionStatsDto> GetCommissionStatsAsync(Guid? sellerId = null, CancellationToken cancellationToken = default);
    Task<decimal> GetAvailablePayoutAmountAsync(Guid sellerId, CancellationToken cancellationToken = default);
}
