using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Merge.Application.DTOs.Identity;
using VerifyBackupCodeDto = Merge.Application.DTOs.Identity.VerifyBackupCodeDto;
using Merge.Application.Identity.Queries.Get2FAStatus;
using Merge.Application.Identity.Commands.Setup2FA;
using Merge.Application.Identity.Commands.Enable2FA;
using Merge.Application.Identity.Commands.Disable2FA;
using Merge.Application.Identity.Commands.Verify2FACode;
using Merge.Application.Identity.Commands.SendVerificationCode;
using Merge.Application.Identity.Commands.RegenerateBackupCodes;
using Merge.Application.Identity.Commands.VerifyBackupCode;
using Merge.API.Middleware;

namespace Merge.API.Controllers.Identity;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/two-factor-auth")]
[Authorize]
public class TwoFactorAuthController(IMediator mediator) : BaseController
{

    /// <summary>
    /// İki faktörlü kimlik doğrulama durumunu getirir
    /// </summary>
    [HttpGet("status")]
    [RateLimit(60, 60)]
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

        var query = new Get2FAStatusQuery(userId);
        
        var status = await mediator.Send(query, cancellationToken);
        return Ok(status);
    }

    /// <summary>
    /// İki faktörlü kimlik doğrulamayı kurar
    /// </summary>
    [HttpPost("setup")]
    [RateLimit(5, 60)]
    [ProducesResponseType(typeof(TwoFactorSetupResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<TwoFactorSetupResponseDto>> Setup(
        [FromBody] TwoFactorSetupDto setupDto,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var command = new Setup2FACommand(userId, setupDto);
        
        var result = await mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// İki faktörlü kimlik doğrulamayı etkinleştirir
    /// </summary>
    [HttpPost("enable")]
    [RateLimit(5, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Enable(
        [FromBody] Enable2FADto enableDto,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var command = new Enable2FACommand(userId, enableDto);
        
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// İki faktörlü kimlik doğrulamayı devre dışı bırakır
    /// </summary>
    [HttpPost("disable")]
    [RateLimit(5, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Disable(
        [FromBody] Disable2FADto disableDto,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var command = new Disable2FACommand(userId, disableDto);
        
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// İki faktörlü kimlik doğrulama kodunu doğrular
    /// </summary>
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
        // Note: AllowAnonymous endpoint, so UserId comes from request body (for login flow)
        var command = new Verify2FACodeCommand(verifyDto.UserId, verifyDto.Code);
        
        var isValid = await mediator.Send(command, cancellationToken);
        if (!isValid)
        {
            return BadRequest("Geçersiz kod.");
        }
        return NoContent();
    }

    /// <summary>
    /// Doğrulama kodunu gönderir
    /// </summary>
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

        var command = new SendVerificationCodeCommand(userId, "Login");
        
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Yedek kodları yeniden oluşturur
    /// </summary>
    [HttpPost("regenerate-backup-codes")]
    [RateLimit(5, 60)]
    [ProducesResponseType(typeof(BackupCodesResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<BackupCodesResponseDto>> RegenerateBackupCodes(
        [FromBody] RegenerateBackupCodesDto regenerateDto,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var command = new RegenerateBackupCodesCommand(userId, regenerateDto);
        
        var result = await mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Yedek kodu doğrular
    /// </summary>
    [HttpPost("verify-backup-code")]
    [AllowAnonymous]
    [RateLimit(5, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> VerifyBackupCode(
        [FromBody] VerifyBackupCodeDto verifyDto,
        CancellationToken cancellationToken = default)
    {
        // Note: AllowAnonymous endpoint, so UserId comes from request body (for login flow)
        var command = new VerifyBackupCodeCommand(verifyDto.UserId, verifyDto.BackupCode);
        
        var isValid = await mediator.Send(command, cancellationToken);
        if (!isValid)
        {
            return BadRequest("Geçersiz yedek kod.");
        }
        return NoContent();
    }
}

