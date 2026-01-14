using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MediatR;
using Merge.Application.Configuration;
using Merge.Application.DTOs.Analytics;
using Merge.Application.DTOs.Review;
using Merge.Application.Common;
using Merge.Application.Analytics.Queries.GetDashboardSummary;
using Merge.Application.Analytics.Queries.GetCustomerLifetimeValue;
using Merge.Application.Analytics.Queries.GetSalesAnalytics;
using Merge.Application.Analytics.Queries.GetProductAnalytics;
using Merge.Application.Analytics.Queries.GetCustomerAnalytics;
using Merge.Application.Analytics.Queries.GetFinancialAnalytics;
using Merge.Application.Analytics.Queries.GetReviewAnalytics;
using Merge.Application.Analytics.Queries.GetMarketingAnalytics;
using Merge.Application.Analytics.Queries.GetInventoryAnalytics;
using Merge.Application.Analytics.Queries.GetTopProducts;
using Merge.Application.Analytics.Queries.GetTopCustomers;
using Merge.Application.Analytics.Queries.GetBestSellers;
using Merge.Application.Analytics.Queries.GetWorstPerformers;
using Merge.Application.Analytics.Queries.GetRevenueOverTime;
using Merge.Application.Analytics.Queries.GetSalesByCategory;
using Merge.Application.Analytics.Queries.GetCustomerSegments;
using Merge.Application.Analytics.Queries.GetLowStockProducts;
using Merge.Application.Analytics.Queries.GetStockByWarehouse;
using Merge.Application.Analytics.Queries.GetCouponPerformance;
using Merge.Application.Analytics.Queries.GetReferralPerformance;
using Merge.Application.Analytics.Queries.GetRatingDistribution;
using Merge.Application.Analytics.Queries.GetReviewTrends;
using Merge.Application.Analytics.Queries.GetTopReviewedProducts;
using Merge.Application.Analytics.Queries.GetTopReviewers;
using Merge.Application.Analytics.Queries.GetDashboardMetrics;
using Merge.Application.Analytics.Queries.GetFinancialReport;
using Merge.Application.Analytics.Queries.GetFinancialSummaries;
using Merge.Application.Analytics.Queries.GetFinancialMetrics;
using Merge.Application.Analytics.Commands.RefreshDashboardMetrics;
using Merge.Application.Analytics.Queries.GetReport;
using Merge.Application.Analytics.Queries.GetReports;
using Merge.Application.Analytics.Queries.ExportReport;
using Merge.Application.Analytics.Queries.GetReportSchedules;
using Merge.Application.Analytics.Commands.DeleteReport;
using Merge.Application.Analytics.Commands.ToggleReportSchedule;
using Merge.Application.Analytics.Commands.DeleteReportSchedule;
using Merge.Application.Analytics.Commands.GenerateReport;
using Merge.Application.Analytics.Commands.CreateReportSchedule;
using Merge.API.Middleware;


namespace Merge.API.Controllers.Analytics;

