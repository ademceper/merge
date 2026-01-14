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

[ApiController]
[ApiVersion("1.0")] // ✅ BOLUM 4.1: API Versioning (ZORUNLU)
[Route("api/v1/notifications/templates")]
[Authorize(Roles = "Admin,Manager")]
public class NotificationTemplatesController : BaseController
{
    private readonly IMediator _mediator;
    private readonly PaginationSettings _paginationSettings;

    public NotificationTemplatesController(
        IMediator mediator,
        IOptions<PaginationSettings> paginationSettings)
    {
        _mediator = mediator;
        _paginationSettings = paginationSettings.Value;
    }

    /// <summary>
    /// Yeni bildirim şablonu oluşturur (Admin, Manager)
    /// </summary>
    [HttpPost]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10/dakika
    [ProducesResponseType(typeof(NotificationTemplateDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<NotificationTemplateDto>> CreateTemplate(
        [FromBody] CreateNotificationTemplateDto dto,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder, manuel ValidateModelState() gereksiz
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var command = new CreateTemplateCommand(dto);
        var template = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetTemplate), new { id = template.Id }, template);
    }

    /// <summary>
    /// Bildirim şablonu detaylarını getirir (Admin, Manager)
    /// </summary>
    [HttpGet("{id}")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(NotificationTemplateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<NotificationTemplateDto>> GetTemplate(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var query = new GetTemplateQuery(id);
        var template = await _mediator.Send(query, cancellationToken);
        if (template == null)
        {
            return NotFound();
        }
        return Ok(template);
    }

    /// <summary>
    /// Tip'e göre bildirim şablonu getirir (Admin, Manager)
    /// </summary>
    [HttpGet("type/{type}")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
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

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var query = new GetTemplateByTypeQuery(notificationTypeEnum);
        var template = await _mediator.Send(query, cancellationToken);
        if (template == null)
        {
            return NotFound(new { message = $"Template not found for type: {type}" });
        }
        return Ok(template);
    }

    /// <summary>
    /// Tüm bildirim şablonlarını getirir (pagination ile) (Admin, Manager)
    /// </summary>
    [HttpGet]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
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
        // ✅ BOLUM 3.4: Pagination (ZORUNLU)
        // ✅ BOLUM 12.0: Magic Numbers YASAK - Configuration kullan
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

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var query = new GetTemplatesQuery(notificationTypeEnum, page, pageSize);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Bildirim şablonu günceller (Admin, Manager)
    /// </summary>
    [HttpPut("{id}")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10/dakika
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
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder, manuel ValidateModelState() gereksiz
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var command = new UpdateTemplateCommand(id, dto);
        var template = await _mediator.Send(command, cancellationToken);
        return Ok(template);
    }

    /// <summary>
    /// Bildirim şablonu siler (Admin, Manager)
    /// </summary>
    [HttpDelete("{id}")]
    [RateLimit(5, 60)] // ✅ BOLUM 3.3: Rate Limiting - 5/dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> DeleteTemplate(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var command = new DeleteTemplateCommand(id);
        var success = await _mediator.Send(command, cancellationToken);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    /// <summary>
    /// Şablondan bildirim oluşturur (Admin, Manager)
    /// </summary>
    [HttpPost("create-notification")]
    [Authorize]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika
    [ProducesResponseType(typeof(NotificationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<NotificationDto>> CreateNotificationFromTemplate(
        [FromBody] CreateNotificationFromTemplateDto dto,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder, manuel ValidateModelState() gereksiz
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var command = new CreateNotificationFromTemplateCommand(dto.UserId, dto.TemplateType, dto.Variables);
        var notification = await _mediator.Send(command, cancellationToken);
        return Ok(notification);
    }
}
