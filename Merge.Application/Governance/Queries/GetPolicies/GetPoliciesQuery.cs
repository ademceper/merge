using MediatR;
using Merge.Application.Common;
using Merge.Application.DTOs.Governance;

namespace Merge.Application.Governance.Queries.GetPolicies;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetPoliciesQuery(
    string? PolicyType = null,
    string? Language = null,
    bool ActiveOnly = false,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<PolicyDto>>;

