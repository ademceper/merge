using Merge.Application.DTOs.User;
using Merge.Domain.Modules.Identity;

namespace Merge.Application.DTOs.Auth;

// ✅ BOLUM 4.2: Record DTOs (ZORUNLU) - Immutability için record kullan
public record AuthResponseDto(
    string Token,
    DateTime ExpiresAt,
    string RefreshToken,
    DateTime RefreshTokenExpiresAt,
    UserDto User);

