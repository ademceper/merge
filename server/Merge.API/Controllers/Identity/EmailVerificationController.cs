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
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/email-verification")]
public class EmailVerificationController(IMediator mediator) : BaseController
{

    /// <summary>
    /// E-posta doğrulama token'ını doğrular
    /// </summary>
    [HttpPost("verify")]
    [AllowAnonymous]
    [RateLimit(10, 60)]
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

        var command = new VerifyEmailCommand(token);
        
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Doğrulama e-postasını yeniden gönderir
    /// </summary>
    [HttpPost("resend")]
    [Authorize]
    [RateLimit(3, 60)]
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

        var command = new ResendVerificationEmailCommand(userId);
        
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// E-posta doğrulama durumunu getirir
    /// </summary>
    [HttpGet("status")]
    [Authorize]
    [RateLimit(60, 60)]
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

        var query = new IsEmailVerifiedQuery(userId);
        
        var isVerified = await mediator.Send(query, cancellationToken);
        return Ok(new { isVerified });
    }
}

