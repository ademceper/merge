using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Swashbuckle.AspNetCore.Annotations;
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
using Merge.Application.Marketing.Commands.CreateEmailTemplate;
using Merge.Application.Marketing.Queries.GetEmailTemplateById;
using Merge.Application.Marketing.Queries.GetAllEmailTemplates;
using Merge.Application.Marketing.Commands.UpdateEmailTemplate;
using Merge.Application.Marketing.Commands.DeleteEmailTemplate;
using Merge.Application.Marketing.Commands.SubscribeEmail;
using Merge.Application.Marketing.Commands.UnsubscribeEmail;
using Merge.Application.Marketing.Queries.GetEmailSubscriberById;
using Merge.Application.Marketing.Queries.GetEmailSubscriberByEmail;
using Merge.Application.Marketing.Queries.GetAllEmailSubscribers;
using Merge.Application.Marketing.Commands.UpdateEmailSubscriber;
using Merge.Application.Marketing.Commands.BulkImportEmailSubscribers;
using Merge.Application.Marketing.Queries.GetCampaignAnalytics;
using Merge.Application.Marketing.Queries.GetCampaignStats;
using Merge.Application.Marketing.Commands.RecordEmailOpen;
using Merge.Application.Marketing.Commands.RecordEmailClick;
using Merge.Application.Marketing.Commands.CreateEmailAutomation;
using Merge.Application.Marketing.Queries.GetAllEmailAutomations;
using Merge.Application.Marketing.Commands.ToggleEmailAutomation;
using Merge.Application.Marketing.Commands.DeleteEmailAutomation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.API.Controllers.Marketing;

[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/marketing/email-campaigns")]
[Authorize]
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
            return NotFound();
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
            return NotFound();
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
            return NotFound();
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
            return NotFound();
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
            return NotFound();
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
            return NotFound();
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


    [HttpPost("templates")]
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

    [HttpGet("templates/{id}")]
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
            return NotFound();
        }

        return Ok(template);
    }

    [HttpGet("templates")]
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

    [HttpPut("templates/{id}")]
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

    [HttpDelete("templates/{id}")]
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
            return NotFound();
        }

        return NoContent();
    }


    [HttpPost("subscribers")]
    [AllowAnonymous]
    [RateLimit(10, 60)]
    [ProducesResponseType(typeof(EmailSubscriberDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<EmailSubscriberDto>> Subscribe(
        [FromBody] CreateEmailSubscriberDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new SubscribeEmailCommand(
            dto.Email,
            dto.FirstName,
            dto.LastName,
            dto.Source,
            dto.Tags,
            dto.CustomFields);

        var subscriber = await mediator.Send(command, cancellationToken);
        return Ok(subscriber);
    }

    [HttpPost("subscribers/unsubscribe")]
    [AllowAnonymous]
    [RateLimit(10, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Unsubscribe(
        [FromBody] string email,
        CancellationToken cancellationToken = default)
    {
        var command = new UnsubscribeEmailCommand(email);
        var success = await mediator.Send(command, cancellationToken);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpGet("subscribers/{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(EmailSubscriberDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<EmailSubscriberDto>> GetSubscriber(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetEmailSubscriberByIdQuery(id);
        var subscriber = await mediator.Send(query, cancellationToken);

        if (subscriber == null)
        {
            return NotFound();
        }

        return Ok(subscriber);
    }

    [HttpGet("subscribers/by-email/{email}")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(EmailSubscriberDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<EmailSubscriberDto>> GetSubscriberByEmail(
        string email,
        CancellationToken cancellationToken = default)
    {
        var query = new GetEmailSubscriberByEmailQuery(email);
        var subscriber = await mediator.Send(query, cancellationToken);

        if (subscriber == null)
        {
            return NotFound();
        }

        return Ok(subscriber);
    }

    [HttpGet("subscribers")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(PagedResult<EmailSubscriberDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<EmailSubscriberDto>>> GetSubscribers(
        [FromQuery] bool? isSubscribed = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        if (pageSize > _marketingSettings.MaxPageSize) pageSize = _marketingSettings.MaxPageSize;

        var query = new GetAllEmailSubscribersQuery(isSubscribed, PageNumber: page, PageSize: pageSize);
        var subscribers = await mediator.Send(query, cancellationToken);
        return Ok(subscribers);
    }

    [HttpPut("subscribers/{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(20, 60)]
    [ProducesResponseType(typeof(EmailSubscriberDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<EmailSubscriberDto>> UpdateSubscriber(
        Guid id,
        [FromBody] UpdateEmailSubscriberDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateEmailSubscriberCommand(
            id,
            dto.FirstName,
            dto.LastName,
            dto.Source,
            dto.Tags,
            dto.CustomFields,
            dto.IsSubscribed);

        var subscriber = await mediator.Send(command, cancellationToken);
        return Ok(subscriber);
    }

    [HttpPost("subscribers/bulk-import")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(5, 60)]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<int>> BulkImportSubscribers(
        [FromBody] BulkImportSubscribersDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new BulkImportEmailSubscribersCommand(dto.Subscribers);
        var count = await mediator.Send(command, cancellationToken);
        return Ok(count);
    }


    [HttpGet("{campaignId}/analytics")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(EmailCampaignAnalyticsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<EmailCampaignAnalyticsDto>> GetCampaignAnalytics(
        Guid campaignId,
        CancellationToken cancellationToken = default)
    {
        var query = new GetCampaignAnalyticsQuery(campaignId);
        var analytics = await mediator.Send(query, cancellationToken);

        if (analytics == null)
        {
            return NotFound();
        }

        return Ok(analytics);
    }

    [HttpGet("stats")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(EmailCampaignStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<EmailCampaignStatsDto>> GetCampaignStats(
        CancellationToken cancellationToken = default)
    {
        var query = new GetCampaignStatsQuery();
        var stats = await mediator.Send(query, cancellationToken);
        return Ok(stats);
    }

    [HttpPost("{campaignId}/subscribers/{subscriberId}/open")]
    [AllowAnonymous]
    [RateLimit(100, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> RecordEmailOpen(
        Guid campaignId,
        Guid subscriberId,
        CancellationToken cancellationToken = default)
    {
        var command = new RecordEmailOpenCommand(campaignId, subscriberId);
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpPost("{campaignId}/subscribers/{subscriberId}/click")]
    [AllowAnonymous]
    [RateLimit(100, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> RecordEmailClick(
        Guid campaignId,
        Guid subscriberId,
        CancellationToken cancellationToken = default)
    {
        var command = new RecordEmailClickCommand(campaignId, subscriberId);
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }


    [HttpPost("automations")]
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

    [HttpGet("automations")]
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

    [HttpPatch("automations/{id}/toggle")]
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

    [HttpDelete("automations/{id}")]
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
