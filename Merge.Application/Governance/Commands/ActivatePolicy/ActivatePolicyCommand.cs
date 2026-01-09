using MediatR;

namespace Merge.Application.Governance.Commands.ActivatePolicy;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record ActivatePolicyCommand(
    Guid Id
) : IRequest<bool>;

