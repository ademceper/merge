using MediatR;

namespace Merge.Application.Security.Commands.RejectOrder;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record RejectOrderCommand(
    Guid VerificationId,
    Guid VerifiedByUserId,
    string Reason
) : IRequest<bool>;
