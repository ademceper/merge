using Merge.Application.DTOs.Cart;
using Merge.Application.Common;
using Merge.Domain.Enums;

namespace Merge.Application.Interfaces.Cart;

// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
public interface IAbandonedCartService
{
    // ✅ BOLUM 3.4: Pagination (ZORUNLU) - PagedResult döndürüyor
    Task<PagedResult<AbandonedCartDto>> GetAbandonedCartsAsync(int minHours = 1, int maxDays = 30, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<AbandonedCartDto?> GetAbandonedCartByIdAsync(Guid cartId, CancellationToken cancellationToken = default);
    Task<AbandonedCartRecoveryStatsDto> GetRecoveryStatsAsync(int days = 30, CancellationToken cancellationToken = default);
    // ✅ BOLUM 1.2: Enum Kullanimi (ZORUNLU - String Status YASAK)
    Task SendRecoveryEmailAsync(Guid cartId, AbandonedCartEmailType emailType = AbandonedCartEmailType.First, bool includeCoupon = false, decimal? couponDiscountPercentage = null, CancellationToken cancellationToken = default);
    // ✅ BOLUM 1.2: Enum Kullanimi (ZORUNLU - String Status YASAK)
    Task SendBulkRecoveryEmailsAsync(int minHours = 2, AbandonedCartEmailType emailType = AbandonedCartEmailType.First, CancellationToken cancellationToken = default);
    Task<bool> TrackEmailOpenAsync(Guid emailId, CancellationToken cancellationToken = default);
    Task<bool> TrackEmailClickAsync(Guid emailId, CancellationToken cancellationToken = default);
    Task MarkCartAsRecoveredAsync(Guid cartId, CancellationToken cancellationToken = default);
    // ✅ BOLUM 3.4: Pagination (ZORUNLU) - PagedResult döndürüyor
    Task<PagedResult<AbandonedCartEmailDto>> GetCartEmailHistoryAsync(Guid cartId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
}
