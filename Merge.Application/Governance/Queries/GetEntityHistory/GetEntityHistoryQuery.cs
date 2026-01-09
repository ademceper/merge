using MediatR;
using Merge.Application.DTOs.Security;

namespace Merge.Application.Governance.Queries.GetEntityHistory;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetEntityHistoryQuery(
    string EntityType,
    Guid EntityId
) : IRequest<EntityAuditHistoryDto?>;

