using Microsoft.AspNetCore.Authorization;
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
    public async Task<ActionResult<bool>> GetVerificationStatus()
    {
        var userId = GetUserId();
        var isVerified = await _emailVerificationService.IsEmailVerifiedAsync(userId);
        return Ok(new { isVerified });
    }
}

