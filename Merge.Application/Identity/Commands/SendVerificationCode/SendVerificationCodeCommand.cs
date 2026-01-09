using MediatR;

namespace Merge.Application.Identity.Commands.SendVerificationCode;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record SendVerificationCodeCommand(
    Guid UserId,
    string Purpose = "Login") : IRequest<Unit>;

