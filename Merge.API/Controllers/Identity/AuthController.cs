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

    // ✅ SECURITY: Rate limiting - 3 kayıt denemesi / dakika
    [HttpPost("register")]
    [RateLimit(3, 60)]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto registerDto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var result = await _authService.RegisterAsync(registerDto);
        if (result == null)
        {
            return BadRequest("Kayıt işlemi başarısız oldu.");
        }
        return Ok(result);
    }

    // ✅ SECURITY: Rate limiting - 5 giriş denemesi / dakika (brute force koruması)
    [HttpPost("login")]
    [RateLimit(5, 60)]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto loginDto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var result = await _authService.LoginAsync(loginDto);
        if (result == null)
        {
            return Unauthorized("Kullanıcı adı veya şifre hatalı.");
        }
        return Ok(result);
    }

    // ✅ SECURITY: Refresh token endpoint - Access token yenilemek için
    [HttpPost("refresh")]
    [RateLimit(10, 60)]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponseDto>> RefreshToken([FromBody] RefreshTokenRequestDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _authService.RefreshTokenAsync(dto.RefreshToken, ipAddress);
        return Ok(result);
    }

    // ✅ SECURITY: Logout / Token revoke endpoint
    [HttpPost("revoke")]
    [RateLimit(10, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenRequestDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        await _authService.RevokeTokenAsync(dto.RefreshToken, ipAddress);
        return NoContent();
    }
}

