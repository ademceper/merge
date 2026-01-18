using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Merge.Application.DTOs.Marketing;
using Merge.API.Middleware;
using Merge.Application.Marketing.Queries.GetCampaignAnalytics;
using Merge.Application.Marketing.Queries.GetCampaignStats;
using Merge.Application.Marketing.Commands.RecordEmailOpen;
using Merge.Application.Marketing.Commands.RecordEmailClick;
using Merge.Application.Exceptions;

namespace Merge.API.Controllers.Marketing.EmailCampaignAnalytics;

/// <summary>
/// Email Campaign Analytics API endpoints.
/// E-posta kampanya analitiklerini yönetir.
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/marketing/email-campaigns")]
[Authorize]
[Tags("EmailCampaignAnalytics")]
public class EmailCampaignAnalyticsController(IMediator mediator) : BaseController
{
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
        var analytics = await mediator.Send(query, cancellationToken)
            ?? throw new NotFoundException("EmailCampaignAnalytics", campaignId);

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
}
