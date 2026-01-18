using MediatR;
using Merge.Application.DTOs.Governance;

namespace Merge.Application.Governance.Queries.GetPendingPolicies;

public record GetPendingPoliciesQuery(
    Guid UserId
) : IRequest<IEnumerable<PolicyDto>>;

