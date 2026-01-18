using MediatR;
using Merge.Application.DTOs.Governance;
using Merge.Domain.Modules.Content;

namespace Merge.Application.Governance.Queries.GetActivePolicy;

public record GetActivePolicyQuery(
    string PolicyType,
    string Language = "tr"
) : IRequest<PolicyDto?>;

