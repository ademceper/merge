using MediatR;
using Merge.Domain.Modules.Identity;

namespace Merge.Application.Identity.Commands.RevokeToken;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record RevokeTokenCommand(
    string RefreshToken,
    string? IpAddress) : IRequest<Unit>;

