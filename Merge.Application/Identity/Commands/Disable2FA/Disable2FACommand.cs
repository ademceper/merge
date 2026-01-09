using MediatR;
using Merge.Application.DTOs.Identity;

namespace Merge.Application.Identity.Commands.Disable2FA;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record Disable2FACommand(
    Guid UserId,
    Disable2FADto DisableDto) : IRequest<Unit>;

