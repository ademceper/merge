using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Common;
using Merge.Application.DTOs.Notification;
using Merge.Application.Interfaces.Notification;
using Merge.API.Middleware;

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

    /// <summary>
    /// Kullanıcının bildirimlerini getirir (pagination ile)
    /// </summary>
    [HttpGet]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(PagedResult<NotificationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<NotificationDto>>> GetNotifications(
        [FromQuery] bool unreadOnly = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Pagination (ZORUNLU)
        if (pageSize > 100) pageSize = 100; // Max limit
        if (page < 1) page = 1;

        var userId = GetUserId();
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var notifications = await _notificationService.GetUserNotificationsAsync(userId, unreadOnly, page, pageSize, cancellationToken);
        return Ok(notifications);
    }

    /// <summary>
    /// Kullanıcının okunmamış bildirim sayısını getirir
    /// </summary>
    [HttpGet("unread-count")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<int>> GetUnreadCount(CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var count = await _notificationService.GetUnreadCountAsync(userId, cancellationToken);
        return Ok(new { count });
    }

    /// <summary>
    /// Bildirimi okundu olarak işaretler
    /// </summary>
    [HttpPost("{notificationId}/read")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> MarkAsRead(Guid notificationId, CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        
        // ✅ SECURITY: IDOR koruması - Kullanıcı sadece kendi bildirimlerini okuyabilir
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var notification = await _notificationService.GetByIdAsync(notificationId, cancellationToken);
        if (notification == null)
        {
            return NotFound();
        }
        
        if (notification.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }
        
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var result = await _notificationService.MarkAsReadAsync(notificationId, userId, cancellationToken);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    /// <summary>
    /// Tüm bildirimleri okundu olarak işaretler
    /// </summary>
    [HttpPost("read-all")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10/dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        await _notificationService.MarkAllAsReadAsync(userId, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Bildirimi siler
    /// </summary>
    [HttpDelete("{notificationId}")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Delete(Guid notificationId, CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        
        // ✅ SECURITY: IDOR koruması - Kullanıcı sadece kendi bildirimlerini silebilir
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var notification = await _notificationService.GetByIdAsync(notificationId, cancellationToken);
        if (notification == null)
        {
            return NotFound();
        }
        
        if (notification.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }
        
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var result = await _notificationService.DeleteNotificationAsync(notificationId, userId, cancellationToken);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }
}
