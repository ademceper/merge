using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Common;
using Merge.API.Middleware;
using Merge.Application.Marketing.Commands.CreateEmailCampaign;
using Merge.Application.Marketing.Queries.GetEmailCampaignById;
using Merge.Application.Marketing.Queries.GetAllEmailCampaigns;
using Merge.Application.Marketing.Commands.UpdateEmailCampaign;
using Merge.Application.Marketing.Commands.DeleteEmailCampaign;
using Merge.Application.Marketing.Commands.ScheduleEmailCampaign;
using Merge.Application.Marketing.Commands.SendEmailCampaign;
using Merge.Application.Marketing.Commands.PauseEmailCampaign;
using Merge.Application.Marketing.Commands.CancelEmailCampaign;
using Merge.Application.Marketing.Commands.SendTestEmail;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.API.Controllers.Marketing.EmailCampaigns;

/// <summary>
/// Email Campaigns API endpoints.
/// E-posta kampanyalarını yönetir.
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/marketing/email-campaigns")]
[Authorize]
[Tags("EmailCampaigns")]
public class EmailCampaignsController(
    IMediator mediator,
    IOptions<MarketingSettings> marketingSettings) : BaseController
{
    private readonly MarketingSettings _marketingSettings = marketingSettings.Value;

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(10, 60)]
    [ProducesResponseType(typeof(EmailCampaignDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<EmailCampaignDto>> CreateCampaign(
        [FromBody] CreateEmailCampaignDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateEmailCampaignCommand(
            dto.Name,
            dto.Subject,
            dto.FromName,
            dto.FromEmail,
            dto.ReplyToEmail,
            dto.TemplateId,
            dto.Content,
            dto.Type ?? "Promotional",
            dto.ScheduledAt,
            dto.TargetSegment ?? "All",
            dto.Tags);

        var campaign = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetCampaign), new { id = campaign.Id }, campaign);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(EmailCampaignDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<EmailCampaignDto>> GetCampaign(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetEmailCampaignByIdQuery(id);
        var campaign = await mediator.Send(query, cancellationToken);

        if (campaign == null)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
        }

        return Ok(campaign);
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(PagedResult<EmailCampaignDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<EmailCampaignDto>>> GetCampaigns(
        [FromQuery] string? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (pageSize > _marketingSettings.MaxPageSize) pageSize = _marketingSettings.MaxPageSize;

        var query = new GetAllEmailCampaignsQuery(status, PageNumber: page, PageSize: pageSize);
        var campaigns = await mediator.Send(query, cancellationToken);
        return Ok(campaigns);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(20, 60)]
    [ProducesResponseType(typeof(EmailCampaignDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<EmailCampaignDto>> UpdateCampaign(
        Guid id,
        [FromBody] UpdateEmailCampaignDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateEmailCampaignCommand(
            id,
            dto.Name,
            dto.Subject,
            dto.FromName,
            dto.FromEmail,
            dto.ReplyToEmail,
            dto.TemplateId,
            dto.Content,
            dto.ScheduledAt,
            dto.TargetSegment);

        var campaign = await mediator.Send(command, cancellationToken);
        return Ok(campaign);
    }

    /// <summary>
    /// Email kampanyasını kısmi olarak günceller (PATCH)
    /// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
    /// </summary>
    [HttpPatch("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(20, 60)]
    [ProducesResponseType(typeof(EmailCampaignDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<EmailCampaignDto>> PatchCampaign(
        Guid id,
        [FromBody] PatchEmailCampaignDto patchDto,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateEmailCampaignCommand(
            id,
            patchDto.Name,
            patchDto.Subject,
            patchDto.FromName,
            patchDto.FromEmail,
            patchDto.ReplyToEmail,
            patchDto.TemplateId,
            patchDto.Content,
            patchDto.ScheduledAt,
            patchDto.TargetSegment);

        var campaign = await mediator.Send(command, cancellationToken);
        return Ok(campaign);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(10, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> DeleteCampaign(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteEmailCampaignCommand(id);
        var success = await mediator.Send(command, cancellationToken);

        if (!success)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
        }

        return NoContent();
    }

    [HttpPost("{id}/schedule")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(10, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> ScheduleCampaign(
        Guid id,
        [FromBody] DateTime scheduledAt,
        CancellationToken cancellationToken = default)
    {
        var command = new ScheduleEmailCampaignCommand(id, scheduledAt);
        var success = await mediator.Send(command, cancellationToken);

        if (!success)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
        }

        return NoContent();
    }

    [HttpPost("{id}/send")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(5, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> SendCampaign(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var command = new SendEmailCampaignCommand(id);
        var success = await mediator.Send(command, cancellationToken);

        if (!success)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
        }

        return NoContent();
    }

    [HttpPost("{id}/pause")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(10, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> PauseCampaign(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var command = new PauseEmailCampaignCommand(id);
        var success = await mediator.Send(command, cancellationToken);

        if (!success)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
        }

        return NoContent();
    }

    [HttpPost("{id}/cancel")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(10, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> CancelCampaign(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var command = new CancelEmailCampaignCommand(id);
        var success = await mediator.Send(command, cancellationToken);

        if (!success)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
        }

        return NoContent();
    }

    [HttpPost("test-email")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(10, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> SendTestEmail(
        [FromBody] SendTestEmailDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new SendTestEmailCommand(dto.CampaignId, dto.TestEmail);
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }
}
