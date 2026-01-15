using MediatR;
using Merge.Application.DTOs.Auth;
using Merge.Domain.Modules.Identity;

namespace Merge.Application.Identity.Commands.RefreshToken;

public record RefreshTokenCommand(
    string RefreshToken,
    string? IpAddress) : IRequest<AuthResponseDto>;

