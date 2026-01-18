using System.ComponentModel.DataAnnotations;
using Merge.Domain.Modules.Identity;

namespace Merge.Application.DTOs.Auth;

public record RefreshTokenRequestDto(
    [Required(ErrorMessage = "Refresh token zorunludur.")] string RefreshToken);

public record RevokeTokenRequestDto(
    [Required(ErrorMessage = "Refresh token zorunludur.")] string RefreshToken);
