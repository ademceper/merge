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

namespace Merge.API.Controllers.User;

/// <summary>
/// User Activities API endpoints.
/// Kullanıcı aktivitelerini yönetir.
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/user/activities")]
[Authorize]
[Tags("UserActivities")]
public class ActivitiesController(IMediator mediator, IOptions<UserSettings> userSettings) : BaseController
{
    private readonly UserSettings _userSettings = userSettings.Value;

    [HttpPost("log")]
    [AllowAnonymous]
    [RateLimit(100, 60)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> LogActivity(
        [FromBody] CreateActivityLogDto activityDto,
        CancellationToken cancellationToken = default)
    {
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
        await mediator.Send(command, cancellationToken);
        return Ok();
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(UserActivityLogDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<UserActivityLogDto>> GetActivityById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetActivityByIdQuery(id);
        var activity = await mediator.Send(query, cancellationToken);
        if (activity == null)
        {
            return Problem("Activity not found", "Not Found", StatusCodes.Status404NotFound);
        }
        return Ok(activity);
    }

    [HttpGet("my-activities")]
    [Authorize]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(IEnumerable<UserActivityLogDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<UserActivityLogDto>>> GetMyActivities(
        [FromQuery] int days = 30,
        CancellationToken cancellationToken = default)
    {
        if (days > _userSettings.Activity.MaxDays) days = _userSettings.Activity.MaxDays;
        if (days < 1) days = _userSettings.Activity.DefaultDays;

        var userId = GetUserId();
        var query = new GetUserActivitiesQuery(userId, days);
        var activities = await mediator.Send(query, cancellationToken);
        return Ok(activities);
    }

    [HttpGet("user/{userId}")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(IEnumerable<UserActivityLogDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<UserActivityLogDto>>> GetUserActivities(
        Guid userId,
        [FromQuery] int days = 30,
        CancellationToken cancellationToken = default)
    {
        if (days > _userSettings.Activity.MaxDays) days = _userSettings.Activity.MaxDays;
        if (days < 1) days = _userSettings.Activity.DefaultDays;

        var query = new GetUserActivitiesQuery(userId, days);
        var activities = await mediator.Send(query, cancellationToken);
        return Ok(activities);
    }

    [HttpPost("search")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(IEnumerable<UserActivityLogDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<UserActivityLogDto>>> SearchActivities(
        [FromBody] ActivityFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        var query = new SearchActivitiesQuery(filter);
        var activities = await mediator.Send(query, cancellationToken);
        return Ok(activities);
    }

    [HttpGet("stats")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(30, 60)]
    [ProducesResponseType(typeof(ActivityStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ActivityStatsDto>> GetActivityStats(
        [FromQuery] int days = 30,
        CancellationToken cancellationToken = default)
    {
        var query = new GetActivityStatsQuery(days);
        var stats = await mediator.Send(query, cancellationToken);
        return Ok(stats);
    }

    [HttpGet("sessions/{userId}")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(IEnumerable<UserSessionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<UserSessionDto>>> GetUserSessions(
        Guid userId,
        [FromQuery] int days = 7,
        CancellationToken cancellationToken = default)
    {
        var query = new GetUserSessionsQuery(userId, days);
        var sessions = await mediator.Send(query, cancellationToken);
        return Ok(sessions);
    }

    [HttpGet("my-sessions")]
    [Authorize]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(IEnumerable<UserSessionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<UserSessionDto>>> GetMySessions(
        [FromQuery] int days = 7,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var query = new GetUserSessionsQuery(userId, days);
        var sessions = await mediator.Send(query, cancellationToken);
        return Ok(sessions);
    }

    [HttpGet("popular-products")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(30, 60)]
    [ProducesResponseType(typeof(IEnumerable<PopularProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<PopularProductDto>>> GetMostViewedProducts(
        [FromQuery] int days = 30,
        [FromQuery] int topN = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetMostViewedProductsQuery(days, topN);
        var products = await mediator.Send(query, cancellationToken);
        return Ok(products);
    }

    [HttpDelete("cleanup")]
    [Authorize(Roles = "Admin")]
    [RateLimit(5, 3600)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> DeleteOldActivities(
        [FromQuery] int daysToKeep = 90,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteOldActivitiesCommand(daysToKeep);
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }
}
