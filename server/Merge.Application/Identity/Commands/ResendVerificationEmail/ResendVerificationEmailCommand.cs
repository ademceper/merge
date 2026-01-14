using MediatR;

namespace Merge.Application.Identity.Commands.ResendVerificationEmail;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record ResendVerificationEmailCommand(
    Guid UserId) : IRequest<Unit>;

