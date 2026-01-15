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
[ApiVersion("1.0")]
[Route("api/v1/notifications")]
[Authorize]
public class NotificationsController(
    IMediator mediator,
    IOptions<PaginationSettings> paginationSettings) : BaseController
{
    private readonly PaginationSettings _paginationSettings = paginationSettings.Value;

    [HttpGet]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(PagedResult<NotificationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<NotificationDto>>> GetNotifications(
        [FromQuery] bool unreadOnly = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (pageSize > _paginationSettings.MaxPageSize) pageSize = _paginationSettings.MaxPageSize;
        if (page < 1) page = 1;
        var userId = GetUserId();
        var query = new GetUserNotificationsQuery(userId, unreadOnly, page, pageSize);
        var notifications = await mediator.Send(query, cancellationToken);
        return Ok(notifications);
    }

    [HttpGet("unread-count")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<int>> GetUnreadCount(CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var query = new GetUnreadCountQuery(userId);
        var count = await mediator.Send(query, cancellationToken);
        return Ok(new { count });
    }

    [HttpPost("{notificationId}/read")]
    [RateLimit(30, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> MarkAsRead(Guid notificationId, CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var notificationQuery = new GetNotificationByIdQuery(notificationId);
        var notification = await mediator.Send(notificationQuery, cancellationToken);
        if (notification == null)
        {
            return NotFound();
        }
        
        if (notification.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }
        
        var command = new MarkAsReadCommand(notificationId, userId);
        var result = await mediator.Send(command, cancellationToken);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpPost("read-all")]
    [RateLimit(10, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var command = new MarkAllAsReadCommand(userId);
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{notificationId}")]
    [RateLimit(30, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Delete(Guid notificationId, CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var notificationQuery = new GetNotificationByIdQuery(notificationId);
        var notification = await mediator.Send(notificationQuery, cancellationToken);
        if (notification == null)
        {
            return NotFound();
        }
        
        if (notification.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }
        
        var command = new DeleteNotificationCommand(notificationId, userId);
        var result = await mediator.Send(command, cancellationToken);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }
}
