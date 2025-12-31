using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Analytics;
using Merge.Application.DTOs.Analytics;
using Merge.Application.DTOs.Review;


namespace Merge.API.Controllers.Analytics;

[ApiController]
[Route("api/analytics")]
[Authorize(Roles = "Admin,Manager")]
public class AnalyticsController : BaseController
{
    private readonly IAnalyticsService _analyticsService;

    public AnalyticsController(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    // Dashboard
    [HttpGet("dashboard/summary")]
    public async Task<ActionResult<DashboardSummaryDto>> GetDashboardSummary(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var summary = await _analyticsService.GetDashboardSummaryAsync(startDate, endDate);
        return Ok(summary);
    }

    [HttpGet("dashboard/metrics")]
    public async Task<ActionResult<List<DashboardMetricDto>>> GetDashboardMetrics([FromQuery] string? category = null)
    {
        var metrics = await _analyticsService.GetDashboardMetricsAsync(category);
        return Ok(metrics);
    }

    [HttpPost("dashboard/refresh")]
    public async Task<IActionResult> RefreshDashboardMetrics()
    {
        await _analyticsService.RefreshDashboardMetricsAsync();
        return Ok();
    }

    // Sales Analytics
    [HttpGet("sales")]
    public async Task<ActionResult<SalesAnalyticsDto>> GetSalesAnalytics(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var analytics = await _analyticsService.GetSalesAnalyticsAsync(startDate, endDate);
        return Ok(analytics);
    }

    [HttpGet("sales/revenue-over-time")]
    public async Task<ActionResult<List<TimeSeriesDataPoint>>> GetRevenueOverTime(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] string interval = "day")
    {
        var data = await _analyticsService.GetRevenueOverTimeAsync(startDate, endDate, interval);
        return Ok(data);
    }

