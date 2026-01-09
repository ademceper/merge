using MediatR;
using Merge.Application.DTOs.Identity;

namespace Merge.Application.Identity.Commands.Enable2FA;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record Enable2FACommand(
    Guid UserId,
    Enable2FADto EnableDto) : IRequest<Unit>;

