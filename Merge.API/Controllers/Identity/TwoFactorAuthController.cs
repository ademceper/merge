using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.Identity;
using Merge.Application.DTOs.Identity;

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
    public async Task<ActionResult<TwoFactorStatusDto>> GetStatus()
    {
        var userId = GetUserId();
        var status = await _twoFactorAuthService.Get2FAStatusAsync(userId);
        return Ok(status);
    }

    [HttpPost("setup")]
    public async Task<ActionResult<TwoFactorSetupResponseDto>> Setup([FromBody] TwoFactorSetupDto setupDto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var userId = GetUserId();
        var result = await _twoFactorAuthService.Setup2FAAsync(userId, setupDto);
        return Ok(result);
    }

    [HttpPost("enable")]
    public async Task<IActionResult> Enable([FromBody] Enable2FADto enableDto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var userId = GetUserId();
        await _twoFactorAuthService.Enable2FAAsync(userId, enableDto);
        return NoContent();
    }

    [HttpPost("disable")]
    public async Task<IActionResult> Disable([FromBody] Disable2FADto disableDto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var userId = GetUserId();
        await _twoFactorAuthService.Disable2FAAsync(userId, disableDto);
        return NoContent();
    }

    [HttpPost("verify")]
    [AllowAnonymous]
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

    [HttpPost("send-code")]
    public async Task<IActionResult> SendCode()
    {
        var userId = GetUserId();
        await _twoFactorAuthService.SendVerificationCodeAsync(userId);
        return NoContent();
    }

    [HttpPost("regenerate-backup-codes")]
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

