using MediatR;
using Merge.Application.DTOs.Governance;

namespace Merge.Application.Governance.Queries.GetUserAcceptances;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetUserAcceptancesQuery(
    Guid UserId
) : IRequest<IEnumerable<PolicyAcceptanceDto>>;

