using MediatR;

namespace Merge.Application.Governance.Commands.DeletePolicy;

public record DeletePolicyCommand(
    Guid Id,
    Guid? PerformedBy = null // IDOR protection için
) : IRequest<bool>;

