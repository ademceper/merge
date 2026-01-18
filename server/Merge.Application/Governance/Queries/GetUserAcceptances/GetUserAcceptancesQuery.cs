using MediatR;
using Merge.Application.DTOs.Governance;

namespace Merge.Application.Governance.Queries.GetUserAcceptances;

public record GetUserAcceptancesQuery(
    Guid UserId
) : IRequest<IEnumerable<PolicyAcceptanceDto>>;

