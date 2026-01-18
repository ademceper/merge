using MediatR;
using Merge.Application.DTOs.Security;
using Merge.Domain.SharedKernel;

namespace Merge.Application.Governance.Queries.CompareChanges;

public record CompareChangesQuery(
    Guid AuditLogId
) : IRequest<IEnumerable<AuditComparisonDto>>;
