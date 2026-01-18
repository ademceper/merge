using MediatR;
using Merge.Application.Common;
using Merge.Application.DTOs.Governance;
using Merge.Domain.Modules.Content;

namespace Merge.Application.Governance.Queries.GetPolicies;

public record GetPoliciesQuery(
    string? PolicyType = null,
    string? Language = null,
    bool ActiveOnly = false,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<PolicyDto>>;

