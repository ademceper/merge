using Merge.Application.DTOs.User;
using Merge.Domain.Modules.Identity;

namespace Merge.Application.DTOs.Auth;

public record AuthResponseDto(
    string Token,
    DateTime ExpiresAt,
    string RefreshToken,
    DateTime RefreshTokenExpiresAt,
    UserDto User);

