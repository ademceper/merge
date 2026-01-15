using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Common;
using Merge.API.Middleware;
using Merge.Application.Marketing.Commands.CreateEmailAutomation;
using Merge.Application.Marketing.Queries.GetAllEmailAutomations;
using Merge.Application.Marketing.Commands.ToggleEmailAutomation;
using Merge.Application.Marketing.Commands.DeleteEmailAutomation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.API.Controllers.Marketing.EmailAutomations;

[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/marketing/email-automations")]
[Authorize]
public class EmailAutomationsController(
    IMediator mediator,
    IOptions<MarketingSettings> marketingSettings) : BaseController
{
    private readonly MarketingSettings _marketingSettings = marketingSettings.Value;

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(10, 60)]
    [ProducesResponseType(typeof(EmailAutomationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<EmailAutomationDto>> CreateAutomation(
        [FromBody] CreateEmailAutomationDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateEmailAutomationCommand(
            dto.Name,
            dto.Description ?? string.Empty,
            dto.Type,
            dto.TemplateId,
            dto.DelayHours,
            dto.TriggerConditions);

        var automation = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetAutomations), new { page = 1, pageSize = 20 }, automation);
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(PagedResult<EmailAutomationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<EmailAutomationDto>>> GetAutomations(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (pageSize > _marketingSettings.MaxPageSize) pageSize = _marketingSettings.MaxPageSize;

        var query = new GetAllEmailAutomationsQuery(PageNumber: page, PageSize: pageSize);
        var automations = await mediator.Send(query, cancellationToken);
        return Ok(automations);
    }

    [HttpPatch("{id}/toggle")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(20, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> ToggleAutomation(
        Guid id,
        [FromBody] bool isActive,
        CancellationToken cancellationToken = default)
    {
        var command = new ToggleEmailAutomationCommand(id, isActive);
        var success = await mediator.Send(command, cancellationToken);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(10, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> DeleteAutomation(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteEmailAutomationCommand(id);
        var success = await mediator.Send(command, cancellationToken);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }
}
