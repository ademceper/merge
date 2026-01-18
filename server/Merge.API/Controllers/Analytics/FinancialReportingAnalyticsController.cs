using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MediatR;
using Merge.Application.Configuration;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Common;
using Merge.API.Middleware;
using Merge.Application.Analytics.Queries.GetFinancialReport;
using Merge.Application.Analytics.Queries.GetFinancialSummaries;
using Merge.Application.Analytics.Queries.GetFinancialMetrics;

namespace Merge.API.Controllers.Analytics.FinancialReporting;

/// <summary>
/// Financial Reporting Analytics API endpoints.
/// Finansal raporlama analitiklerini yönetir.
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/analytics/financial-reports")]
[Authorize(Roles = "Admin,Manager")]
[Tags("FinancialReportingAnalytics")]
public class FinancialReportingAnalyticsController(
    IMediator mediator,
    IOptions<PaginationSettings> paginationSettings) : BaseController
{

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
        var query = new GetFinancialMetricsQuery(startDate, endDate);
        var metrics = await mediator.Send(query, cancellationToken);
        return Ok(metrics);
    }

}
