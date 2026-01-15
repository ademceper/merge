using MediatR;
using Merge.Application.DTOs.Identity;

namespace Merge.Application.Identity.Commands.Enable2FA;

public record Enable2FACommand(
    Guid UserId,
    Enable2FADto EnableDto) : IRequest<Unit>;

