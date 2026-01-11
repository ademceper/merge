using MediatR;

namespace Merge.Application.Security.Commands.VerifyOrder;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record VerifyOrderCommand(
    Guid VerificationId,
    Guid VerifiedByUserId,
    string? Notes = null
) : IRequest<bool>;
