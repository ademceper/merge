using Merge.Application.DTOs.Analytics;
using Merge.Application.DTOs.Review;
using Merge.Application.Common;
using Merge.Domain.Modules.Analytics;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Inventory;

namespace Merge.Application.Interfaces.Analytics;

// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
public interface IAnalyticsService
{
    // Dashboard
    Task<DashboardSummaryDto> GetDashboardSummaryAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    Task<List<DashboardMetricDto>> GetDashboardMetricsAsync(string? category = null, CancellationToken cancellationToken = default);
    Task RefreshDashboardMetricsAsync(CancellationToken cancellationToken = default);

    // Sales Analytics
    Task<SalesAnalyticsDto> GetSalesAnalyticsAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<List<TimeSeriesDataPoint>> GetRevenueOverTimeAsync(DateTime startDate, DateTime endDate, string interval = "day", CancellationToken cancellationToken = default);
    Task<List<TopProductDto>> GetTopProductsAsync(DateTime startDate, DateTime endDate, int limit = 10, CancellationToken cancellationToken = default);
    Task<List<CategorySalesDto>> GetSalesByCategoryAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

    // Product Analytics
    Task<ProductAnalyticsDto> GetProductAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    Task<List<TopProductDto>> GetBestSellersAsync(int limit = 10, CancellationToken cancellationToken = default);
    Task<List<TopProductDto>> GetWorstPerformersAsync(int limit = 10, CancellationToken cancellationToken = default);

    // Customer Analytics
    Task<CustomerAnalyticsDto> GetCustomerAnalyticsAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<List<TopCustomerDto>> GetTopCustomersAsync(int limit = 10, CancellationToken cancellationToken = default);
    Task<List<CustomerSegmentDto>> GetCustomerSegmentsAsync(CancellationToken cancellationToken = default);
    Task<decimal> GetCustomerLifetimeValueAsync(Guid customerId, CancellationToken cancellationToken = default);

    // Inventory Analytics
    Task<InventoryAnalyticsDto> GetInventoryAnalyticsAsync(CancellationToken cancellationToken = default);
    Task<List<LowStockProductDto>> GetLowStockProductsAsync(int threshold = 10, CancellationToken cancellationToken = default);
    Task<List<WarehouseStockDto>> GetStockByWarehouseAsync(CancellationToken cancellationToken = default);

    // Marketing Analytics
    Task<MarketingAnalyticsDto> GetMarketingAnalyticsAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<List<CouponPerformanceDto>> GetCouponPerformanceAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<ReferralPerformanceDto> GetReferralPerformanceAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

    // Financial Analytics
    Task<FinancialAnalyticsDto> GetFinancialAnalyticsAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

    // Reports
    Task<ReportDto> GenerateReportAsync(CreateReportDto dto, Guid userId, CancellationToken cancellationToken = default);
    Task<ReportDto?> GetReportAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<ReportDto>> GetReportsAsync(Guid? userId = null, string? type = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<byte[]> ExportReportAsync(Guid reportId, Guid? userId = null, CancellationToken cancellationToken = default);
    Task<bool> DeleteReportAsync(Guid id, Guid? userId = null, CancellationToken cancellationToken = default);

    // Report Scheduling
    Task<ReportScheduleDto> CreateReportScheduleAsync(CreateReportScheduleDto dto, Guid userId, CancellationToken cancellationToken = default);
    Task<PagedResult<ReportScheduleDto>> GetReportSchedulesAsync(Guid userId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<bool> ToggleReportScheduleAsync(Guid id, bool isActive, Guid? userId = null, CancellationToken cancellationToken = default);
    Task<bool> DeleteReportScheduleAsync(Guid id, Guid? userId = null, CancellationToken cancellationToken = default);
    Task ExecuteScheduledReportsAsync(CancellationToken cancellationToken = default); // Background job

    // Review Analytics
    Task<ReviewAnalyticsDto> GetReviewAnalyticsAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<List<RatingDistributionDto>> GetRatingDistributionAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    Task<List<ReviewTrendDto>> GetReviewTrendsAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<List<TopReviewedProductDto>> GetTopReviewedProductsAsync(int limit = 10, CancellationToken cancellationToken = default);
    Task<List<ReviewerStatsDto>> GetTopReviewersAsync(int limit = 10, CancellationToken cancellationToken = default);
    
    // Financial Reporting
    Task<FinancialReportDto> GetFinancialReportAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<List<FinancialSummaryDto>> GetFinancialSummariesAsync(DateTime startDate, DateTime endDate, string period = "daily", CancellationToken cancellationToken = default);
    Task<FinancialMetricsDto> GetFinancialMetricsAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
}
