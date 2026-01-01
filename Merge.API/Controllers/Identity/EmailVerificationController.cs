using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.Identity;

namespace Merge.API.Controllers.Identity;

[ApiController]
[Route("api/email-verification")]
public class EmailVerificationController : BaseController
{
    private readonly IEmailVerificationService _emailVerificationService;
    public EmailVerificationController(
    IEmailVerificationService emailVerificationService)
    {
        _emailVerificationService = emailVerificationService;
    }

    [HttpPost("verify")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyEmail([FromQuery] string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return BadRequest("Token boş olamaz.");
        }

        var result = await _emailVerificationService.VerifyEmailAsync(token);
        if (!result)
        {
            return BadRequest("Geçersiz token.");
        }
        return NoContent();
    }

    [HttpPost("resend")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ResendVerificationEmail()
    {
        var userId = GetUserId();
        var result = await _emailVerificationService.ResendVerificationEmailAsync(userId);
        if (!result)
        {
            return BadRequest("E-posta gönderilemedi.");
        }
        return NoContent();
    }

    [HttpGet("status")]
    [Authorize]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<bool>> GetVerificationStatus()
    {
        var userId = GetUserId();
        var isVerified = await _emailVerificationService.IsEmailVerifiedAsync(userId);
        return Ok(new { isVerified });
    }
}

