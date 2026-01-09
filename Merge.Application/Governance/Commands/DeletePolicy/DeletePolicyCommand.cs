using MediatR;

namespace Merge.Application.Governance.Commands.DeletePolicy;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record DeletePolicyCommand(
    Guid Id,
    Guid? PerformedBy = null // IDOR protection için
) : IRequest<bool>;

