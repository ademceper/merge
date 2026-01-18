using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MediatR;
using Merge.Application.Configuration;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Common;
using Merge.API.Middleware;
using Merge.Application.Analytics.Queries.GetReport;
using Merge.Application.Analytics.Queries.GetReports;
using Merge.Application.Analytics.Queries.ExportReport;
using Merge.Application.Analytics.Commands.DeleteReport;
using Merge.Application.Analytics.Commands.GenerateReport;

namespace Merge.API.Controllers.Analytics.Reports;

/// <summary>
/// Reports Analytics API endpoints.
/// Rapor analitiklerini yönetir.
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/analytics/reports")]
[Authorize(Roles = "Admin,Manager")]
[Tags("ReportsAnalytics")]
public class ReportsAnalyticsController(
    IMediator mediator,
    IOptions<PaginationSettings> paginationSettings) : BaseController
{

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
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

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

        var query = new GetReportQuery(id, userId);
        var report = await mediator.Send(query, cancellationToken);

        if (report == null)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
        }

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

        if (!User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            userId = currentUserId; // Force current user's ID
        }

        if (pageSize > paginationSettings.Value.MaxPageSize) pageSize = paginationSettings.Value.MaxPageSize;
        if (page < 1) page = 1;

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

        var reportQuery = new GetReportQuery(id, userId);
        var report = await mediator.Send(reportQuery, cancellationToken);
        if (report == null)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
        }

        if (report.GeneratedByUserId != userId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        var query = new ExportReportQuery(id, userId);
        var data = await mediator.Send(query, cancellationToken);
        
        if (data == null)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
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

        var reportQuery = new GetReportQuery(id, userId);
        var report = await mediator.Send(reportQuery, cancellationToken);
        if (report == null)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
        }

        if (report.GeneratedByUserId != userId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        var command = new DeleteReportCommand(id, userId);
        var success = await mediator.Send(command, cancellationToken);

        if (!success)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
        }

        return Ok();
    }

    
}
