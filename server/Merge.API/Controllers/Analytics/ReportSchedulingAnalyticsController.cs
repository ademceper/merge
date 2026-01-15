using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MediatR;
using Merge.Application.Configuration;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Common;
using Merge.API.Middleware;
using Merge.Application.Analytics.Queries.GetReportSchedules;
using Merge.Application.Analytics.Commands.CreateReportSchedule;
using Merge.Application.Analytics.Commands.ToggleReportSchedule;
using Merge.Application.Analytics.Commands.DeleteReportSchedule;

namespace Merge.API.Controllers.Analytics.ReportScheduling;

[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/analytics/report-schedules")]
[Authorize(Roles = "Admin,Manager")]
public class ReportSchedulingAnalyticsController(
    IMediator mediator,
    IOptions<PaginationSettings> paginationSettings) : BaseController
{

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

    
}
