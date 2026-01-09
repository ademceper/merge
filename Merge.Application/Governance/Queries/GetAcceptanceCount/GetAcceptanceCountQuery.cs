using MediatR;

namespace Merge.Application.Governance.Queries.GetAcceptanceCount;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetAcceptanceCountQuery(
    Guid PolicyId
) : IRequest<int>;

