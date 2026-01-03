using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Analytics;
using Merge.Application.DTOs.Analytics;
using Merge.Application.DTOs.Review;
using Merge.Application.Common;
using Merge.API.Middleware;


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
    /// <summary>
    /// Dashboard özet bilgilerini getirir
    /// </summary>
    [HttpGet("dashboard/summary")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30 istek / dakika (ağır işlem)
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
        // ✅ BOLUM 4.1: Input Validation - StartDate < EndDate (her ikisi de varsa)
        if (startDate.HasValue && endDate.HasValue && startDate.Value >= endDate.Value)
        {
            ModelState.AddModelError(nameof(startDate), "Başlangıç tarihi bitiş tarihinden önce olmalıdır");
            return ValidationProblem(ModelState);
        }

        var summary = await _analyticsService.GetDashboardSummaryAsync(startDate, endDate, cancellationToken);
        return Ok(summary);
    }

    /// <summary>
    /// Dashboard metriklerini getirir
    /// </summary>
    [HttpGet("dashboard/metrics")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30 istek / dakika
    [ProducesResponseType(typeof(List<DashboardMetricDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<List<DashboardMetricDto>>> GetDashboardMetrics(
        [FromQuery] string? category = null,
        CancellationToken cancellationToken = default)
    {
        var metrics = await _analyticsService.GetDashboardMetricsAsync(category, cancellationToken);
        return Ok(metrics);
    }

    /// <summary>
    /// Dashboard metriklerini yeniler
    /// </summary>
    [HttpPost("dashboard/refresh")]
    [RateLimit(5, 300)] // ✅ BOLUM 3.3: Rate Limiting - 5 refresh / 5 dakika
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> RefreshDashboardMetrics(CancellationToken cancellationToken = default)
    {
        await _analyticsService.RefreshDashboardMetricsAsync(cancellationToken);
        return Ok();
    }

    // Sales Analytics
    /// <summary>
    /// Satış analitiklerini getirir
    /// </summary>
    [HttpGet("sales")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30 istek / dakika (ağır işlem)
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
        // ✅ BOLUM 4.1: Input Validation - StartDate < EndDate
        if (startDate >= endDate)
        {
            ModelState.AddModelError(nameof(startDate), "Başlangıç tarihi bitiş tarihinden önce olmalıdır");
            return ValidationProblem(ModelState);
        }

        var analytics = await _analyticsService.GetSalesAnalyticsAsync(startDate, endDate, cancellationToken);
        return Ok(analytics);
    }

    /// <summary>
    /// Zaman içinde gelir trendini getirir
    /// </summary>
    [HttpGet("sales/revenue-over-time")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30 istek / dakika (ağır işlem)
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
        // ✅ BOLUM 4.1: Input Validation - StartDate < EndDate
        if (startDate >= endDate)
        {
            ModelState.AddModelError(nameof(startDate), "Başlangıç tarihi bitiş tarihinden önce olmalıdır");
            return ValidationProblem(ModelState);
        }

        var data = await _analyticsService.GetRevenueOverTimeAsync(startDate, endDate, interval, cancellationToken);
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
        // ✅ BOLUM 4.1: Input Validation - StartDate < EndDate
        if (startDate >= endDate)
        {
            ModelState.AddModelError(nameof(startDate), "Başlangıç tarihi bitiş tarihinden önce olmalıdır");
            return ValidationProblem(ModelState);
        }

        if (limit > 100) limit = 100; // ✅ BOLUM 3.4: Max limit kontrolü
        if (limit < 1) limit = 1; // ✅ BOLUM 4.1: Min limit kontrolü

        var products = await _analyticsService.GetTopProductsAsync(startDate, endDate, limit, cancellationToken);
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
        // ✅ BOLUM 4.1: Input Validation - StartDate < EndDate
        if (startDate >= endDate)
        {
            ModelState.AddModelError(nameof(startDate), "Başlangıç tarihi bitiş tarihinden önce olmalıdır");
            return ValidationProblem(ModelState);
        }

        var sales = await _analyticsService.GetSalesByCategoryAsync(startDate, endDate, cancellationToken);
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
        // ✅ BOLUM 4.1: Input Validation - StartDate < EndDate (her ikisi de varsa)
        if (startDate.HasValue && endDate.HasValue && startDate.Value >= endDate.Value)
        {
            ModelState.AddModelError(nameof(startDate), "Başlangıç tarihi bitiş tarihinden önce olmalıdır");
            return ValidationProblem(ModelState);
        }

        var analytics = await _analyticsService.GetProductAnalyticsAsync(startDate, endDate, cancellationToken);
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
        if (limit > 100) limit = 100; // ✅ BOLUM 3.4: Max limit kontrolü
        if (limit < 1) limit = 1; // ✅ BOLUM 4.1: Min limit kontrolü

        var products = await _analyticsService.GetBestSellersAsync(limit, cancellationToken);
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
        if (limit > 100) limit = 100; // ✅ BOLUM 3.4: Max limit kontrolü
        if (limit < 1) limit = 1; // ✅ BOLUM 4.1: Min limit kontrolü

        var products = await _analyticsService.GetWorstPerformersAsync(limit, cancellationToken);
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
        // ✅ BOLUM 4.1: Input Validation - StartDate < EndDate
        if (startDate >= endDate)
        {
            ModelState.AddModelError(nameof(startDate), "Başlangıç tarihi bitiş tarihinden önce olmalıdır");
            return ValidationProblem(ModelState);
        }

        var analytics = await _analyticsService.GetCustomerAnalyticsAsync(startDate, endDate, cancellationToken);
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
        if (limit > 100) limit = 100; // ✅ BOLUM 3.4: Max limit kontrolü
        if (limit < 1) limit = 1; // ✅ BOLUM 4.1: Min limit kontrolü

        var customers = await _analyticsService.GetTopCustomersAsync(limit, cancellationToken);
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
        var segments = await _analyticsService.GetCustomerSegmentsAsync(cancellationToken);
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
        var ltv = await _analyticsService.GetCustomerLifetimeValueAsync(customerId, cancellationToken);
        return Ok(new CustomerLifetimeValueDto { CustomerId = customerId, LifetimeValue = ltv });
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
        var analytics = await _analyticsService.GetInventoryAnalyticsAsync(cancellationToken);
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
        // ✅ BOLUM 4.1: Input Validation - threshold pozitif olmalı
        if (threshold < 0)
        {
            ModelState.AddModelError(nameof(threshold), "Eşik değeri 0 veya daha büyük olmalıdır");
            return ValidationProblem(ModelState);
        }

        var products = await _analyticsService.GetLowStockProductsAsync(threshold, cancellationToken);
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
        var stock = await _analyticsService.GetStockByWarehouseAsync(cancellationToken);
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
        // ✅ BOLUM 4.1: Input Validation - StartDate < EndDate
        if (startDate >= endDate)
        {
            ModelState.AddModelError(nameof(startDate), "Başlangıç tarihi bitiş tarihinden önce olmalıdır");
            return ValidationProblem(ModelState);
        }

        var analytics = await _analyticsService.GetMarketingAnalyticsAsync(startDate, endDate, cancellationToken);
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
        // ✅ BOLUM 4.1: Input Validation - StartDate < EndDate
        if (startDate >= endDate)
        {
            ModelState.AddModelError(nameof(startDate), "Başlangıç tarihi bitiş tarihinden önce olmalıdır");
            return ValidationProblem(ModelState);
        }

        var performance = await _analyticsService.GetCouponPerformanceAsync(startDate, endDate, cancellationToken);
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
        // ✅ BOLUM 4.1: Input Validation - StartDate < EndDate
        if (startDate >= endDate)
        {
            ModelState.AddModelError(nameof(startDate), "Başlangıç tarihi bitiş tarihinden önce olmalıdır");
            return ValidationProblem(ModelState);
        }

        var performance = await _analyticsService.GetReferralPerformanceAsync(startDate, endDate, cancellationToken);
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
        // ✅ BOLUM 4.1: Input Validation - StartDate < EndDate
        if (startDate >= endDate)
        {
            ModelState.AddModelError(nameof(startDate), "Başlangıç tarihi bitiş tarihinden önce olmalıdır");
            return ValidationProblem(ModelState);
        }

        var analytics = await _analyticsService.GetFinancialAnalyticsAsync(startDate, endDate, cancellationToken);
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
        // ✅ BOLUM 4.1: Input Validation - StartDate < EndDate
        if (startDate >= endDate)
        {
            ModelState.AddModelError(nameof(startDate), "Başlangıç tarihi bitiş tarihinden önce olmalıdır");
            return ValidationProblem(ModelState);
        }

        var analytics = await _analyticsService.GetReviewAnalyticsAsync(startDate, endDate, cancellationToken);
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
        // ✅ BOLUM 4.1: Input Validation - StartDate < EndDate (her ikisi de varsa)
        if (startDate.HasValue && endDate.HasValue && startDate.Value >= endDate.Value)
        {
            ModelState.AddModelError(nameof(startDate), "Başlangıç tarihi bitiş tarihinden önce olmalıdır");
            return ValidationProblem(ModelState);
        }

        var distribution = await _analyticsService.GetRatingDistributionAsync(startDate, endDate, cancellationToken);
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
        // ✅ BOLUM 4.1: Input Validation - StartDate < EndDate
        if (startDate >= endDate)
        {
            ModelState.AddModelError(nameof(startDate), "Başlangıç tarihi bitiş tarihinden önce olmalıdır");
            return ValidationProblem(ModelState);
        }

        var trends = await _analyticsService.GetReviewTrendsAsync(startDate, endDate, cancellationToken);
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
        if (limit > 100) limit = 100; // ✅ BOLUM 3.4: Max limit kontrolü
        if (limit < 1) limit = 1; // ✅ BOLUM 4.1: Min limit kontrolü

        var products = await _analyticsService.GetTopReviewedProductsAsync(limit, cancellationToken);
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
        if (limit > 100) limit = 100; // ✅ BOLUM 3.4: Max limit kontrolü
        if (limit < 1) limit = 1; // ✅ BOLUM 4.1: Min limit kontrolü

        var reviewers = await _analyticsService.GetTopReviewersAsync(limit, cancellationToken);
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
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var report = await _analyticsService.GenerateReportAsync(dto, userId, cancellationToken);
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

        var report = await _analyticsService.GetReportAsync(id, cancellationToken);

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
        // ✅ BOLUM 3.4: Pagination limit kontrolü
        if (pageSize > 100) pageSize = 100;
        if (page < 1) page = 1;

        var reports = await _analyticsService.GetReportsAsync(userId, type, page, pageSize, cancellationToken);
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
        var report = await _analyticsService.GetReportAsync(id, cancellationToken);
        if (report == null)
        {
            return NotFound();
        }

        if (report.GeneratedByUserId != userId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        var data = await _analyticsService.ExportReportAsync(id, userId, cancellationToken);
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
        var report = await _analyticsService.GetReportAsync(id, cancellationToken);
        if (report == null)
        {
            return NotFound();
        }

        if (report.GeneratedByUserId != userId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        var success = await _analyticsService.DeleteReportAsync(id, userId, cancellationToken);

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
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var schedule = await _analyticsService.CreateReportScheduleAsync(dto, userId, cancellationToken);
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

        // ✅ BOLUM 3.4: Pagination limit kontrolü
        if (pageSize > 100) pageSize = 100;
        if (page < 1) page = 1;

        var schedules = await _analyticsService.GetReportSchedulesAsync(userId, page, pageSize, cancellationToken);
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

        // ✅ SECURITY: Authorization check - Users can only toggle their own schedules unless Admin
        var scheduleResult = await _analyticsService.GetReportSchedulesAsync(userId, page: 1, pageSize: 100, cancellationToken);
        var schedule = scheduleResult.Items.FirstOrDefault(s => s.Id == id);
        
        if (schedule == null && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        var success = await _analyticsService.ToggleReportScheduleAsync(id, isActive, userId, cancellationToken);

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

        // ✅ SECURITY: Authorization check - Users can only delete their own schedules unless Admin
        var scheduleResult = await _analyticsService.GetReportSchedulesAsync(userId, page: 1, pageSize: 100, cancellationToken);
        var schedule = scheduleResult.Items.FirstOrDefault(s => s.Id == id);
        
        if (schedule == null && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        var success = await _analyticsService.DeleteReportScheduleAsync(id, userId, cancellationToken);

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
        // ✅ BOLUM 4.1: Input Validation - StartDate < EndDate
        if (startDate >= endDate)
        {
            ModelState.AddModelError(nameof(startDate), "Başlangıç tarihi bitiş tarihinden önce olmalıdır");
            return ValidationProblem(ModelState);
        }

        var report = await _analyticsService.GetFinancialReportAsync(startDate, endDate, cancellationToken);
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
        // ✅ BOLUM 4.1: Input Validation - StartDate < EndDate
        if (startDate >= endDate)
        {
            ModelState.AddModelError(nameof(startDate), "Başlangıç tarihi bitiş tarihinden önce olmalıdır");
            return ValidationProblem(ModelState);
        }

        var summaries = await _analyticsService.GetFinancialSummariesAsync(startDate, endDate, period, cancellationToken);
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
        // ✅ BOLUM 4.1: Input Validation - StartDate < EndDate (her ikisi de varsa)
        if (startDate.HasValue && endDate.HasValue && startDate.Value >= endDate.Value)
        {
            ModelState.AddModelError(nameof(startDate), "Başlangıç tarihi bitiş tarihinden önce olmalıdır");
            return ValidationProblem(ModelState);
        }

        var metrics = await _analyticsService.GetFinancialMetricsAsync(startDate, endDate, cancellationToken);
        return Ok(metrics);
    }
}
