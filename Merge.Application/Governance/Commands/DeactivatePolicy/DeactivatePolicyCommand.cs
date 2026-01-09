using MediatR;

namespace Merge.Application.Governance.Commands.DeactivatePolicy;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record DeactivatePolicyCommand(
    Guid Id
) : IRequest<bool>;

