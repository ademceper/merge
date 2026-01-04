using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.Marketing;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Common;
using Merge.API.Middleware;

namespace Merge.API.Controllers.Marketing;

[ApiController]
[Route("api/marketing/referrals")]
[Authorize]
public class ReferralsController : BaseController
{
    private readonly IReferralService _referralService;

    public ReferralsController(IReferralService referralService)
    {
        _referralService = referralService;
    }

    /// <summary>
    /// Kullanıcının referans kodunu getirir
    /// </summary>
    [HttpGet("my-code")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(ReferralCodeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ReferralCodeDto>> GetMyReferralCode(
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var code = await _referralService.GetMyReferralCodeAsync(userId, cancellationToken);
        return Ok(code);
    }

    /// <summary>
    /// Kullanıcının referanslarını getirir (pagination ile)
    /// </summary>
    [HttpGet("my-referrals")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(PagedResult<ReferralDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<ReferralDto>>> GetMyReferrals(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Pagination (ZORUNLU)
        if (pageSize > 100) pageSize = 100; // Max limit

        var userId = GetUserId();
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var referrals = await _referralService.GetMyReferralsAsync(userId, page, pageSize, cancellationToken);
        return Ok(referrals);
    }

    /// <summary>
    /// Referans kodunu uygular
    /// </summary>
    [HttpPost("apply")]
    [RateLimit(5, 60)] // ✅ BOLUM 3.3: Rate Limiting - 5 istek / dakika (kritik işlem)
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

        var userId = GetUserId();
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var success = await _referralService.ApplyReferralCodeAsync(userId, code, cancellationToken);
        return success ? NoContent() : BadRequest(new { message = "Geçersiz kod" });
    }

    /// <summary>
    /// Kullanıcının referans istatistiklerini getirir
    /// </summary>
    [HttpGet("stats")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(ReferralStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ReferralStatsDto>> GetStats(
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var stats = await _referralService.GetReferralStatsAsync(userId, cancellationToken);
        return Ok(stats);
    }
}
