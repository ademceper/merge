using MediatR;
using Merge.Application.DTOs.Identity;

namespace Merge.Application.Identity.Commands.Setup2FA;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record Setup2FACommand(
    Guid UserId,
    TwoFactorSetupDto SetupDto) : IRequest<TwoFactorSetupResponseDto>;

