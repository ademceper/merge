using MediatR;

namespace Merge.Application.Governance.Commands.DeactivatePolicy;

public record DeactivatePolicyCommand(
    Guid Id
) : IRequest<bool>;

