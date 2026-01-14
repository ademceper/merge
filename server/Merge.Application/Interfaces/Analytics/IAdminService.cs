using Merge.Application.DTOs.Analytics;
using Merge.Application.DTOs.Order;
using Merge.Application.DTOs.Product;
using Merge.Application.DTOs.Review;
using Merge.Application.DTOs.User;
using Merge.Application.Common;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Interfaces.Analytics;

// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
public interface IAdminService
{
    // Dashboard
    Task<DashboardStatsDto> GetDashboardStatsAsync(CancellationToken cancellationToken = default);
    Task<RevenueChartDto> GetRevenueChartAsync(int days = 30, CancellationToken cancellationToken = default);
    Task<IEnumerable<AdminTopProductDto>> GetTopProductsAsync(int count = 10, CancellationToken cancellationToken = default);
    Task<InventoryOverviewDto> GetInventoryOverviewAsync(CancellationToken cancellationToken = default);
    
    // Orders
    Task<IEnumerable<OrderDto>> GetRecentOrdersAsync(int count = 10, CancellationToken cancellationToken = default);
    
    // Products
    Task<IEnumerable<ProductDto>> GetLowStockProductsAsync(int threshold = 10, CancellationToken cancellationToken = default);
    
    // Reviews
    Task<PagedResult<ReviewDto>> GetPendingReviewsAsync(int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    
    // Returns
    Task<PagedResult<ReturnRequestDto>> GetPendingReturnsAsync(int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    
    // Users
    Task<PagedResult<UserDto>> GetUsersAsync(int page = 1, int pageSize = 20, string? role = null, CancellationToken cancellationToken = default);
    Task<bool> ActivateUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> DeactivateUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> ChangeUserRoleAsync(Guid userId, string role, CancellationToken cancellationToken = default);
    Task<bool> DeleteUserAsync(Guid userId, CancellationToken cancellationToken = default);
    
    // Analytics
    Task<AnalyticsSummaryDto> GetAnalyticsSummaryAsync(int days = 30, CancellationToken cancellationToken = default);
    Task<TwoFactorStatsDto> Get2FAStatsAsync(CancellationToken cancellationToken = default);
    Task<SystemHealthDto> GetSystemHealthAsync(CancellationToken cancellationToken = default);
}

