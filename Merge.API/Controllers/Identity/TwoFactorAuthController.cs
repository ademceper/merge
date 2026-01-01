using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.Identity;
using Merge.Application.DTOs.Identity;
using Merge.API.Middleware;

namespace Merge.API.Controllers.Identity;

[ApiController]
[Route("api/two-factor-auth")]
[Authorize]
public class TwoFactorAuthController : BaseController
{
    private readonly ITwoFactorAuthService _twoFactorAuthService;

    public TwoFactorAuthController(ITwoFactorAuthService twoFactorAuthService)
    {
        _twoFactorAuthService = twoFactorAuthService;
    }

    [HttpGet("status")]
    [ProducesResponseType(typeof(TwoFactorStatusDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TwoFactorStatusDto>> GetStatus()
    {
        var userId = GetUserId();
        var status = await _twoFactorAuthService.Get2FAStatusAsync(userId);
        return Ok(status);
    }

    [HttpPost("setup")]
    [ProducesResponseType(typeof(TwoFactorSetupResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TwoFactorSetupResponseDto>> Setup([FromBody] TwoFactorSetupDto setupDto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var userId = GetUserId();
        var result = await _twoFactorAuthService.Setup2FAAsync(userId, setupDto);
        return Ok(result);
    }

    [HttpPost("enable")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Enable([FromBody] Enable2FADto enableDto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var userId = GetUserId();
        await _twoFactorAuthService.Enable2FAAsync(userId, enableDto);
        return NoContent();
    }

    [HttpPost("disable")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Disable([FromBody] Disable2FADto disableDto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var userId = GetUserId();
        await _twoFactorAuthService.Disable2FAAsync(userId, disableDto);
        return NoContent();
    }

    // ✅ SECURITY: Rate limiting - 5 doğrulama denemesi / dakika (brute force koruması)
    [HttpPost("verify")]
    [AllowAnonymous]
    [RateLimit(5, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Verify([FromBody] Verify2FADto verifyDto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var isValid = await _twoFactorAuthService.Verify2FACodeAsync(verifyDto.UserId, verifyDto.Code);
        if (!isValid)
        {
            return BadRequest("Geçersiz kod.");
        }
        return NoContent();
    }

    // ✅ SECURITY: Rate limiting - 3 kod gönderimi / dakika (spam koruması)
    [HttpPost("send-code")]
    [RateLimit(3, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SendCode()
    {
        var userId = GetUserId();
        await _twoFactorAuthService.SendVerificationCodeAsync(userId);
        return NoContent();
    }

    [HttpPost("regenerate-backup-codes")]
    [ProducesResponseType(typeof(BackupCodesResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<BackupCodesResponseDto>> RegenerateBackupCodes([FromBody] RegenerateBackupCodesDto regenerateDto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var userId = GetUserId();
        var result = await _twoFactorAuthService.RegenerateBackupCodesAsync(userId, regenerateDto);
        return Ok(result);
    }

    [HttpPost("verify-backup-code")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyBackupCode([FromBody] Verify2FADto verifyDto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var isValid = await _twoFactorAuthService.VerifyBackupCodeAsync(verifyDto.UserId, verifyDto.Code);
        if (!isValid)
        {
            return BadRequest("Geçersiz yedek kod.");
        }
        return NoContent();
    }
}