[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/analytics")]
[Authorize(Roles = "Admin,Manager")]
public class AnalyticsController(
    IMediator mediator,
    IOptions<PaginationSettings> paginationSettings) : BaseController
{

    /// <summary>
    /// Dashboard özet bilgilerini getirir
    /// </summary>
    [HttpGet("dashboard/summary")]
    [RateLimit(30, 60)]
    [ProducesResponseType(typeof(DashboardSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<DashboardSummaryDto>> GetDashboardSummary(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetDashboardSummaryQuery(startDate, endDate);
        var summary = await mediator.Send(query, cancellationToken);
        return Ok(summary);
    }

    /// <summary>
    /// Dashboard metriklerini getirir
    /// </summary>
    [HttpGet("dashboard/metrics")]
    [RateLimit(30, 60)]
    [ProducesResponseType(typeof(List<DashboardMetricDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<List<DashboardMetricDto>>> GetDashboardMetrics(
        [FromQuery] string? category = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetDashboardMetricsQuery(category);
        var metrics = await mediator.Send(query, cancellationToken);
        return Ok(metrics);
    }

    /// <summary>
    /// Dashboard metriklerini yeniler
    /// </summary>
    [HttpPost("dashboard/refresh")]
    [RateLimit(5, 300)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> RefreshDashboardMetrics(CancellationToken cancellationToken = default)
    {
        var command = new RefreshDashboardMetricsCommand();
        await mediator.Send(command, cancellationToken);
        return Ok();
    }

    /// <summary>
    /// Satış analitiklerini getirir
    /// </summary>
    [HttpGet("sales")]
    [RateLimit(30, 60)]
    [ProducesResponseType(typeof(SalesAnalyticsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<SalesAnalyticsDto>> GetSalesAnalytics(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        var query = new GetSalesAnalyticsQuery(startDate, endDate);
        var analytics = await mediator.Send(query, cancellationToken);
        return Ok(analytics);
    }

    /// <summary>
    /// Zaman içinde gelir trendini getirir
    /// </summary>
    [HttpGet("sales/revenue-over-time")]
    [RateLimit(30, 60)]
    [ProducesResponseType(typeof(List<TimeSeriesDataPoint>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<List<TimeSeriesDataPoint>>> GetRevenueOverTime(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] string interval = "day",
        CancellationToken cancellationToken = default)
    {
        var query = new GetRevenueOverTimeQuery(startDate, endDate, interval);
        var data = await mediator.Send(query, cancellationToken);
        return Ok(data);
    }

    /// <summary>
    /// En çok satan ürünleri getirir
    /// </summary>
    [HttpGet("sales/top-products")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30 istek / dakika
    [ProducesResponseType(typeof(List<TopProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<List<TopProductDto>>> GetTopProducts(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] int limit = 10,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Max limit kontrolü (config'den)
        if (limit > paginationSettings.Value.MaxPageSize) limit = paginationSettings.Value.MaxPageSize;
        if (limit < 1) limit = 1;

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder
        var query = new GetTopProductsQuery(startDate, endDate, limit);
        var products = await mediator.Send(query, cancellationToken);
        return Ok(products);
    }

    /// <summary>
    /// Kategori bazında satışları getirir
    /// </summary>
    [HttpGet("sales/by-category")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30 istek / dakika (ağır işlem)
    [ProducesResponseType(typeof(List<CategorySalesDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<List<CategorySalesDto>>> GetSalesByCategory(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder
        var query = new GetSalesByCategoryQuery(startDate, endDate);
        var sales = await mediator.Send(query, cancellationToken);
        return Ok(sales);
    }

    // Product Analytics
    /// <summary>
    /// Ürün analitiklerini getirir
    /// </summary>
    [HttpGet("products")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30 istek / dakika (ağır işlem)
    [ProducesResponseType(typeof(ProductAnalyticsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ProductAnalyticsDto>> GetProductAnalytics(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder
        var query = new GetProductAnalyticsQuery(startDate, endDate);
        var analytics = await mediator.Send(query, cancellationToken);
        return Ok(analytics);
    }

    /// <summary>
    /// En çok satan ürünleri getirir
    /// </summary>
    [HttpGet("products/best-sellers")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30 istek / dakika
    [ProducesResponseType(typeof(List<TopProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<List<TopProductDto>>> GetBestSellers(
        [FromQuery] int limit = 10,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Max limit kontrolü (config'den)
        if (limit > paginationSettings.Value.MaxPageSize) limit = paginationSettings.Value.MaxPageSize;
        if (limit < 1) limit = 1;

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder
        var query = new GetBestSellersQuery(limit);
        var products = await mediator.Send(query, cancellationToken);
        return Ok(products);
    }

    /// <summary>
    /// En az performans gösteren ürünleri getirir
    /// </summary>
    [HttpGet("products/worst-performers")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30 istek / dakika
    [ProducesResponseType(typeof(List<TopProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<List<TopProductDto>>> GetWorstPerformers(
        [FromQuery] int limit = 10,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Max limit kontrolü (config'den)
        if (limit > paginationSettings.Value.MaxPageSize) limit = paginationSettings.Value.MaxPageSize;
        if (limit < 1) limit = 1;

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder
        var query = new GetWorstPerformersQuery(limit);
        var products = await mediator.Send(query, cancellationToken);
        return Ok(products);
    }

    // Customer Analytics
    /// <summary>
    /// Müşteri analitiklerini getirir
    /// </summary>
    [HttpGet("customers")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30 istek / dakika (ağır işlem)
    [ProducesResponseType(typeof(CustomerAnalyticsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<CustomerAnalyticsDto>> GetCustomerAnalytics(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder
        var query = new GetCustomerAnalyticsQuery(startDate, endDate);
        var analytics = await mediator.Send(query, cancellationToken);
        return Ok(analytics);
    }

    /// <summary>
    /// En çok harcama yapan müşterileri getirir
    /// </summary>
    [HttpGet("customers/top")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30 istek / dakika
    [ProducesResponseType(typeof(List<TopCustomerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<List<TopCustomerDto>>> GetTopCustomers(
        [FromQuery] int limit = 10,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Max limit kontrolü (config'den)
        if (limit > paginationSettings.Value.MaxPageSize) limit = paginationSettings.Value.MaxPageSize;
        if (limit < 1) limit = 1;

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder
        var query = new GetTopCustomersQuery(limit);
        var customers = await mediator.Send(query, cancellationToken);
        return Ok(customers);
    }

    /// <summary>
    /// Müşteri segmentlerini getirir
    /// </summary>
    [HttpGet("customers/segments")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30 istek / dakika (ağır işlem)
    [ProducesResponseType(typeof(List<CustomerSegmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<List<CustomerSegmentDto>>> GetCustomerSegments(
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var query = new GetCustomerSegmentsQuery();
        var segments = await mediator.Send(query, cancellationToken);
        return Ok(segments);
    }

    /// <summary>
    /// Müşteri yaşam boyu değerini getirir
    /// </summary>
    [HttpGet("customers/{customerId}/lifetime-value")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30 istek / dakika
    [ProducesResponseType(typeof(CustomerLifetimeValueDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<CustomerLifetimeValueDto>> GetCustomerLifetimeValue(
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ SECURITY: IDOR Protection - Kullanıcı sadece kendi verilerine erişebilir (Manager/Admin hariç)
        if (!User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            var currentUserId = GetUserId();
            if (customerId != currentUserId)
            {
                return Forbid();
            }
        }

        var query = new GetCustomerLifetimeValueQuery(customerId);
        var result = await mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    // Inventory Analytics
    /// <summary>
    /// Envanter analitiklerini getirir
    /// </summary>
    [HttpGet("inventory")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30 istek / dakika (ağır işlem)
    [ProducesResponseType(typeof(InventoryAnalyticsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<InventoryAnalyticsDto>> GetInventoryAnalytics(
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var query = new GetInventoryAnalyticsQuery();
        var analytics = await mediator.Send(query, cancellationToken);
        return Ok(analytics);
    }

    /// <summary>
    /// Düşük stoklu ürünleri getirir
    /// </summary>
    [HttpGet("inventory/low-stock")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30 istek / dakika
    [ProducesResponseType(typeof(List<LowStockProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<List<LowStockProductDto>>> GetLowStockProducts(
        [FromQuery] int threshold = 10,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder
        var query = new GetLowStockProductsQuery(threshold);
        var products = await mediator.Send(query, cancellationToken);
        return Ok(products);
    }

    /// <summary>
    /// Depo bazında stok bilgilerini getirir
    /// </summary>
    [HttpGet("inventory/by-warehouse")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30 istek / dakika (ağır işlem)
    [ProducesResponseType(typeof(List<WarehouseStockDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<List<WarehouseStockDto>>> GetStockByWarehouse(
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var query = new GetStockByWarehouseQuery();
        var stock = await mediator.Send(query, cancellationToken);
        return Ok(stock);
    }

    // Marketing Analytics
    /// <summary>
    /// Pazarlama analitiklerini getirir
    /// </summary>
    [HttpGet("marketing")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30 istek / dakika (ağır işlem)
    [ProducesResponseType(typeof(MarketingAnalyticsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<MarketingAnalyticsDto>> GetMarketingAnalytics(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder
        var query = new GetMarketingAnalyticsQuery(startDate, endDate);
        var analytics = await mediator.Send(query, cancellationToken);
        return Ok(analytics);
    }

    /// <summary>
    /// Kupon performansını getirir
    /// </summary>
    [HttpGet("marketing/coupons")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30 istek / dakika
    [ProducesResponseType(typeof(List<CouponPerformanceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<List<CouponPerformanceDto>>> GetCouponPerformance(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder
        var query = new GetCouponPerformanceQuery(startDate, endDate);
        var performance = await mediator.Send(query, cancellationToken);
        return Ok(performance);
    }

    /// <summary>
    /// Referans performansını getirir
    /// </summary>
    [HttpGet("marketing/referrals")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30 istek / dakika
    [ProducesResponseType(typeof(ReferralPerformanceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ReferralPerformanceDto>> GetReferralPerformance(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder
        var query = new GetReferralPerformanceQuery(startDate, endDate);
        var performance = await mediator.Send(query, cancellationToken);
        return Ok(performance);
    }

    // Financial Analytics
    /// <summary>
    /// Finansal analitiklerini getirir
    /// </summary>
    [HttpGet("financial")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30 istek / dakika (ağır işlem)
    [ProducesResponseType(typeof(FinancialAnalyticsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<FinancialAnalyticsDto>> GetFinancialAnalytics(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder
        var query = new GetFinancialAnalyticsQuery(startDate, endDate);
        var analytics = await mediator.Send(query, cancellationToken);
        return Ok(analytics);
    }

    // Review Analytics
    /// <summary>
    /// Yorum analitiklerini getirir
    /// </summary>
    [HttpGet("reviews")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30 istek / dakika (ağır işlem)
    [ProducesResponseType(typeof(ReviewAnalyticsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ReviewAnalyticsDto>> GetReviewAnalytics(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder
        var query = new GetReviewAnalyticsQuery(startDate, endDate);
        var analytics = await mediator.Send(query, cancellationToken);
        return Ok(analytics);
    }

    /// <summary>
    /// Puan dağılımını getirir
    /// </summary>
    [HttpGet("reviews/rating-distribution")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30 istek / dakika
    [ProducesResponseType(typeof(List<RatingDistributionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<List<RatingDistributionDto>>> GetRatingDistribution(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder
        var query = new GetRatingDistributionQuery(startDate, endDate);
        var distribution = await mediator.Send(query, cancellationToken);
        return Ok(distribution);
    }

    /// <summary>
    /// Yorum trendlerini getirir
    /// </summary>
    [HttpGet("reviews/trends")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30 istek / dakika
    [ProducesResponseType(typeof(List<ReviewTrendDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<List<ReviewTrendDto>>> GetReviewTrends(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder
        var query = new GetReviewTrendsQuery(startDate, endDate);
        var trends = await mediator.Send(query, cancellationToken);
        return Ok(trends);
    }

    /// <summary>
    /// En çok yorumlanan ürünleri getirir
    /// </summary>
    [HttpGet("reviews/top-products")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30 istek / dakika
    [ProducesResponseType(typeof(List<TopReviewedProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<List<TopReviewedProductDto>>> GetTopReviewedProducts(
        [FromQuery] int limit = 10,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Max limit kontrolü (config'den)
        if (limit > paginationSettings.Value.MaxPageSize) limit = paginationSettings.Value.MaxPageSize;
        if (limit < 1) limit = 1;

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder
        var query = new GetTopReviewedProductsQuery(limit);
        var products = await mediator.Send(query, cancellationToken);
        return Ok(products);
    }

    /// <summary>
    /// En aktif yorumcuları getirir
    /// </summary>
    [HttpGet("reviews/top-reviewers")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30 istek / dakika
    [ProducesResponseType(typeof(List<ReviewerStatsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<List<ReviewerStatsDto>>> GetTopReviewers(
        [FromQuery] int limit = 10,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Max limit kontrolü (config'den)
        if (limit > paginationSettings.Value.MaxPageSize) limit = paginationSettings.Value.MaxPageSize;
        if (limit < 1) limit = 1;

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder
        var query = new GetTopReviewersQuery(limit);
        var reviewers = await mediator.Send(query, cancellationToken);
        return Ok(reviewers);
    }

    // Reports
    /// <summary>
    /// Yeni rapor oluşturur
    /// </summary>
    [HttpPost("reports")]
    [RateLimit(10, 3600)] // ✅ BOLUM 3.3: Rate Limiting - 10 rapor / saat
    [ProducesResponseType(typeof(ReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ReportDto>> GenerateReport(
        [FromBody] CreateReportDto dto,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder, manuel ValidateModelState() gereksiz
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var command = new GenerateReportCommand(
            userId,
            dto.Name,
            dto.Description,
            dto.Type,
            dto.StartDate,
            dto.EndDate,
            dto.Filters,
            dto.Format);

        var report = await mediator.Send(command, cancellationToken);
        return Ok(report);
    }

    /// <summary>
    /// Rapor detaylarını getirir
    /// </summary>
    [HttpGet("reports/{id}")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30 istek / dakika
    [ProducesResponseType(typeof(ReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ReportDto>> GetReport(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder
        var query = new GetReportQuery(id, userId);
        var report = await mediator.Send(query, cancellationToken);

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

    /// <summary>
    /// Raporları listeler (pagination ile)
    /// </summary>
    [HttpGet("reports")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30 istek / dakika
    [ProducesResponseType(typeof(PagedResult<ReportDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<ReportDto>>> GetReports(
        [FromQuery] Guid? userId = null,
        [FromQuery] string? type = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var currentUserId))
        {
            return Unauthorized();
        }

        // ✅ SECURITY: IDOR Protection - Users can only view their own reports unless Admin/Manager
        if (!User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            userId = currentUserId; // Force current user's ID
        }

        // ✅ BOLUM 3.4: Pagination limit kontrolü (config'den)
        if (pageSize > paginationSettings.Value.MaxPageSize) pageSize = paginationSettings.Value.MaxPageSize;
        if (page < 1) page = 1;

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder
        var query = new GetReportsQuery(userId, type, page, pageSize);
        var reports = await mediator.Send(query, cancellationToken);
        return Ok(reports);
    }

    /// <summary>
    /// Raporu export eder
    /// </summary>
    [HttpGet("reports/{id}/export")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10 export / dakika
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> ExportReport(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ SECURITY: Authorization check - Users can only export their own reports unless Admin
        var reportQuery = new GetReportQuery(id, userId);
        var report = await mediator.Send(reportQuery, cancellationToken);
        if (report == null)
        {
            return NotFound();
        }

        if (report.GeneratedByUserId != userId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder
        var query = new ExportReportQuery(id, userId);
        var data = await mediator.Send(query, cancellationToken);
        
        if (data == null)
        {
            return NotFound();
        }
        
        return File(data, "application/json", $"report_{id}.json");
    }

    /// <summary>
    /// Raporu siler
    /// </summary>
    [HttpDelete("reports/{id}")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10 silme / dakika
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> DeleteReport(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ SECURITY: Authorization check - Users can only delete their own reports unless Admin
        var reportQuery = new GetReportQuery(id, userId);
        var report = await mediator.Send(reportQuery, cancellationToken);
        if (report == null)
        {
            return NotFound();
        }

        if (report.GeneratedByUserId != userId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder
        var command = new DeleteReportCommand(id, userId);
        var success = await mediator.Send(command, cancellationToken);

        if (!success)
        {
            return NotFound();
        }

        return Ok();
    }

    // Report Scheduling
    /// <summary>
    /// Rapor zamanlaması oluşturur
    /// </summary>
    [HttpPost("reports/schedules")]
    [RateLimit(5, 3600)] // ✅ BOLUM 3.3: Rate Limiting - 5 zamanlama / saat
    [ProducesResponseType(typeof(ReportScheduleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ReportScheduleDto>> CreateReportSchedule(
        [FromBody] CreateReportScheduleDto dto,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder, manuel ValidateModelState() gereksiz
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var command = new CreateReportScheduleCommand(
            userId,
            dto.Name,
            dto.Description,
            dto.Type,
            dto.Frequency,
            dto.DayOfWeek,
            dto.DayOfMonth,
            dto.TimeOfDay,
            dto.Filters,
            dto.Format,
            dto.EmailRecipients);

        var schedule = await mediator.Send(command, cancellationToken);
        return Ok(schedule);
    }

    /// <summary>
    /// Rapor zamanlamalarını listeler (pagination ile)
    /// </summary>
    [HttpGet("reports/schedules")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30 istek / dakika
    [ProducesResponseType(typeof(PagedResult<ReportScheduleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<ReportScheduleDto>>> GetReportSchedules(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ BOLUM 3.4: Pagination limit kontrolü (config'den)
        if (pageSize > paginationSettings.Value.MaxPageSize) pageSize = paginationSettings.Value.MaxPageSize;
        if (page < 1) page = 1;

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder
        var query = new GetReportSchedulesQuery(userId, page, pageSize);
        var schedules = await mediator.Send(query, cancellationToken);
        return Ok(schedules);
    }

    /// <summary>
    /// Rapor zamanlamasını aktif/pasif yapar
    /// </summary>
    [HttpPost("reports/schedules/{id}/toggle")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10 toggle / dakika
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> ToggleReportSchedule(
        Guid id,
        [FromQuery] bool isActive,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder
        // ✅ SECURITY: Authorization check handler'da yapılıyor
        var command = new ToggleReportScheduleCommand(id, isActive, userId);
        var success = await mediator.Send(command, cancellationToken);

        if (!success)
        {
            return NotFound();
        }

        return Ok(new { message = $"Report schedule {(isActive ? "activated" : "deactivated")} successfully" });
    }

    /// <summary>
    /// Rapor zamanlamasını siler
    /// </summary>
    [HttpDelete("reports/schedules/{id}")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10 silme / dakika
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> DeleteReportSchedule(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder
        // ✅ SECURITY: Authorization check handler'da yapılıyor
        var command = new DeleteReportScheduleCommand(id, userId);
        var success = await mediator.Send(command, cancellationToken);

        if (!success)
        {
            return NotFound();
        }

        return Ok();
    }

    // Financial Reporting
    /// <summary>
    /// Finansal raporu getirir
    /// </summary>
    [HttpGet("financial/report")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30 istek / dakika (ağır işlem)
    [ProducesResponseType(typeof(FinancialReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<FinancialReportDto>> GetFinancialReport(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder
        var query = new GetFinancialReportQuery(startDate, endDate);
        var report = await mediator.Send(query, cancellationToken);
        return Ok(report);
    }

    /// <summary>
    /// Finansal özetleri getirir
    /// </summary>
    [HttpGet("financial/summaries")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30 istek / dakika (ağır işlem)
    [ProducesResponseType(typeof(List<FinancialSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<List<FinancialSummaryDto>>> GetFinancialSummaries(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] string period = "daily",
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder
        var query = new GetFinancialSummariesQuery(startDate, endDate, period);
        var summaries = await mediator.Send(query, cancellationToken);
        return Ok(summaries);
    }

    /// <summary>
    /// Finansal metrikleri getirir
    /// </summary>
    [HttpGet("financial/metrics")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30 istek / dakika (ağır işlem)
    [ProducesResponseType(typeof(FinancialMetricsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<FinancialMetricsDto>> GetFinancialMetrics(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder
        var query = new GetFinancialMetricsQuery(startDate, endDate);
        var metrics = await mediator.Send(query, cancellationToken);
        return Ok(metrics);
    }
}
