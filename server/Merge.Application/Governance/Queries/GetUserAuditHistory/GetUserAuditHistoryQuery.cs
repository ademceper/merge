using MediatR;
using Merge.Application.DTOs.Security;
using Merge.Domain.SharedKernel;

namespace Merge.Application.Governance.Queries.GetUserAuditHistory;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetUserAuditHistoryQuery(
    Guid UserId,
    int Days = 30
) : IRequest<IEnumerable<AuditLogDto>>;
