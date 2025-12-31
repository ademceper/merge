using Merge.Application.DTOs.Auth;

namespace Merge.Application.Interfaces.Identity;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto);
    Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
    Task<bool> ValidateTokenAsync(string token);
}

