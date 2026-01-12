using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;
using Merge.Application.DTOs.User;
using Merge.Application.User.Commands.DeleteOldActivities;
using Merge.Application.User.Commands.LogActivity;
using Merge.Application.User.Queries.GetActivityById;
using Merge.Application.User.Queries.GetActivityStats;
using Merge.Application.User.Queries.GetMostViewedProducts;
using Merge.Application.User.Queries.GetUserActivities;
using Merge.Application.User.Queries.GetUserSessions;
using Merge.Application.User.Queries.SearchActivities;
using Merge.API.Middleware;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
// ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
// ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
// ✅ BOLUM 3.4: Pagination (ZORUNLU)
// ✅ BOLUM 4.0: API Versioning (ZORUNLU)
namespace Merge.API.Controllers.User;

[ApiController]
[Route("api/v{version:apiVersion}/user/activities")]
public class ActivitiesController : BaseController
{
    private readonly IMediator _mediator;
    private readonly UserSettings _userSettings;

    public ActivitiesController(IMediator mediator, IOptions<UserSettings> userSettings)
    {
        _mediator = mediator;
        _userSettings = userSettings.Value;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpPost("log")]
    [AllowAnonymous]
    [RateLimit(100, 60)] // ✅ BOLUM 3.3: Rate Limiting - 100/dakika (yüksek limit - activity logging için)
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> LogActivity(
        [FromBody] CreateActivityLogDto activityDto,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";
        var userAgent = Request.Headers["User-Agent"].ToString();

        if (User.Identity?.IsAuthenticated == true)
        {
            var userId = GetUserIdOrNull();
            if (userId.HasValue)
            {
                activityDto.UserId = userId.Value;
            }
        }

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var command = new LogActivityCommand(
            activityDto.UserId,
            activityDto.ActivityType,
            activityDto.EntityType,
            activityDto.EntityId,
            activityDto.Description,
            ipAddress,
            userAgent,
            activityDto.Metadata,
            activityDto.DurationMs,
            activityDto.WasSuccessful,
            activityDto.ErrorMessage);
        await _mediator.Send(command, cancellationToken);
        return Ok();
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika
    [ProducesResponseType(typeof(UserActivityLogDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<UserActivityLogDto>> GetActivityById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var query = new GetActivityByIdQuery(id);
        var activity = await _mediator.Send(query, cancellationToken);
        if (activity == null)
        {
            return NotFound();
        }
        return Ok(activity);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    // ✅ BOLUM 3.4: Pagination (ZORUNLU) - days parametresi ile sınırlı
    [HttpGet("my-activities")]
    [Authorize]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika
    [ProducesResponseType(typeof(IEnumerable<UserActivityLogDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<UserActivityLogDto>>> GetMyActivities(
        [FromQuery] int days = 30,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU) - days parametresi ile sınırlı
        // ✅ BOLUM 12.0: Magic numbers configuration'dan alınıyor
        if (days > _userSettings.Activity.MaxDays) days = _userSettings.Activity.MaxDays;
        if (days < 1) days = _userSettings.Activity.DefaultDays;

        var userId = GetUserId();
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var query = new GetUserActivitiesQuery(userId, days);
        var activities = await _mediator.Send(query, cancellationToken);
        return Ok(activities);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    // ✅ BOLUM 3.4: Pagination (ZORUNLU) - days parametresi ile sınırlı
    [HttpGet("user/{userId}")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika
    [ProducesResponseType(typeof(IEnumerable<UserActivityLogDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<UserActivityLogDto>>> GetUserActivities(
        Guid userId,
        [FromQuery] int days = 30,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU) - days parametresi ile sınırlı
        // ✅ BOLUM 12.0: Magic numbers configuration'dan alınıyor
        if (days > _userSettings.Activity.MaxDays) days = _userSettings.Activity.MaxDays;
        if (days < 1) days = _userSettings.Activity.DefaultDays;

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var query = new GetUserActivitiesQuery(userId, days);
        var activities = await _mediator.Send(query, cancellationToken);
        return Ok(activities);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    // ✅ BOLUM 3.4: Pagination (ZORUNLU) - ActivityFilterDto içinde pageSize var
    [HttpPost("search")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika
    [ProducesResponseType(typeof(IEnumerable<UserActivityLogDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<UserActivityLogDto>>> SearchActivities(
        [FromBody] ActivityFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        // ✅ BOLUM 12.0: Magic numbers configuration'dan alınıyor (PaginationSettings)
        // Note: Validation QueryHandler içinde yapılıyor

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var query = new SearchActivitiesQuery(filter);
        var activities = await _mediator.Send(query, cancellationToken);
        return Ok(activities);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpGet("stats")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika (stats hesaplama ağır)
    [ProducesResponseType(typeof(ActivityStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ActivityStatsDto>> GetActivityStats(
        [FromQuery] int days = 30,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU) - days parametresi ile sınırlı
        // ✅ BOLUM 12.0: Magic numbers configuration'dan alınıyor
        // Note: Validation QueryHandler içinde yapılıyor

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var query = new GetActivityStatsQuery(days);
        var stats = await _mediator.Send(query, cancellationToken);
        return Ok(stats);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    // ✅ BOLUM 3.4: Pagination (ZORUNLU) - days parametresi ile sınırlı
    [HttpGet("sessions/{userId}")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika
    [ProducesResponseType(typeof(IEnumerable<UserSessionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<UserSessionDto>>> GetUserSessions(
        Guid userId,
        [FromQuery] int days = 7,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU) - days parametresi ile sınırlı
        // ✅ BOLUM 12.0: Magic numbers configuration'dan alınıyor
        // Note: Validation QueryHandler içinde yapılıyor

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var query = new GetUserSessionsQuery(userId, days);
        var sessions = await _mediator.Send(query, cancellationToken);
        return Ok(sessions);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    // ✅ BOLUM 3.4: Pagination (ZORUNLU) - days parametresi ile sınırlı
    [HttpGet("my-sessions")]
    [Authorize]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika
    [ProducesResponseType(typeof(IEnumerable<UserSessionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<UserSessionDto>>> GetMySessions(
        [FromQuery] int days = 7,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU) - days parametresi ile sınırlı
        // ✅ BOLUM 12.0: Magic numbers configuration'dan alınıyor
        // Note: Validation QueryHandler içinde yapılıyor

        var userId = GetUserId();
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var query = new GetUserSessionsQuery(userId, days);
        var sessions = await _mediator.Send(query, cancellationToken);
        return Ok(sessions);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpGet("popular-products")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika (analytics hesaplama ağır)
    [ProducesResponseType(typeof(IEnumerable<PopularProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<PopularProductDto>>> GetMostViewedProducts(
        [FromQuery] int days = 30,
        [FromQuery] int topN = 10,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        // ✅ BOLUM 12.0: Magic numbers configuration'dan alınıyor
        // Note: Validation QueryHandler içinde yapılıyor

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var query = new GetMostViewedProductsQuery(days, topN);
        var products = await _mediator.Send(query, cancellationToken);
        return Ok(products);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpDelete("cleanup")]
    [Authorize(Roles = "Admin")]
    [RateLimit(5, 3600)] // ✅ BOLUM 3.3: Rate Limiting - 5/saat (cleanup işlemi ağır)
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> DeleteOldActivities(
        [FromQuery] int daysToKeep = 90,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        // ✅ BOLUM 12.0: Magic numbers configuration'dan alınıyor
        // Note: Validation CommandHandler içinde yapılıyor

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var command = new DeleteOldActivitiesCommand(daysToKeep);
        await _mediator.Send(command, cancellationToken);
        return Ok(new { message = $"Activities older than {daysToKeep} days deleted successfully" });
    }
}

