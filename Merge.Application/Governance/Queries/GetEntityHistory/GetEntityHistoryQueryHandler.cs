using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Security;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;

namespace Merge.Application.Governance.Queries.GetEntityHistory;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetEntityHistoryQueryHandler : IRequestHandler<GetEntityHistoryQuery, EntityAuditHistoryDto?>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetEntityHistoryQueryHandler> _logger;

    public GetEntityHistoryQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetEntityHistoryQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<EntityAuditHistoryDto?> Handle(GetEntityHistoryQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving entity history. EntityType: {EntityType}, EntityId: {EntityId}",
            request.EntityType, request.EntityId);

        // ✅ PERFORMANCE: Database'de Count yap (memory'de işlem YASAK)
        var auditCount = await _context.Set<AuditLog>()
            .AsNoTracking()
            .CountAsync(a => a.EntityType == request.EntityType && a.EntityId == request.EntityId, cancellationToken);

        if (auditCount == 0)
        {
            _logger.LogWarning("No audit logs found for entity. EntityType: {EntityType}, EntityId: {EntityId}",
                request.EntityType, request.EntityId);
            return null;
        }

        // ✅ BOLUM 6.3: Unbounded Query Koruması - Güvenlik için limit ekle
        var audits = await _context.Set<AuditLog>()
            .AsNoTracking()
            .Include(a => a.User)
            .Where(a => a.EntityType == request.EntityType &&
                   a.EntityId == request.EntityId)
            .OrderByDescending(a => a.CreatedAt)
            .Take(500) // ✅ Güvenlik: Maksimum 500 audit log
            .ToListAsync(cancellationToken);

        // ✅ PERFORMANCE: Database'de aggregation yap
        var firstChange = await _context.Set<AuditLog>()
            .AsNoTracking()
            .Where(a => a.EntityType == request.EntityType && a.EntityId == request.EntityId)
            .MinAsync(a => (DateTime?)a.CreatedAt, cancellationToken);

        var lastChange = await _context.Set<AuditLog>()
            .AsNoTracking()
            .Where(a => a.EntityType == request.EntityType && a.EntityId == request.EntityId)
            .MaxAsync(a => (DateTime?)a.CreatedAt, cancellationToken);

        var totalChanges = await _context.Set<AuditLog>()
            .AsNoTracking()
            .CountAsync(a => a.EntityType == request.EntityType && a.EntityId == request.EntityId, cancellationToken);

        var modifiedBy = await _context.Set<AuditLog>()
            .AsNoTracking()
            .Where(a => a.EntityType == request.EntityType && 
                   a.EntityId == request.EntityId && 
                   !string.IsNullOrEmpty(a.UserEmail))
            .Select(a => a.UserEmail)
            .Distinct()
            .Take(100) // ✅ Güvenlik: Maksimum 100 unique user
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        // ✅ BOLUM 6.4: List Capacity Pre-allocation (ZORUNLU)
        var auditLogDtos = new List<AuditLogDto>(audits.Count);
        foreach (var audit in audits)
        {
            auditLogDtos.Add(_mapper.Map<AuditLogDto>(audit));
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

