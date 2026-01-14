using MediatR;
using Merge.Application.DTOs.Governance;

namespace Merge.Application.Governance.Queries.GetPolicyById;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetPolicyByIdQuery(
    Guid Id
) : IRequest<PolicyDto?>;

