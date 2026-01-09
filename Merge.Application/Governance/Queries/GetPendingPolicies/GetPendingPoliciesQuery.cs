using MediatR;
using Merge.Application.DTOs.Governance;

namespace Merge.Application.Governance.Queries.GetPendingPolicies;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetPendingPoliciesQuery(
    Guid UserId
) : IRequest<IEnumerable<PolicyDto>>;

