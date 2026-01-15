using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Security;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;
using Merge.Domain.SharedKernel;

namespace Merge.Application.Governance.Queries.GetAuditLogById;

public class GetAuditLogByIdQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetAuditLogByIdQueryHandler> logger) : IRequestHandler<GetAuditLogByIdQuery, AuditLogDto?>
{
    public async Task<AuditLogDto?> Handle(GetAuditLogByIdQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving audit log with Id: {AuditLogId}", request.Id);

        var audit = await context.Set<AuditLog>()
            .AsNoTracking()
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (audit == null)
        {
            logger.LogWarning("Audit log not found. AuditLogId: {AuditLogId}", request.Id);
            return null;
        }

        return mapper.Map<AuditLogDto>(audit);
    }
}
