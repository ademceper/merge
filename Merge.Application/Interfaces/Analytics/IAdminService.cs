using Merge.Application.DTOs.Analytics;
using Merge.Application.DTOs.Order;
using Merge.Application.DTOs.Product;
using Merge.Application.DTOs.Review;
using Merge.Application.DTOs.User;

namespace Merge.Application.Interfaces.Analytics;

public interface IAdminService
{
    // Dashboard
    Task<DashboardStatsDto> GetDashboardStatsAsync();
    Task<RevenueChartDto> GetRevenueChartAsync(int days = 30);
    Task<IEnumerable<AdminTopProductDto>> GetTopProductsAsync(int count = 10);
    Task<InventoryOverviewDto> GetInventoryOverviewAsync();
    
    // Orders
    Task<IEnumerable<OrderDto>> GetRecentOrdersAsync(int count = 10);
    
    // Products
    Task<IEnumerable<ProductDto>> GetLowStockProductsAsync(int threshold = 10);
    
    // Reviews
    Task<IEnumerable<ReviewDto>> GetPendingReviewsAsync();
    
    // Returns
    Task<IEnumerable<ReturnRequestDto>> GetPendingReturnsAsync();
    
    // Users
    Task<IEnumerable<UserDto>> GetUsersAsync(int page = 1, int pageSize = 20, string? role = null);
    Task<bool> ActivateUserAsync(Guid userId);
    Task<bool> DeactivateUserAsync(Guid userId);
    Task<bool> ChangeUserRoleAsync(Guid userId, string role);
    Task<bool> DeleteUserAsync(Guid userId);
    
    // Analytics
    Task<AnalyticsSummaryDto> GetAnalyticsSummaryAsync(int days = 30);
    Task<TwoFactorStatsDto> Get2FAStatsAsync();
    Task<SystemHealthDto> GetSystemHealthAsync();
}

