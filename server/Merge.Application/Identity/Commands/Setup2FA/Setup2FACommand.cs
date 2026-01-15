using MediatR;
using Merge.Application.DTOs.Identity;

namespace Merge.Application.Identity.Commands.Setup2FA;

public record Setup2FACommand(
    Guid UserId,
    TwoFactorSetupDto SetupDto) : IRequest<TwoFactorSetupResponseDto>;

