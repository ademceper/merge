using Merge.Application.DTOs.Auth;

namespace Merge.Application.Interfaces.Identity;

// ⚠️ OBSOLETE: Bu interface artık kullanılmamalı. MediatR Command/Query pattern kullanılmalı.
// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU) - Service'ler yerine Command/Query handler'ları kullan
[Obsolete("Use MediatR Commands/Queries instead. This interface will be removed in a future version.")]
public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto, CancellationToken cancellationToken = default);
    Task<AuthResponseDto> LoginAsync(LoginDto loginDto, CancellationToken cancellationToken = default);
    Task<bool> ValidateTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<AuthResponseDto> RefreshTokenAsync(string refreshToken, string? ipAddress = null, CancellationToken cancellationToken = default);
    Task RevokeTokenAsync(string refreshToken, string? ipAddress = null, CancellationToken cancellationToken = default);
}

