using MediatR;
using Merge.Application.DTOs.Auth;
using Merge.Domain.Modules.Identity;

namespace Merge.Application.Identity.Commands.RefreshToken;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record RefreshTokenCommand(
    string RefreshToken,
    string? IpAddress) : IRequest<AuthResponseDto>;

