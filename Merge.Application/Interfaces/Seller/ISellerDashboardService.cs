using Merge.Application.DTOs.Order;
using Merge.Application.DTOs.Product;
using Merge.Application.DTOs.Seller;

namespace Merge.Application.Interfaces.Seller;

public interface ISellerDashboardService
{
    Task<SellerDashboardStatsDto> GetDashboardStatsAsync(Guid sellerId);
    Task<IEnumerable<OrderDto>> GetSellerOrdersAsync(Guid sellerId, int page = 1, int pageSize = 20);
    Task<IEnumerable<ProductDto>> GetSellerProductsAsync(Guid sellerId, int page = 1, int pageSize = 20);
    Task<SellerPerformanceDto> GetPerformanceMetricsAsync(Guid sellerId, DateTime? startDate = null, DateTime? endDate = null);
    Task<SellerPerformanceMetricsDto> GetDetailedPerformanceMetricsAsync(Guid sellerId, DateTime startDate, DateTime endDate);
    Task<List<CategoryPerformanceDto>> GetCategoryPerformanceAsync(Guid sellerId, DateTime? startDate = null, DateTime? endDate = null);
}