    [HttpGet("sales/top-products")]
    public async Task<ActionResult<List<TopProductDto>>> GetTopProducts(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] int limit = 10)
    {
        var products = await _analyticsService.GetTopProductsAsync(startDate, endDate, limit);
        return Ok(products);
    }

    [HttpGet("sales/by-category")]
    public async Task<ActionResult<List<CategorySalesDto>>> GetSalesByCategory(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var sales = await _analyticsService.GetSalesByCategoryAsync(startDate, endDate);
        return Ok(sales);
    }

    // Product Analytics
    [HttpGet("products")]
    public async Task<ActionResult<ProductAnalyticsDto>> GetProductAnalytics(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var analytics = await _analyticsService.GetProductAnalyticsAsync(startDate, endDate);
        return Ok(analytics);
    }

    [HttpGet("products/best-sellers")]
    public async Task<ActionResult<List<TopProductDto>>> GetBestSellers([FromQuery] int limit = 10)
    {
        var products = await _analyticsService.GetBestSellersAsync(limit);
        return Ok(products);
    }

    [HttpGet("products/worst-performers")]
    public async Task<ActionResult<List<TopProductDto>>> GetWorstPerformers([FromQuery] int limit = 10)
    {
        var products = await _analyticsService.GetWorstPerformersAsync(limit);
        return Ok(products);
    }

    // Customer Analytics
    [HttpGet("customers")]
    public async Task<ActionResult<CustomerAnalyticsDto>> GetCustomerAnalytics(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var analytics = await _analyticsService.GetCustomerAnalyticsAsync(startDate, endDate);
        return Ok(analytics);
    }

    [HttpGet("customers/top")]
    public async Task<ActionResult<List<TopCustomerDto>>> GetTopCustomers([FromQuery] int limit = 10)
    {
        var customers = await _analyticsService.GetTopCustomersAsync(limit);
        return Ok(customers);
    }

    [HttpGet("customers/segments")]
    public async Task<ActionResult<List<CustomerSegmentDto>>> GetCustomerSegments()
    {
        var segments = await _analyticsService.GetCustomerSegmentsAsync();
        return Ok(segments);
    }

    [HttpGet("customers/{customerId}/lifetime-value")]
    public async Task<ActionResult<decimal>> GetCustomerLifetimeValue(Guid customerId)
    {
        var ltv = await _analyticsService.GetCustomerLifetimeValueAsync(customerId);
        return Ok(new { customerId, lifetimeValue = ltv });
    }

    // Inventory Analytics
    [HttpGet("inventory")]
    public async Task<ActionResult<InventoryAnalyticsDto>> GetInventoryAnalytics()
    {
        var analytics = await _analyticsService.GetInventoryAnalyticsAsync();
        return Ok(analytics);
    }

    [HttpGet("inventory/low-stock")]
    public async Task<ActionResult<List<LowStockProductDto>>> GetLowStockProducts([FromQuery] int threshold = 10)
    {
        var products = await _analyticsService.GetLowStockProductsAsync(threshold);
        return Ok(products);
    }

    [HttpGet("inventory/by-warehouse")]
    public async Task<ActionResult<List<WarehouseStockDto>>> GetStockByWarehouse()
    {
        var stock = await _analyticsService.GetStockByWarehouseAsync();
        return Ok(stock);
    }

    // Marketing Analytics
    [HttpGet("marketing")]
    public async Task<ActionResult<MarketingAnalyticsDto>> GetMarketingAnalytics(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var analytics = await _analyticsService.GetMarketingAnalyticsAsync(startDate, endDate);
        return Ok(analytics);
    }

    [HttpGet("marketing/coupons")]
    public async Task<ActionResult<List<CouponPerformanceDto>>> GetCouponPerformance(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var performance = await _analyticsService.GetCouponPerformanceAsync(startDate, endDate);
        return Ok(performance);
    }

    [HttpGet("marketing/referrals")]
    public async Task<ActionResult<ReferralPerformanceDto>> GetReferralPerformance(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var performance = await _analyticsService.GetReferralPerformanceAsync(startDate, endDate);
        return Ok(performance);
    }

    // Financial Analytics
    [HttpGet("financial")]
    public async Task<ActionResult<FinancialAnalyticsDto>> GetFinancialAnalytics(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var analytics = await _analyticsService.GetFinancialAnalyticsAsync(startDate, endDate);
        return Ok(analytics);
    }

    // Review Analytics
    [HttpGet("reviews")]
    public async Task<ActionResult<ReviewAnalyticsDto>> GetReviewAnalytics(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var analytics = await _analyticsService.GetReviewAnalyticsAsync(startDate, endDate);
        return Ok(analytics);
    }

    [HttpGet("reviews/rating-distribution")]
    public async Task<ActionResult<List<RatingDistributionDto>>> GetRatingDistribution(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var distribution = await _analyticsService.GetRatingDistributionAsync(startDate, endDate);
        return Ok(distribution);
    }

    [HttpGet("reviews/trends")]
    public async Task<ActionResult<List<ReviewTrendDto>>> GetReviewTrends(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var trends = await _analyticsService.GetReviewTrendsAsync(startDate, endDate);
        return Ok(trends);
    }

    [HttpGet("reviews/top-products")]
    public async Task<ActionResult<List<TopReviewedProductDto>>> GetTopReviewedProducts([FromQuery] int limit = 10)
    {
        var products = await _analyticsService.GetTopReviewedProductsAsync(limit);
        return Ok(products);
    }

    [HttpGet("reviews/top-reviewers")]
    public async Task<ActionResult<List<ReviewerStatsDto>>> GetTopReviewers([FromQuery] int limit = 10)
    {
        var reviewers = await _analyticsService.GetTopReviewersAsync(limit);
        return Ok(reviewers);
    }

    // Reports
    [HttpPost("reports")]
    public async Task<ActionResult<ReportDto>> GenerateReport([FromBody] CreateReportDto dto)
    {
            if (!TryGetUserId(out var userId))
            {
                return Unauthorized();
            }

            var report = await _analyticsService.GenerateReportAsync(dto, userId);
            return Ok(report);
    }

    [HttpGet("reports/{id}")]
    public async Task<ActionResult<ReportDto>> GetReport(Guid id)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var report = await _analyticsService.GetReportAsync(id);

        if (report == null)
        {
            return NotFound();
        }

        // ✅ SECURITY: Authorization check - Users can only view their own reports unless Admin
        if (report.GeneratedByUserId != userId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        return Ok(report);
    }

    [HttpGet("reports")]
    public async Task<ActionResult<IEnumerable<ReportDto>>> GetReports(
        [FromQuery] Guid? userId = null,
        [FromQuery] string? type = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var reports = await _analyticsService.GetReportsAsync(userId, type, page, pageSize);
        return Ok(reports);
    }

    [HttpGet("reports/{id}/export")]
    public async Task<IActionResult> ExportReport(Guid id)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ SECURITY: Authorization check - Users can only export their own reports unless Admin
        var report = await _analyticsService.GetReportAsync(id);
        if (report == null)
        {
            return NotFound();
        }

        if (report.GeneratedByUserId != userId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        var data = await _analyticsService.ExportReportAsync(id, userId);
        return File(data, "application/json", $"report_{id}.json");
    }

    [HttpDelete("reports/{id}")]
    public async Task<IActionResult> DeleteReport(Guid id)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ SECURITY: Authorization check - Users can only delete their own reports unless Admin
        var report = await _analyticsService.GetReportAsync(id);
        if (report == null)
        {
            return NotFound();
        }

        if (report.GeneratedByUserId != userId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        var success = await _analyticsService.DeleteReportAsync(id, userId);

        if (!success)
        {
            return NotFound();
        }

        return Ok();
    }

    // Report Scheduling
    [HttpPost("reports/schedules")]
    public async Task<ActionResult<ReportScheduleDto>> CreateReportSchedule([FromBody] CreateReportScheduleDto dto)
    {
            if (!TryGetUserId(out var userId))
            {
                return Unauthorized();
            }

            var schedule = await _analyticsService.CreateReportScheduleAsync(dto, userId);
            return Ok(schedule);
    }

    [HttpGet("reports/schedules")]
    public async Task<ActionResult<IEnumerable<ReportScheduleDto>>> GetReportSchedules()
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var schedules = await _analyticsService.GetReportSchedulesAsync(userId);
        return Ok(schedules);
    }

    [HttpPost("reports/schedules/{id}/toggle")]
    public async Task<IActionResult> ToggleReportSchedule(Guid id, [FromQuery] bool isActive)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ SECURITY: Authorization check - Users can only toggle their own schedules unless Admin
        var schedules = await _analyticsService.GetReportSchedulesAsync(userId);
        var schedule = schedules.FirstOrDefault(s => s.Id == id);
        
        if (schedule == null && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        var success = await _analyticsService.ToggleReportScheduleAsync(id, isActive, userId);

        if (!success)
        {
            return NotFound();
        }

        return Ok(new { message = $"Report schedule {(isActive ? "activated" : "deactivated")} successfully" });
    }

    [HttpDelete("reports/schedules/{id}")]
    public async Task<IActionResult> DeleteReportSchedule(Guid id)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ SECURITY: Authorization check - Users can only delete their own schedules unless Admin
        var schedules = await _analyticsService.GetReportSchedulesAsync(userId);
        var schedule = schedules.FirstOrDefault(s => s.Id == id);
        
        if (schedule == null && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        var success = await _analyticsService.DeleteReportScheduleAsync(id, userId);

        if (!success)
        {
            return NotFound();
        }

        return Ok();
    }

    // Financial Reporting
    [HttpGet("financial/report")]
    public async Task<ActionResult<FinancialReportDto>> GetFinancialReport(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var report = await _analyticsService.GetFinancialReportAsync(startDate, endDate);
        return Ok(report);
    }

    [HttpGet("financial/summaries")]
    public async Task<ActionResult<List<FinancialSummaryDto>>> GetFinancialSummaries(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] string period = "daily")
    {
        var summaries = await _analyticsService.GetFinancialSummariesAsync(startDate, endDate, period);
        return Ok(summaries);
    }

    [HttpGet("financial/metrics")]
    public async Task<ActionResult<Dictionary<string, decimal>>> GetFinancialMetrics(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var metrics = await _analyticsService.GetFinancialMetricsAsync(startDate, endDate);
        return Ok(metrics);
    }
}
