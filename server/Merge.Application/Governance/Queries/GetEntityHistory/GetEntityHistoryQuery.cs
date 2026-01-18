using MediatR;
using Merge.Application.DTOs.Security;

namespace Merge.Application.Governance.Queries.GetEntityHistory;

public record GetEntityHistoryQuery(
    string EntityType,
    Guid EntityId
) : IRequest<EntityAuditHistoryDto?>;

