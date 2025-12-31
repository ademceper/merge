using Merge.Application.DTOs.Analytics;
using Merge.Application.DTOs.Review;

namespace Merge.Application.Interfaces.Analytics;

public interface IAnalyticsService
{
    // Dashboard
    Task<DashboardSummaryDto> GetDashboardSummaryAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<List<DashboardMetricDto>> GetDashboardMetricsAsync(string? category = null);
    Task RefreshDashboardMetricsAsync();

    // Sales Analytics
    Task<SalesAnalyticsDto> GetSalesAnalyticsAsync(DateTime startDate, DateTime endDate);
    Task<List<TimeSeriesDataPoint>> GetRevenueOverTimeAsync(DateTime startDate, DateTime endDate, string interval = "day");
    Task<List<TopProductDto>> GetTopProductsAsync(DateTime startDate, DateTime endDate, int limit = 10);
    Task<List<CategorySalesDto>> GetSalesByCategoryAsync(DateTime startDate, DateTime endDate);

    // Product Analytics
    Task<ProductAnalyticsDto> GetProductAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<List<TopProductDto>> GetBestSellersAsync(int limit = 10);
    Task<List<TopProductDto>> GetWorstPerformersAsync(int limit = 10);

    // Customer Analytics
    Task<CustomerAnalyticsDto> GetCustomerAnalyticsAsync(DateTime startDate, DateTime endDate);
    Task<List<TopCustomerDto>> GetTopCustomersAsync(int limit = 10);
    Task<List<CustomerSegmentDto>> GetCustomerSegmentsAsync();
    Task<decimal> GetCustomerLifetimeValueAsync(Guid customerId);

    // Inventory Analytics
    Task<InventoryAnalyticsDto> GetInventoryAnalyticsAsync();
    Task<List<LowStockProductDto>> GetLowStockProductsAsync(int threshold = 10);
    Task<List<WarehouseStockDto>> GetStockByWarehouseAsync();

    // Marketing Analytics
    Task<MarketingAnalyticsDto> GetMarketingAnalyticsAsync(DateTime startDate, DateTime endDate);
    Task<List<CouponPerformanceDto>> GetCouponPerformanceAsync(DateTime startDate, DateTime endDate);
    Task<ReferralPerformanceDto> GetReferralPerformanceAsync(DateTime startDate, DateTime endDate);

    // Financial Analytics
    Task<FinancialAnalyticsDto> GetFinancialAnalyticsAsync(DateTime startDate, DateTime endDate);

    // Reports
    Task<ReportDto> GenerateReportAsync(CreateReportDto dto, Guid userId);
    Task<ReportDto?> GetReportAsync(Guid id);
    Task<IEnumerable<ReportDto>> GetReportsAsync(Guid? userId = null, string? type = null, int page = 1, int pageSize = 20);
    Task<byte[]> ExportReportAsync(Guid reportId, Guid? userId = null);
    Task<bool> DeleteReportAsync(Guid id, Guid? userId = null);

    // Report Scheduling
    Task<ReportScheduleDto> CreateReportScheduleAsync(CreateReportScheduleDto dto, Guid userId);
    Task<IEnumerable<ReportScheduleDto>> GetReportSchedulesAsync(Guid userId);
    Task<bool> ToggleReportScheduleAsync(Guid id, bool isActive, Guid? userId = null);
    Task<bool> DeleteReportScheduleAsync(Guid id, Guid? userId = null);
    Task ExecuteScheduledReportsAsync(); // Background job

    // Review Analytics
    Task<ReviewAnalyticsDto> GetReviewAnalyticsAsync(DateTime startDate, DateTime endDate);
    Task<List<RatingDistributionDto>> GetRatingDistributionAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<List<ReviewTrendDto>> GetReviewTrendsAsync(DateTime startDate, DateTime endDate);
    Task<List<TopReviewedProductDto>> GetTopReviewedProductsAsync(int limit = 10);
    Task<List<ReviewerStatsDto>> GetTopReviewersAsync(int limit = 10);
    
    // Financial Reporting
    Task<FinancialReportDto> GetFinancialReportAsync(DateTime startDate, DateTime endDate);
    Task<List<FinancialSummaryDto>> GetFinancialSummariesAsync(DateTime startDate, DateTime endDate, string period = "daily");
    Task<Dictionary<string, decimal>> GetFinancialMetricsAsync(DateTime? startDate = null, DateTime? endDate = null);
}
