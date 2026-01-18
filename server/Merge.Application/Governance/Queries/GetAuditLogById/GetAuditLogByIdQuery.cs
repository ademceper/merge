using MediatR;
using Merge.Application.DTOs.Security;
using Merge.Domain.SharedKernel;

namespace Merge.Application.Governance.Queries.GetAuditLogById;

public record GetAuditLogByIdQuery(
    Guid Id
) : IRequest<AuditLogDto?>;
