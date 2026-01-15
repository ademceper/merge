using MediatR;
using Merge.Application.DTOs.Identity;

namespace Merge.Application.Identity.Commands.Disable2FA;

public record Disable2FACommand(
    Guid UserId,
    Disable2FADto DisableDto) : IRequest<Unit>;

