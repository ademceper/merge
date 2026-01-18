using MediatR;

namespace Merge.Application.Governance.Queries.GetAcceptanceCount;

public record GetAcceptanceCountQuery(
    Guid PolicyId
) : IRequest<int>;

