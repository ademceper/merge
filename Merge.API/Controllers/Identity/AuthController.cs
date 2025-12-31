using Microsoft.AspNetCore.Mvc;
using Merge.Application.DTOs.Auth;
using Merge.Application.Interfaces.Identity;

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

    [HttpPost("register")]
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

    [HttpPost("login")]
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
}

