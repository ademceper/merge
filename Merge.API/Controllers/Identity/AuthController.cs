using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.DTOs.Auth;
using Merge.Application.Interfaces.Identity;
using Merge.API.Middleware;

namespace Merge.API.Controllers.Identity;

[ApiController]
[Route("api/auth")]
public class AuthController : BaseController
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
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
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var result = await _authService.RegisterAsync(registerDto, cancellationToken);
        if (result == null)
        {
            return BadRequest("Kayıt işlemi başarısız oldu.");
        }
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
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var result = await _authService.LoginAsync(loginDto, cancellationToken);
        if (result == null)
        {
            return Unauthorized("Kullanıcı adı veya şifre hatalı.");
        }
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
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var result = await _authService.RefreshTokenAsync(dto.RefreshToken, ipAddress, cancellationToken);
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
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        await _authService.RevokeTokenAsync(dto.RefreshToken, ipAddress, cancellationToken);
        return NoContent();
    }
}

