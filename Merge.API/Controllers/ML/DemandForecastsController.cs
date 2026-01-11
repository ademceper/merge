using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Common;
using Merge.Application.ML.Commands.ForecastDemand;
using Merge.Application.ML.Queries.ForecastDemandForCategory;
using Merge.Application.ML.Queries.GetForecastStats;
using Merge.API.Middleware;

namespace Merge.API.Controllers.ML;

[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/ml/demand-forecasting")]
[Authorize(Roles = "Admin,Manager")]
public class DemandForecastsController : BaseController
{
    private readonly IMediator _mediator;

    public DemandForecastsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Ürün için talep tahmini yapar (Admin, Manager)
    /// </summary>
    /// <param name="productId">Ürün ID</param>
    /// <param name="forecastDays">Tahmin edilecek gün sayısı (varsayılan: 30)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Talep tahmini sonuçları</returns>
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
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var command = new ForecastDemandCommand(productId, forecastDays);
        var forecast = await _mediator.Send(command, cancellationToken);
        return Ok(forecast);
    }

    /// <summary>
    /// Kategori için talep tahmini yapar (pagination ile) (Admin, Manager)
    /// </summary>
    /// <param name="categoryId">Kategori ID</param>
    /// <param name="forecastDays">Tahmin edilecek gün sayısı (varsayılan: 30)</param>
    /// <param name="page">Sayfa numarası (varsayılan: 1)</param>
    /// <param name="pageSize">Sayfa boyutu (varsayılan: 20)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Sayfalanmış talep tahmini sonuçları</returns>
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
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var query = new ForecastDemandForCategoryQuery(categoryId, forecastDays, page, pageSize);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Talep tahmin istatistiklerini getirir (Admin, Manager)
    /// </summary>
    /// <param name="startDate">Başlangıç tarihi (opsiyonel)</param>
    /// <param name="endDate">Bitiş tarihi (opsiyonel)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Talep tahmin istatistikleri</returns>
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
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var query = new GetForecastStatsQuery(startDate, endDate);
        var stats = await _mediator.Send(query, cancellationToken);
        return Ok(stats);
    }
}
