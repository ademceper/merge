using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Merge.Application.Common;
using Merge.Application.Configuration;
using Merge.Application.DTOs.Notification;
using Merge.Application.Notification.Commands.CreateNotification;
using Merge.Application.Notification.Commands.DeleteNotification;
using Merge.Application.Notification.Commands.MarkAllAsRead;
using Merge.Application.Notification.Commands.MarkAsRead;
using Merge.Application.Notification.Queries.GetNotificationById;
using Merge.Application.Notification.Queries.GetUnreadCount;
using Merge.Application.Notification.Queries.GetUserNotifications;
using Merge.API.Middleware;

namespace Merge.API.Controllers.Notification;

[ApiController]
[ApiVersion("1.0")] // ✅ BOLUM 4.1: API Versioning (ZORUNLU)
[Route("api/v1/notifications")]
[Authorize]
public class NotificationsController : BaseController
{
    private readonly IMediator _mediator;
    private readonly PaginationSettings _paginationSettings;

    public NotificationsController(
        IMediator mediator,
        IOptions<PaginationSettings> paginationSettings)
    {
        _mediator = mediator;
        _paginationSettings = paginationSettings.Value;
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
        // ✅ BOLUM 12.0: Magic Numbers YASAK - Configuration kullan
        if (pageSize > _paginationSettings.MaxPageSize) pageSize = _paginationSettings.MaxPageSize;
        if (page < 1) page = 1;

        var userId = GetUserId();
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var query = new GetUserNotificationsQuery(userId, unreadOnly, page, pageSize);
        var notifications = await _mediator.Send(query, cancellationToken);
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
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var query = new GetUnreadCountQuery(userId);
        var count = await _mediator.Send(query, cancellationToken);
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
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var notificationQuery = new GetNotificationByIdQuery(notificationId);
        var notification = await _mediator.Send(notificationQuery, cancellationToken);
        if (notification == null)
        {
            return NotFound();
        }
        
        if (notification.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }
        
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var command = new MarkAsReadCommand(notificationId, userId);
        var result = await _mediator.Send(command, cancellationToken);
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
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var command = new MarkAllAsReadCommand(userId);
        await _mediator.Send(command, cancellationToken);
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
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var notificationQuery = new GetNotificationByIdQuery(notificationId);
        var notification = await _mediator.Send(notificationQuery, cancellationToken);
        if (notification == null)
        {
            return NotFound();
        }
        
        if (notification.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }
        
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var command = new DeleteNotificationCommand(notificationId, userId);
        var result = await _mediator.Send(command, cancellationToken);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }
}
