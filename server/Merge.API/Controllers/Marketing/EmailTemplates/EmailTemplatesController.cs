using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Common;
using Merge.API.Middleware;
using Merge.Application.Marketing.Commands.CreateEmailTemplate;
using Merge.Application.Marketing.Queries.GetEmailTemplateById;
using Merge.Application.Marketing.Queries.GetAllEmailTemplates;
using Merge.Application.Marketing.Commands.UpdateEmailTemplate;
using Merge.Application.Marketing.Commands.DeleteEmailTemplate;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.API.Controllers.Marketing.EmailTemplates;

/// <summary>
/// Email Templates API endpoints.
/// E-posta şablonlarını yönetir.
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/marketing/email-templates")]
[Authorize]
[Tags("EmailTemplates")]
public class EmailTemplatesController(
    IMediator mediator,
    IOptions<MarketingSettings> marketingSettings) : BaseController
{
    private readonly MarketingSettings _marketingSettings = marketingSettings.Value;

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(10, 60)]
    [ProducesResponseType(typeof(EmailTemplateDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<EmailTemplateDto>> CreateTemplate(
        [FromBody] CreateEmailTemplateDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateEmailTemplateCommand(
            dto.Name,
            dto.Description ?? string.Empty,
            dto.Subject,
            dto.HtmlContent,
            dto.TextContent ?? string.Empty,
            dto.Type ?? "Custom",
            dto.Variables,
            dto.Thumbnail);

        var template = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetTemplate), new { id = template.Id }, template);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(EmailTemplateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<EmailTemplateDto>> GetTemplate(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetEmailTemplateByIdQuery(id);
        var template = await mediator.Send(query, cancellationToken);

        if (template == null)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
        }

        return Ok(template);
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(PagedResult<EmailTemplateDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<EmailTemplateDto>>> GetTemplates(
        [FromQuery] string? type = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (pageSize > _marketingSettings.MaxPageSize) pageSize = _marketingSettings.MaxPageSize;

        var query = new GetAllEmailTemplatesQuery(type, PageNumber: page, PageSize: pageSize);
        var templates = await mediator.Send(query, cancellationToken);
        return Ok(templates);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(20, 60)]
    [ProducesResponseType(typeof(EmailTemplateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<EmailTemplateDto>> UpdateTemplate(
        Guid id,
        [FromBody] CreateEmailTemplateDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateEmailTemplateCommand(
            id,
            dto.Name,
            dto.Description,
            dto.Subject,
            dto.HtmlContent,
            dto.TextContent,
            dto.Type,
            dto.Variables,
            dto.Thumbnail,
            null);

        var template = await mediator.Send(command, cancellationToken);
        return Ok(template);
    }

    /// <summary>
    /// Email şablonunu kısmi olarak günceller (PATCH)
    /// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
    /// </summary>
    [HttpPatch("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(20, 60)]
    [ProducesResponseType(typeof(EmailTemplateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<EmailTemplateDto>> PatchTemplate(
        Guid id,
        [FromBody] PatchEmailTemplateDto patchDto,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateEmailTemplateCommand(
            id,
            patchDto.Name,
            patchDto.Description,
            patchDto.Subject,
            patchDto.HtmlContent,
            patchDto.TextContent,
            patchDto.Type,
            patchDto.Variables,
            patchDto.Thumbnail,
            patchDto.IsActive);

        var template = await mediator.Send(command, cancellationToken);
        return Ok(template);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(10, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> DeleteTemplate(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteEmailTemplateCommand(id);
        var success = await mediator.Send(command, cancellationToken);

        if (!success)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
        }

        return NoContent();
    }
}
