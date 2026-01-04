using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.User;
using Merge.Application.Services;
using Merge.Application.Interfaces.ML;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Common;
using Merge.API.Middleware;

namespace Merge.API.Controllers.ML;

[ApiController]
[Route("api/ml/demand-forecasting")]
[Authorize(Roles = "Admin,Manager")]
public class DemandForecastsController : BaseController
{
    private readonly IDemandForecastingService _demandForecastingService;

    public DemandForecastsController(IDemandForecastingService demandForecastingService)
    {
        _demandForecastingService = demandForecastingService;
    }

    /// <summary>
    /// Ürün için talep tahmini yapar (Admin, Manager)
    /// </summary>
    [HttpPost("products/{productId}")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika
    [ProducesResponseType(typeof(DemandForecastDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<DemandForecastDto>> ForecastDemand(
        Guid productId,
        [FromQuery] int forecastDays = 30,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Unbounded query koruması - forecastDays limiti
        if (forecastDays > 365) forecastDays = 365; // Max 1 yıl
        if (forecastDays < 1) forecastDays = 30; // Min 1 gün

        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var forecast = await _demandForecastingService.ForecastDemandAsync(productId, forecastDays, cancellationToken);
        return Ok(forecast);
    }

    /// <summary>
    /// Kategori için talep tahmini yapar (pagination ile) (Admin, Manager)
    /// </summary>
    [HttpPost("categories/{categoryId}")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika
    [ProducesResponseType(typeof(PagedResult<DemandForecastDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<DemandForecastDto>>> ForecastDemandForCategory(
        Guid categoryId,
        [FromQuery] int forecastDays = 30,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Unbounded query koruması - forecastDays limiti
        if (forecastDays > 365) forecastDays = 365; // Max 1 yıl
        if (forecastDays < 1) forecastDays = 30; // Min 1 gün

        // ✅ BOLUM 3.4: Pagination (ZORUNLU)
        if (pageSize > 100) pageSize = 100; // Max limit
        if (page < 1) page = 1;

        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var allForecasts = await _demandForecastingService.ForecastDemandForCategoryAsync(categoryId, forecastDays, cancellationToken);
        var forecastsList = allForecasts.ToList();

        // ✅ BOLUM 3.4: Pagination implementation
        var totalCount = forecastsList.Count;
        var pagedForecasts = forecastsList
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var result = new PagedResult<DemandForecastDto>
        {
            Items = pagedForecasts,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };

        return Ok(result);
    }

    /// <summary>
    /// Talep tahmin istatistiklerini getirir (Admin, Manager)
    /// </summary>
    [HttpGet("stats")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika
    [ProducesResponseType(typeof(DemandForecastStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<DemandForecastStatsDto>> GetStats(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var stats = await _demandForecastingService.GetForecastStatsAsync(startDate, endDate, cancellationToken);
        return Ok(stats);
    }
}
