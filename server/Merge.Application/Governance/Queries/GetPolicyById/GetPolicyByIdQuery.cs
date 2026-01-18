using MediatR;
using Merge.Application.DTOs.Governance;

namespace Merge.Application.Governance.Queries.GetPolicyById;

public record GetPolicyByIdQuery(
    Guid Id
) : IRequest<PolicyDto?>;

