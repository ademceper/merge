using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Merge.Application.DTOs.Auth;
using Merge.Application.Identity.Commands.Login;
using Merge.Application.Identity.Commands.Register;
using Merge.Application.Identity.Commands.RefreshToken;
using Merge.Application.Identity.Commands.RevokeToken;
using Merge.API.Middleware;

namespace Merge.API.Controllers.Identity;

[ApiController]
[ApiVersion("1.0")] // ✅ BOLUM 4.1: API Versioning (ZORUNLU)
[Route("api/v{version:apiVersion}/auth")]
public class AuthController : BaseController
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Yeni kullanıcı kaydı oluşturur
    /// </summary>
    // ✅ BOLUM 3.3: Rate Limiting - 3 kayıt denemesi / dakika (spam koruması)
    [HttpPost("register")]
    [RateLimit(3, 60)]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<AuthResponseDto>> Register(
        [FromBody] RegisterDto registerDto,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder, manuel ValidateModelState() gereksiz
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var command = new RegisterCommand(
            registerDto.FirstName,
            registerDto.LastName,
            registerDto.Email,
            registerDto.Password,
            registerDto.PhoneNumber,
            ipAddress);
        
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(Login), new { }, result);
    }

    /// <summary>
    /// Kullanıcı girişi yapar
    /// </summary>
    // ✅ BOLUM 3.3: Rate Limiting - 5 giriş denemesi / dakika (brute force koruması)
    [HttpPost("login")]
    [RateLimit(5, 60)]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<AuthResponseDto>> Login(
        [FromBody] LoginDto loginDto,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder, manuel ValidateModelState() gereksiz
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var command = new LoginCommand(
            loginDto.Email,
            loginDto.Password,
            ipAddress);
        
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Access token'ı yeniler (refresh token kullanarak)
    /// </summary>
    // ✅ BOLUM 3.3: Rate Limiting - 10 istek / dakika
    [HttpPost("refresh")]
    [RateLimit(10, 60)]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<AuthResponseDto>> RefreshToken(
        [FromBody] RefreshTokenRequestDto dto,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder, manuel ValidateModelState() gereksiz
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var command = new RefreshTokenCommand(
            dto.RefreshToken,
            ipAddress);
        
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Refresh token'ı iptal eder (logout)
    /// </summary>
    // ✅ BOLUM 3.3: Rate Limiting - 10 istek / dakika
    [HttpPost("revoke")]
    [RateLimit(10, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> RevokeToken(
        [FromBody] RevokeTokenRequestDto dto,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder, manuel ValidateModelState() gereksiz
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var command = new RevokeTokenCommand(
            dto.RefreshToken,
            ipAddress);
        
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }
}

