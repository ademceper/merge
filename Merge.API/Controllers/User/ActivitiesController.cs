using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.User;
using Merge.Application.DTOs.User;

namespace Merge.API.Controllers.User;

[ApiController]
[Route("api/user/activities")]
public class ActivitiesController : BaseController
{
    private readonly IUserActivityService _activityService;

    public ActivitiesController(IUserActivityService activityService)
    {
        _activityService = activityService;
    }

    [HttpPost("log")]
    [AllowAnonymous]
    public async Task<IActionResult> LogActivity([FromBody] CreateActivityLogDto activityDto)
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

        await _activityService.LogActivityAsync(activityDto, ipAddress, userAgent);
        return Ok();
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<UserActivityLogDto>> GetActivityById(Guid id)
    {
        var activity = await _activityService.GetActivityByIdAsync(id);
        if (activity == null)
        {
            return NotFound();
        }
        return Ok(activity);
    }

    [HttpGet("my-activities")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<UserActivityLogDto>>> GetMyActivities([FromQuery] int days = 30)
    {
        var userId = GetUserId();
        var activities = await _activityService.GetUserActivitiesAsync(userId, days);
        return Ok(activities);
    }

    [HttpGet("user/{userId}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<IEnumerable<UserActivityLogDto>>> GetUserActivities(
        Guid userId,
        [FromQuery] int days = 30)
    {
        var activities = await _activityService.GetUserActivitiesAsync(userId, days);
        return Ok(activities);
    }

    [HttpPost("search")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<IEnumerable<UserActivityLogDto>>> SearchActivities(
        [FromBody] ActivityFilterDto filter)
    {
        var activities = await _activityService.GetActivitiesAsync(filter);
        return Ok(activities);
    }

    [HttpGet("stats")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<ActivityStatsDto>> GetActivityStats([FromQuery] int days = 30)
    {
        var stats = await _activityService.GetActivityStatsAsync(days);
        return Ok(stats);
    }

    [HttpGet("sessions/{userId}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<IEnumerable<UserSessionDto>>> GetUserSessions(
        Guid userId,
        [FromQuery] int days = 7)
    {
        var sessions = await _activityService.GetUserSessionsAsync(userId, days);
        return Ok(sessions);
    }

    [HttpGet("my-sessions")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<UserSessionDto>>> GetMySessions([FromQuery] int days = 7)
    {
        var userId = GetUserId();
        var sessions = await _activityService.GetUserSessionsAsync(userId, days);
        return Ok(sessions);
    }

    [HttpGet("popular-products")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<IEnumerable<PopularProductDto>>> GetMostViewedProducts(
        [FromQuery] int days = 30,
        [FromQuery] int topN = 10)
    {
        var products = await _activityService.GetMostViewedProductsAsync(days, topN);
        return Ok(products);
    }

    [HttpDelete("cleanup")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteOldActivities([FromQuery] int daysToKeep = 90)
    {
        await _activityService.DeleteOldActivitiesAsync(daysToKeep);
        return Ok(new { message = $"Activities older than {daysToKeep} days deleted successfully" });
    }
}

