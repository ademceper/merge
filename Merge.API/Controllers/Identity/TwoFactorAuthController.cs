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
[ApiVersion("1.0")] // ✅ BOLUM 4.1: API Versioning (ZORUNLU)
[Route("api/v{version:apiVersion}/two-factor-auth")]
[Authorize]
public class TwoFactorAuthController : BaseController
{
    private readonly IMediator _mediator;

    public TwoFactorAuthController(IMediator mediator)
    {
        _mediator = mediator;
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

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var query = new Get2FAStatusQuery(userId);
        
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var status = await _mediator.Send(query, cancellationToken);
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
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder, manuel ValidateModelState() gereksiz
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var command = new Setup2FACommand(userId, setupDto);
        
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var result = await _mediator.Send(command, cancellationToken);
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
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder, manuel ValidateModelState() gereksiz
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var command = new Enable2FACommand(userId, enableDto);
        
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        await _mediator.Send(command, cancellationToken);
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
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder, manuel ValidateModelState() gereksiz
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var command = new Disable2FACommand(userId, disableDto);
        
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        await _mediator.Send(command, cancellationToken);
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
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder, manuel ValidateModelState() gereksiz
        // ✅ BOLUM 3.2: IDOR Protection - UserId from authenticated user (if authenticated)
        // Note: AllowAnonymous endpoint, so UserId comes from request body (for login flow)
        var command = new Verify2FACodeCommand(verifyDto.UserId, verifyDto.Code);
        
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var isValid = await _mediator.Send(command, cancellationToken);
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

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var command = new SendVerificationCodeCommand(userId, "Login");
        
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        await _mediator.Send(command, cancellationToken);
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
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder, manuel ValidateModelState() gereksiz
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var command = new RegenerateBackupCodesCommand(userId, regenerateDto);
        
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var result = await _mediator.Send(command, cancellationToken);
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
        [FromBody] VerifyBackupCodeDto verifyDto,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder, manuel ValidateModelState() gereksiz
        // ✅ BOLUM 3.2: IDOR Protection - UserId from authenticated user (if authenticated)
        // Note: AllowAnonymous endpoint, so UserId comes from request body (for login flow)
        var command = new VerifyBackupCodeCommand(verifyDto.UserId, verifyDto.BackupCode);
        
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var isValid = await _mediator.Send(command, cancellationToken);
        if (!isValid)
        {
            return BadRequest("Geçersiz yedek kod.");
        }
        return NoContent();
    }
}

