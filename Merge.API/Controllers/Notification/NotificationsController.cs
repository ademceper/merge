using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.Notification;
using Merge.Application.DTOs.Notification;
using Merge.Application.Common;


namespace Merge.API.Controllers.Notification;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController : BaseController
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    // ✅ PERFORMANCE: Pagination ekle (BEST_PRACTICES_ANALIZI.md - BOLUM 3.1.4)
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<NotificationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<NotificationDto>>> GetNotifications(
        [FromQuery] bool unreadOnly = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (pageSize > 100) pageSize = 100; // Max limit
        var userId = GetUserId();
        var notifications = await _notificationService.GetUserNotificationsAsync(userId, unreadOnly, page, pageSize);
        return Ok(notifications);
    }

    [HttpGet("unread-count")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<int>> GetUnreadCount()
    {
        var userId = GetUserId();
        var count = await _notificationService.GetUnreadCountAsync(userId);
        return Ok(new { count });
    }

    [HttpPost("{notificationId}/read")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> MarkAsRead(Guid notificationId)
    {
        var userId = GetUserId();
        var result = await _notificationService.MarkAsReadAsync(notificationId, userId);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpPost("read-all")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var userId = GetUserId();
        await _notificationService.MarkAllAsReadAsync(userId);
        return NoContent();
    }

    [HttpDelete("{notificationId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete(Guid notificationId)
    {
        var userId = GetUserId();
        var result = await _notificationService.DeleteNotificationAsync(notificationId, userId);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }
}

