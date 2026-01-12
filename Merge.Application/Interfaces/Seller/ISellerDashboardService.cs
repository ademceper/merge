using Merge.Application.DTOs.Order;
using Merge.Application.DTOs.Product;
using Merge.Application.DTOs.Seller;
using Merge.Application.Common;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Ordering;

// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
namespace Merge.Application.Interfaces.Seller;

public interface ISellerDashboardService
{
    Task<SellerDashboardStatsDto> GetDashboardStatsAsync(Guid sellerId, CancellationToken cancellationToken = default);
    Task<PagedResult<OrderDto>> GetSellerOrdersAsync(Guid sellerId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<PagedResult<ProductDto>> GetSellerProductsAsync(Guid sellerId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<SellerPerformanceDto> GetPerformanceMetricsAsync(Guid sellerId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    Task<SellerPerformanceMetricsDto> GetDetailedPerformanceMetricsAsync(Guid sellerId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<List<CategoryPerformanceDto>> GetCategoryPerformanceAsync(Guid sellerId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
}

