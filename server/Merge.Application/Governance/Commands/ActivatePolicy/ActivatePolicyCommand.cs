using MediatR;

namespace Merge.Application.Governance.Commands.ActivatePolicy;

public record ActivatePolicyCommand(
    Guid Id
) : IRequest<bool>;

