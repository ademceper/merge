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

namespace Merge.Application.Governance.Queries.GetEntityHistory;

public class GetEntityHistoryQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetEntityHistoryQueryHandler> logger) : IRequestHandler<GetEntityHistoryQuery, EntityAuditHistoryDto?>
{

    public async Task<EntityAuditHistoryDto?> Handle(GetEntityHistoryQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving entity history. EntityType: {EntityType}, EntityId: {EntityId}",
            request.EntityType, request.EntityId);

        var auditCount = await context.Set<AuditLog>()
            .AsNoTracking()
            .CountAsync(a => a.EntityType == request.EntityType && a.EntityId == request.EntityId, cancellationToken);

        if (auditCount == 0)
        {
            logger.LogWarning("No audit logs found for entity. EntityType: {EntityType}, EntityId: {EntityId}",
                request.EntityType, request.EntityId);
            return null;
        }

        var audits = await context.Set<AuditLog>()
            .AsNoTracking()
            .Include(a => a.User)
            .Where(a => a.EntityType == request.EntityType &&
                   a.EntityId == request.EntityId)
            .OrderByDescending(a => a.CreatedAt)
            .Take(500)
            .ToListAsync(cancellationToken);

        var firstChange = await context.Set<AuditLog>()
            .AsNoTracking()
            .Where(a => a.EntityType == request.EntityType && a.EntityId == request.EntityId)
            .MinAsync(a => (DateTime?)a.CreatedAt, cancellationToken);

        var lastChange = await context.Set<AuditLog>()
            .AsNoTracking()
            .Where(a => a.EntityType == request.EntityType && a.EntityId == request.EntityId)
            .MaxAsync(a => (DateTime?)a.CreatedAt, cancellationToken);

        var totalChanges = await context.Set<AuditLog>()
            .AsNoTracking()
            .CountAsync(a => a.EntityType == request.EntityType && a.EntityId == request.EntityId, cancellationToken);

        var modifiedBy = await context.Set<AuditLog>()
            .AsNoTracking()
            .Where(a => a.EntityType == request.EntityType && 
                   a.EntityId == request.EntityId && 
                   !string.IsNullOrEmpty(a.UserEmail))
            .Select(a => a.UserEmail)
            .Distinct()
            .Take(100)
            .ToListAsync(cancellationToken);

        var auditLogDtos = new List<AuditLogDto>(audits.Count);
        foreach (var audit in audits)
        {
            auditLogDtos.Add(mapper.Map<AuditLogDto>(audit));
        }

        return new EntityAuditHistoryDto
        {
            EntityId = request.EntityId,
            EntityType = request.EntityType,
            AuditLogs = auditLogDtos,
            FirstChange = firstChange ?? DateTime.UtcNow,
            LastChange = lastChange ?? DateTime.UtcNow,
            TotalChanges = totalChanges,
            ModifiedBy = modifiedBy
        };
    }
}
