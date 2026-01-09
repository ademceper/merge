using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Auth;

// ✅ BOLUM 4.2: Record DTOs (ZORUNLU) - Immutability için record kullan
public record RefreshTokenRequestDto(
    [Required(ErrorMessage = "Refresh token zorunludur.")] string RefreshToken);

// ✅ BOLUM 4.2: Record DTOs (ZORUNLU) - Immutability için record kullan
public record RevokeTokenRequestDto(
    [Required(ErrorMessage = "Refresh token zorunludur.")] string RefreshToken);
