using MediatR;
using Merge.Application.DTOs.Security;
using Merge.Domain.SharedKernel;

namespace Merge.Application.Governance.Queries.GetAuditLogById;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetAuditLogByIdQuery(
    Guid Id
) : IRequest<AuditLogDto?>;
