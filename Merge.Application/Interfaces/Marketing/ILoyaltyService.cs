using Merge.Application.DTOs.Marketing;
namespace Merge.Application.Interfaces.Marketing;

public interface ILoyaltyService
{
    Task<LoyaltyAccountDto?> GetLoyaltyAccountAsync(Guid userId);
    Task<LoyaltyAccountDto> CreateLoyaltyAccountAsync(Guid userId);
    Task<IEnumerable<LoyaltyTransactionDto>> GetTransactionsAsync(Guid userId, int days = 30);
    Task EarnPointsAsync(Guid userId, int points, string type, string description, Guid? orderId = null);
    Task<bool> RedeemPointsAsync(Guid userId, int points, Guid? orderId = null);
    Task<int> CalculatePointsFromPurchaseAsync(decimal amount);
    Task<decimal> CalculateDiscountFromPointsAsync(int points);
    Task<IEnumerable<LoyaltyTierDto>> GetTiersAsync();
    Task<LoyaltyStatsDto> GetLoyaltyStatsAsync();
    Task ExpirePointsAsync(); // Background job to expire old points
}
