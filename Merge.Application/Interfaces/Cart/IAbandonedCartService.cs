using Merge.Application.DTOs.Cart;

namespace Merge.Application.Interfaces.Cart;

public interface IAbandonedCartService
{
    Task<IEnumerable<AbandonedCartDto>> GetAbandonedCartsAsync(int minHours = 1, int maxDays = 30);
    Task<AbandonedCartDto?> GetAbandonedCartByIdAsync(Guid cartId);
    Task<AbandonedCartRecoveryStatsDto> GetRecoveryStatsAsync(int days = 30);
    Task SendRecoveryEmailAsync(Guid cartId, string emailType = "First", bool includeCoupon = false, decimal? couponDiscountPercentage = null);
    Task SendBulkRecoveryEmailsAsync(int minHours = 2, string emailType = "First");
    Task<bool> TrackEmailOpenAsync(Guid emailId);
    Task<bool> TrackEmailClickAsync(Guid emailId);
    Task MarkCartAsRecoveredAsync(Guid cartId);
    Task<IEnumerable<AbandonedCartEmailDto>> GetCartEmailHistoryAsync(Guid cartId);
}
