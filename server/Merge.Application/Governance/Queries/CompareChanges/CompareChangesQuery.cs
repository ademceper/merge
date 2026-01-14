using MediatR;
using Merge.Application.DTOs.Security;
using Merge.Domain.SharedKernel;

namespace Merge.Application.Governance.Queries.CompareChanges;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record CompareChangesQuery(
    Guid AuditLogId
) : IRequest<IEnumerable<AuditComparisonDto>>;
