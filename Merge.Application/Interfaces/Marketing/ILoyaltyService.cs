using Merge.Application.DTOs.Marketing;

// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
namespace Merge.Application.Interfaces.Marketing;

public interface ILoyaltyService
{
    Task<LoyaltyAccountDto?> GetLoyaltyAccountAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<LoyaltyAccountDto> CreateLoyaltyAccountAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<LoyaltyTransactionDto>> GetTransactionsAsync(Guid userId, int days = 30, CancellationToken cancellationToken = default);
    Task EarnPointsAsync(Guid userId, int points, string type, string description, Guid? orderId = null, CancellationToken cancellationToken = default);
    Task<bool> RedeemPointsAsync(Guid userId, int points, Guid? orderId = null, CancellationToken cancellationToken = default);
    Task<int> CalculatePointsFromPurchaseAsync(decimal amount, CancellationToken cancellationToken = default);
    Task<decimal> CalculateDiscountFromPointsAsync(int points, CancellationToken cancellationToken = default);
    Task<IEnumerable<LoyaltyTierDto>> GetTiersAsync(CancellationToken cancellationToken = default);
    Task<LoyaltyStatsDto> GetLoyaltyStatsAsync(CancellationToken cancellationToken = default);
    Task ExpirePointsAsync(CancellationToken cancellationToken = default); // Background job to expire old points
}
