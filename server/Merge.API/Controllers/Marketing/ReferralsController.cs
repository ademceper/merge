using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Swashbuckle.AspNetCore.Annotations;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Common;
using Merge.API.Middleware;
using Merge.Application.Marketing.Queries.GetMyReferralCode;
using Merge.Application.Marketing.Queries.GetMyReferrals;
using Merge.Application.Marketing.Queries.GetReferralStats;
using Merge.Application.Marketing.Commands.ApplyReferralCode;
using Merge.Application.Marketing.Commands.CreateReferralCode;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.API.Controllers.Marketing;

/// <summary>
/// Referrals API endpoints.
/// Referans programı işlemlerini yönetir.
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/marketing/referrals")]
[Authorize]
[Tags("Referrals")]
public class ReferralsController(
    IMediator mediator,
    IOptions<MarketingSettings> marketingSettings) : BaseController
{
    private readonly MarketingSettings _marketingSettings = marketingSettings.Value;

    [HttpGet("my-code")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(ReferralCodeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ReferralCodeDto>> GetMyReferralCode(
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }
        
        var query = new GetMyReferralCodeQuery(userId);
        var code = await mediator.Send(query, cancellationToken);
        
        if (code is null)
        {
            var createCommand = new CreateReferralCodeCommand(userId);
            code = await mediator.Send(createCommand, cancellationToken);
        }
        
        return Ok(code);
    }

    [HttpGet("my-referrals")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(PagedResult<ReferralDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<ReferralDto>>> GetMyReferrals(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (pageSize > _marketingSettings.MaxPageSize) pageSize = _marketingSettings.MaxPageSize;

        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }
        
        var query = new GetMyReferralsQuery(userId, PageNumber: page, PageSize: pageSize);
        var referrals = await mediator.Send(query, cancellationToken);
        return Ok(referrals);
    }

    [HttpPost("apply")]
    [RateLimit(5, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> ApplyReferralCode(
        [FromBody] string code,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return BadRequest("Referans kodu boş olamaz.");
        }

        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }
        
        var command = new ApplyReferralCodeCommand(userId, code);
        var success = await mediator.Send(command, cancellationToken);
        
        return success ? NoContent() : BadRequest(new { message = "Geçersiz kod" });
    }

    [HttpGet("stats")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(ReferralStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ReferralStatsDto>> GetStats(
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }
        
        var query = new GetReferralStatsQuery(userId);
        var stats = await mediator.Send(query, cancellationToken);
        return Ok(stats);
    }
}
