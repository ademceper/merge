using MediatR;

namespace Merge.Application.Identity.Commands.Verify2FACode;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record Verify2FACodeCommand(
    Guid UserId,
    string Code) : IRequest<bool>;

