using MediatR;

namespace Merge.Application.Identity.Commands.VerifyEmail;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record VerifyEmailCommand(
    string Token) : IRequest<Unit>;

