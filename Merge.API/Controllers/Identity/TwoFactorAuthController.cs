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

    /// <summary>
    /// İki faktörlü kimlik doğrulama durumunu getirir
    /// </summary>
    [HttpGet("status")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(TwoFactorStatusDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<TwoFactorStatusDto>> GetStatus(
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var status = await _twoFactorAuthService.Get2FAStatusAsync(userId, cancellationToken);
        return Ok(status);
    }

    /// <summary>
    /// İki faktörlü kimlik doğrulamayı kurar
    /// </summary>
    [HttpPost("setup")]
    [RateLimit(5, 60)] // ✅ BOLUM 3.3: Rate Limiting - 5 istek / dakika
    [ProducesResponseType(typeof(TwoFactorSetupResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<TwoFactorSetupResponseDto>> Setup(
        [FromBody] TwoFactorSetupDto setupDto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var result = await _twoFactorAuthService.Setup2FAAsync(userId, setupDto, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// İki faktörlü kimlik doğrulamayı etkinleştirir
    /// </summary>
    [HttpPost("enable")]
    [RateLimit(5, 60)] // ✅ BOLUM 3.3: Rate Limiting - 5 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Enable(
        [FromBody] Enable2FADto enableDto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        await _twoFactorAuthService.Enable2FAAsync(userId, enableDto, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// İki faktörlü kimlik doğrulamayı devre dışı bırakır
    /// </summary>
    [HttpPost("disable")]
    [RateLimit(5, 60)] // ✅ BOLUM 3.3: Rate Limiting - 5 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Disable(
        [FromBody] Disable2FADto disableDto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        await _twoFactorAuthService.Disable2FAAsync(userId, disableDto, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// İki faktörlü kimlik doğrulama kodunu doğrular
    /// </summary>
    // ✅ BOLUM 3.3: Rate Limiting - 5 doğrulama denemesi / dakika (brute force koruması)
    [HttpPost("verify")]
    [AllowAnonymous]
    [RateLimit(5, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Verify(
        [FromBody] Verify2FADto verifyDto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var isValid = await _twoFactorAuthService.Verify2FACodeAsync(verifyDto.UserId, verifyDto.Code, cancellationToken);
        if (!isValid)
        {
            return BadRequest("Geçersiz kod.");
        }
        return NoContent();
    }

    /// <summary>
    /// Doğrulama kodunu gönderir
    /// </summary>
    // ✅ BOLUM 3.3: Rate Limiting - 3 kod gönderimi / dakika (spam koruması)
    [HttpPost("send-code")]
    [RateLimit(3, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> SendCode(
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        await _twoFactorAuthService.SendVerificationCodeAsync(userId, "Login", cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Yedek kodları yeniden oluşturur
    /// </summary>
    [HttpPost("regenerate-backup-codes")]
    [RateLimit(5, 60)] // ✅ BOLUM 3.3: Rate Limiting - 5 istek / dakika
    [ProducesResponseType(typeof(BackupCodesResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<BackupCodesResponseDto>> RegenerateBackupCodes(
        [FromBody] RegenerateBackupCodesDto regenerateDto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var result = await _twoFactorAuthService.RegenerateBackupCodesAsync(userId, regenerateDto, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Yedek kodu doğrular
    /// </summary>
    [HttpPost("verify-backup-code")]
    [AllowAnonymous]
    [RateLimit(5, 60)] // ✅ BOLUM 3.3: Rate Limiting - 5 doğrulama denemesi / dakika (brute force koruması)
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> VerifyBackupCode(
        [FromBody] Verify2FADto verifyDto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var isValid = await _twoFactorAuthService.VerifyBackupCodeAsync(verifyDto.UserId, verifyDto.Code, cancellationToken);
        if (!isValid)
        {
            return BadRequest("Geçersiz yedek kod.");
        }
        return NoContent();
    }
}

