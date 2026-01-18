using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Notification;
using Merge.Application.Common;
using Merge.Application.Configuration;
using Merge.Application.Notification.Commands.CreateNotificationFromTemplate;
using Merge.Application.Notification.Commands.CreateTemplate;
using Merge.Application.Notification.Commands.DeleteTemplate;
using Merge.Application.Notification.Commands.UpdateTemplate;
using Merge.Application.Notification.Queries.GetTemplate;
using Merge.Application.Notification.Queries.GetTemplateByType;
using Merge.Application.Notification.Queries.GetTemplates;
using Merge.API.Middleware;
using Merge.Domain.Enums;

namespace Merge.API.Controllers.Notification;

/// <summary>
/// Notification Template API endpoints.
/// Bildirim şablonlarını yönetir.
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/notifications/templates")]
[Authorize(Roles = "Admin,Manager")]
[Tags("NotificationTemplates")]
public class NotificationTemplatesController(
    IMediator mediator,
    IOptions<PaginationSettings> paginationSettings) : BaseController
{
    private readonly PaginationSettings _paginationSettings = paginationSettings.Value;

    [HttpPost]
    [RateLimit(10, 60)]
    [ProducesResponseType(typeof(NotificationTemplateDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<NotificationTemplateDto>> CreateTemplate(
        [FromBody] CreateNotificationTemplateDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateTemplateCommand(dto);
        var template = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetTemplate), new { id = template.Id }, template);
    }

    [HttpGet("{id}")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(NotificationTemplateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<NotificationTemplateDto>> GetTemplate(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetTemplateQuery(id);
        var template = await mediator.Send(query, cancellationToken);
        if (template == null)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
        }
        return Ok(template);
    }

    [HttpGet("type/{type}")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(NotificationTemplateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<NotificationTemplateDto>> GetTemplateByType(
        string type,
        CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<NotificationType>(type, true, out var notificationTypeEnum))
        {
            return BadRequest("Geçersiz notification type.");
        }

        var query = new GetTemplateByTypeQuery(notificationTypeEnum);
        var template = await mediator.Send(query, cancellationToken);
        if (template == null)
        {
            return Problem($"Template not found for type: {type}", "Not Found", StatusCodes.Status404NotFound);
        }
        return Ok(template);
    }

    [HttpGet]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(PagedResult<NotificationTemplateDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<NotificationTemplateDto>>> GetTemplates(
        [FromQuery] string? type = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (pageSize > _paginationSettings.MaxPageSize) pageSize = _paginationSettings.MaxPageSize;
        if (page < 1) page = 1;

        NotificationType? notificationTypeEnum = null;
        if (!string.IsNullOrEmpty(type))
        {
            if (!Enum.TryParse<NotificationType>(type, true, out var parsedType))
            {
                return BadRequest("Geçersiz notification type.");
            }
            notificationTypeEnum = parsedType;
        }

        var query = new GetTemplatesQuery(notificationTypeEnum, page, pageSize);
        var result = await mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id}")]
    [RateLimit(10, 60)]
    [ProducesResponseType(typeof(NotificationTemplateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<NotificationTemplateDto>> UpdateTemplate(
        Guid id,
        [FromBody] UpdateNotificationTemplateDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateTemplateCommand(id, dto);
        var template = await mediator.Send(command, cancellationToken);
        return Ok(template);
    }

    /// <summary>
    /// Bildirim şablonunu kısmi olarak günceller (PATCH)
    /// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
    /// </summary>
    [HttpPatch("{id}")]
    [RateLimit(10, 60)]
    [ProducesResponseType(typeof(NotificationTemplateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<NotificationTemplateDto>> PatchTemplate(
        Guid id,
        [FromBody] PatchNotificationTemplateDto patchDto,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateTemplateCommand(id, new UpdateNotificationTemplateDto(
            patchDto.Name,
            patchDto.Description,
            patchDto.Type,
            patchDto.TitleTemplate,
            patchDto.MessageTemplate,
            patchDto.LinkTemplate,
            patchDto.IsActive,
            patchDto.Variables,
            patchDto.DefaultData));
        var template = await mediator.Send(command, cancellationToken);
        return Ok(template);
    }

    [HttpDelete("{id}")]
    [RateLimit(5, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> DeleteTemplate(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteTemplateCommand(id);
        var success = await mediator.Send(command, cancellationToken);
        if (!success)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
        }
        return NoContent();
    }

    [HttpPost("create-notification")]
    [Authorize]
    [RateLimit(30, 60)]
    [ProducesResponseType(typeof(NotificationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<NotificationDto>> CreateNotificationFromTemplate(
        [FromBody] CreateNotificationFromTemplateDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateNotificationFromTemplateCommand(dto.UserId, dto.TemplateType, dto.Variables);
        var notification = await mediator.Send(command, cancellationToken);
        return Ok(notification);
    }
}
