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

/// <summary>
/// Authentication API endpoints.
/// Kullanıcı kimlik doğrulama işlemlerini yönetir.
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/auth")]
[Tags("Auth")]
public class AuthController(IMediator mediator) : BaseController
{

    /// <summary>
    /// Yeni kullanıcı kaydı oluşturur
    /// </summary>
    [HttpPost("register")]
    [RateLimit(3, 60)]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<AuthResponseDto>> Register(
        [FromBody] RegisterDto registerDto,
        CancellationToken cancellationToken = default)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var command = new RegisterCommand(
            registerDto.FirstName,
            registerDto.LastName,
            registerDto.Email,
            registerDto.Password,
            registerDto.PhoneNumber,
            ipAddress);
        
        var result = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(Login), new { }, result);
    }

    /// <summary>
    /// Kullanıcı girişi yapar
    /// </summary>
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
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var command = new LoginCommand(
            loginDto.Email,
            loginDto.Password,
            ipAddress);
        
        var result = await mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Access token'ı yeniler (refresh token kullanarak)
    /// </summary>
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
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var command = new RefreshTokenCommand(
            dto.RefreshToken,
            ipAddress);
        
        var result = await mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Refresh token'ı iptal eder (logout)
    /// </summary>
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
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var command = new RevokeTokenCommand(
            dto.RefreshToken,
            ipAddress);
        
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }
}

