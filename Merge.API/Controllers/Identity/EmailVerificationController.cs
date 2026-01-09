using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Merge.Application.Identity.Commands.VerifyEmail;
using Merge.Application.Identity.Commands.ResendVerificationEmail;
using Merge.Application.Identity.Queries.IsEmailVerified;
using Merge.API.Middleware;

namespace Merge.API.Controllers.Identity;

[ApiController]
[ApiVersion("1.0")] // ✅ BOLUM 4.1: API Versioning (ZORUNLU)
[Route("api/v{version:apiVersion}/email-verification")]
public class EmailVerificationController : BaseController
{
    private readonly IMediator _mediator;

    public EmailVerificationController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// E-posta doğrulama token'ını doğrular
    /// </summary>
    [HttpPost("verify")]
    [AllowAnonymous]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> VerifyEmail(
        [FromQuery] string token,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return BadRequest("Token boş olamaz.");
        }

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var command = new VerifyEmailCommand(token);
        
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Doğrulama e-postasını yeniden gönderir
    /// </summary>
    [HttpPost("resend")]
    [Authorize]
    [RateLimit(3, 60)] // ✅ BOLUM 3.3: Rate Limiting - 3 istek / dakika (spam koruması)
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> ResendVerificationEmail(
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var command = new ResendVerificationEmailCommand(userId);
        
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// E-posta doğrulama durumunu getirir
    /// </summary>
    [HttpGet("status")]
    [Authorize]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<bool>> GetVerificationStatus(
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var query = new IsEmailVerifiedQuery(userId);
        
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var isVerified = await _mediator.Send(query, cancellationToken);
        return Ok(new { isVerified });
    }
}

