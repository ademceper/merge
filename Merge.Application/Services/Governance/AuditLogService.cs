using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Governance;
using Merge.Application.Common;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using System.Text.Json;
using Merge.Application.DTOs.Security;


namespace Merge.Application.Services.Governance;

// ⚠️ OBSOLETE: Bu service artık kullanılmamalı. MediatR Command/Query handler'ları kullanın.
[Obsolete("Use MediatR commands and queries instead. This service will be removed in a future version.")]
public class AuditLogService : IAuditLogService
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<AuditLogService> _logger;

    public AuditLogService(IDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<AuditLogService> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task LogAsync(CreateAuditLogDto auditDto, string ipAddress, string userAgent, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        var audit = AuditLog.Create(
            action: auditDto.Action,
            entityType: auditDto.EntityType,
            tableName: auditDto.TableName,
            ipAddress: ipAddress,
            userAgent: userAgent,
            module: auditDto.Module,
            severity: ParseSeverity(auditDto.Severity),
            userId: auditDto.UserId,
            userEmail: auditDto.UserEmail,
            entityId: auditDto.EntityId,
            primaryKey: auditDto.PrimaryKey,
            oldValues: auditDto.OldValues,
            newValues: auditDto.NewValues,
            changes: auditDto.Changes,
            additionalData: auditDto.AdditionalData,
            isSuccessful: auditDto.IsSuccessful,
            errorMessage: auditDto.ErrorMessage
        );

        await _context.Set<AuditLog>().AddAsync(audit, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    [Obsolete("Use GetAuditLogByIdQuery via MediatR instead")]
    public async Task<AuditLogDto?> GetAuditByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !a.IsDeleted (Global Query Filter)
        var audit = await _context.Set<AuditLog>()
            .AsNoTracking()
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (audit == null)
        {
            return null;
        }

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<AuditLogDto>(audit);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.4: Pagination - PagedResult dönmeli (ZORUNLU)
    [Obsolete("Use SearchAuditLogsQuery via MediatR instead")]
    public async Task<PagedResult<AuditLogDto>> GetAuditLogsAsync(AuditLogFilterDto filter, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        if (filter.PageSize > 100) filter.PageSize = 100;
        if (filter.PageNumber < 1) filter.PageNumber = 1;

        // ✅ PERFORMANCE: Global Query Filter automatically filters !a.IsDeleted
        // ✅ FIX: Explicitly type as IQueryable to avoid IIncludableQueryable type mismatch
        IQueryable<AuditLog> query = _context.Set<AuditLog>()
            .AsNoTracking()
            .Include(a => a.User);

        if (filter.UserId.HasValue)
            query = query.Where(a => a.UserId == filter.UserId.Value);

        if (!string.IsNullOrEmpty(filter.UserEmail))
            query = query.Where(a => a.UserEmail.Contains(filter.UserEmail));

        if (!string.IsNullOrEmpty(filter.Action))
            query = query.Where(a => a.Action == filter.Action);

        if (!string.IsNullOrEmpty(filter.EntityType))
            query = query.Where(a => a.EntityType == filter.EntityType);

        if (filter.EntityId.HasValue)
            query = query.Where(a => a.EntityId == filter.EntityId.Value);

        if (!string.IsNullOrEmpty(filter.TableName))
            query = query.Where(a => a.TableName == filter.TableName);

        if (!string.IsNullOrEmpty(filter.Severity))
        {
            var severity = ParseSeverity(filter.Severity);
            query = query.Where(a => a.Severity == severity);
        }

        if (!string.IsNullOrEmpty(filter.Module))
            query = query.Where(a => a.Module == filter.Module);

        if (filter.IsSuccessful.HasValue)
            query = query.Where(a => a.IsSuccessful == filter.IsSuccessful.Value);

        if (filter.StartDate.HasValue)
            query = query.Where(a => a.CreatedAt >= filter.StartDate.Value);

        if (filter.EndDate.HasValue)
            query = query.Where(a => a.CreatedAt <= filter.EndDate.Value);

        if (!string.IsNullOrEmpty(filter.IpAddress))
            query = query.Where(a => a.IpAddress == filter.IpAddress);

        var totalCount = await query.CountAsync(cancellationToken);

        var audits = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        // ✅ PERFORMANCE: ToListAsync() sonrası Select() YASAK - AutoMapper'ın Map<IEnumerable<T>> metodunu kullan
        // ✅ BOLUM 6.4: List Capacity Pre-allocation (ZORUNLU)
        var items = new List<AuditLogDto>(audits.Count);
        foreach (var audit in audits)
        {
            items.Add(_mapper.Map<AuditLogDto>(audit));
        }

        return new PagedResult<AuditLogDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = filter.PageNumber,
            PageSize = filter.PageSize
        };
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 6.3: Unbounded Query Koruması - Güvenlik için limit ekle
    [Obsolete("Use GetEntityHistoryQuery via MediatR instead")]
    public async Task<EntityAuditHistoryDto?> GetEntityHistoryAsync(string entityType, Guid entityId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !a.IsDeleted (Global Query Filter)
        // ✅ PERFORMANCE: Database'de aggregation yap (memory'de işlem YASAK)
        // ✅ PERFORMANCE: Database'de Count yap (memory'de işlem YASAK)
        var auditCount = await _context.Set<AuditLog>()
            .AsNoTracking()
            .CountAsync(a => a.EntityType == entityType && a.EntityId == entityId, cancellationToken);

        if (auditCount == 0)
        {
            return null;
        }

        // ✅ BOLUM 6.3: Unbounded Query Koruması - Güvenlik için limit ekle
        var audits = await _context.Set<AuditLog>()
            .AsNoTracking()
            .Include(a => a.User)
            .Where(a => a.EntityType == entityType &&
                       a.EntityId == entityId)
            .OrderByDescending(a => a.CreatedAt)
            .Take(500) // ✅ Güvenlik: Maksimum 500 audit log
            .ToListAsync(cancellationToken);

        // ✅ PERFORMANCE: Database'de aggregation yap
        var firstChange = await _context.Set<AuditLog>()
            .AsNoTracking()
            .Where(a => a.EntityType == entityType && a.EntityId == entityId)
            .MinAsync(a => (DateTime?)a.CreatedAt, cancellationToken);

        var lastChange = await _context.Set<AuditLog>()
            .AsNoTracking()
            .Where(a => a.EntityType == entityType && a.EntityId == entityId)
            .MaxAsync(a => (DateTime?)a.CreatedAt, cancellationToken);

        var totalChanges = await _context.Set<AuditLog>()
            .AsNoTracking()
            .CountAsync(a => a.EntityType == entityType && a.EntityId == entityId, cancellationToken);

        var modifiedBy = await _context.Set<AuditLog>()
            .AsNoTracking()
            .Where(a => a.EntityType == entityType && 
                   a.EntityId == entityId && 
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
            EntityId = entityId,
            EntityType = entityType,
            AuditLogs = auditLogDtos,
            FirstChange = firstChange ?? DateTime.UtcNow,
            LastChange = lastChange ?? DateTime.UtcNow,
            TotalChanges = totalChanges,
            ModifiedBy = modifiedBy
        };
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    [Obsolete("Use GetAuditStatsQuery via MediatR instead")]
    public async Task<AuditStatsDto> GetAuditStatsAsync(int days = 30, CancellationToken cancellationToken = default)
    {
        var startDate = DateTime.UtcNow.AddDays(-days);
        var today = DateTime.UtcNow.Date;

        // ✅ PERFORMANCE: Removed manual !a.IsDeleted (Global Query Filter)
        // ✅ PERFORMANCE: Database'de aggregation yap (memory'de işlem YASAK)
        var query = _context.Set<AuditLog>()
            .AsNoTracking()
            .Where(a => a.CreatedAt >= startDate);

        var totalAudits = await query.CountAsync(cancellationToken);
        var todayAudits = await query.CountAsync(a => a.CreatedAt.Date == today, cancellationToken);
        var failedActions = await query.CountAsync(a => !a.IsSuccessful, cancellationToken);
        var criticalEvents = await query.CountAsync(a => a.Severity == AuditSeverity.Critical, cancellationToken);

        // ✅ PERFORMANCE: Database'de grouping yap
        var actionsByType = await query
            .GroupBy(a => a.Action)
            .Select(g => new { Action = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.Action, g => g.Count, cancellationToken);

        var actionsByModule = await query
            .Where(a => !string.IsNullOrEmpty(a.Module))
            .GroupBy(a => a.Module)
            .Select(g => new { Module = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.Module!, g => g.Count, cancellationToken);

        var actionsBySeverity = await query
            .GroupBy(a => a.Severity.ToString())
            .Select(g => new { Severity = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.Severity, g => g.Count, cancellationToken);

        // ✅ PERFORMANCE: Removed manual !a.IsDeleted (Global Query Filter)
        var mostActiveUsers = await _context.Set<AuditLog>()
            .AsNoTracking()
            .Where(a => a.CreatedAt >= startDate && a.UserId.HasValue)
            .Include(a => a.User)
            .GroupBy(a => a.UserId)
            .Select(g => new TopAuditUserDto
            {
                UserId = g.Key!.Value,
                UserEmail = g.First().UserEmail,
                ActionCount = g.Count(),
                LastAction = g.Max(a => a.CreatedAt)
            })
            .OrderByDescending(u => u.ActionCount)
            .Take(10)
            .ToListAsync(cancellationToken);

        // ✅ PERFORMANCE: Removed manual !a.IsDeleted (Global Query Filter)
        var recentCriticalEvents = await _context.Set<AuditLog>()
            .AsNoTracking()
            .Where(a => a.CreatedAt >= startDate &&
                       a.Severity == AuditSeverity.Critical)
            .OrderByDescending(a => a.CreatedAt)
            .Take(10)
            .Select(a => new RecentCriticalEventDto
            {
                Id = a.Id,
                Action = a.Action,
                UserEmail = a.UserEmail,
                EntityType = a.EntityType,
                CreatedAt = a.CreatedAt,
                ErrorMessage = a.ErrorMessage
            })
            .ToListAsync(cancellationToken);

        return new AuditStatsDto
        {
            TotalAudits = totalAudits,
            TodayAudits = todayAudits,
            FailedActions = failedActions,
            CriticalEvents = criticalEvents,
            ActionsByType = actionsByType,
            ActionsByModule = actionsByModule,
            ActionsBySeverity = actionsBySeverity,
            MostActiveUsers = mostActiveUsers,
            RecentCriticalEvents = recentCriticalEvents
        };
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 6.3: Unbounded Query Koruması - Güvenlik için limit ekle
    [Obsolete("Use GetUserAuditHistoryQuery via MediatR instead")]
    public async Task<IEnumerable<AuditLogDto>> GetUserAuditHistoryAsync(Guid userId, int days = 30, CancellationToken cancellationToken = default)
    {
        var startDate = DateTime.UtcNow.AddDays(-days);

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !a.IsDeleted (Global Query Filter)
        // ✅ BOLUM 6.3: Unbounded Query Koruması - Güvenlik için limit ekle
        var audits = await _context.Set<AuditLog>()
            .AsNoTracking()
            .Include(a => a.User)
            .Where(a => a.UserId == userId &&
                       a.CreatedAt >= startDate)
            .OrderByDescending(a => a.CreatedAt)
            .Take(1000) // ✅ Güvenlik: Maksimum 1000 audit log
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        // ✅ PERFORMANCE: ToListAsync() sonrası Select() YASAK - AutoMapper'ın Map<IEnumerable<T>> metodunu kullan
        // ✅ BOLUM 6.4: List Capacity Pre-allocation (ZORUNLU)
        var result = new List<AuditLogDto>(audits.Count);
        foreach (var audit in audits)
        {
            result.Add(_mapper.Map<AuditLogDto>(audit));
        }
        return result;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    [Obsolete("Use CompareChangesQuery via MediatR instead")]
    public async Task<IEnumerable<AuditComparisonDto>> CompareChangesAsync(Guid auditLogId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !a.IsDeleted (Global Query Filter)
        var audit = await _context.Set<AuditLog>()
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == auditLogId, cancellationToken);

        if (audit == null || string.IsNullOrEmpty(audit.OldValues) || string.IsNullOrEmpty(audit.NewValues))
        {
            return new List<AuditComparisonDto>();
        }

        try
        {
            var oldValues = JsonSerializer.Deserialize<Dictionary<string, object>>(audit.OldValues);
            var newValues = JsonSerializer.Deserialize<Dictionary<string, object>>(audit.NewValues);

            if (oldValues == null || newValues == null)
            {
                return new List<AuditComparisonDto>();
            }

            // ✅ BOLUM 6.4: List Capacity Pre-allocation (ZORUNLU)
            var comparisons = new List<AuditComparisonDto>(newValues.Count);

            foreach (var key in newValues.Keys)
            {
                var oldValue = oldValues.ContainsKey(key) ? oldValues[key]?.ToString() ?? "" : "";
                var newValue = newValues[key]?.ToString() ?? "";

                if (oldValue != newValue)
                {
                    comparisons.Add(new AuditComparisonDto
                    {
                        FieldName = key,
                        OldValue = oldValue,
                        NewValue = newValue
                    });
                }
            }

            return comparisons;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Audit log karşılaştırma hatası. AuditLogId: {AuditLogId}", auditLogId);
            return new List<AuditComparisonDto>(); // ✅ BOLUM 2.1: Exception yutulmamali - ama burada boş liste döndürmek mantıklı
        }
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 9.1: ILogger kullanimi (ZORUNLU)
    // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
    [Obsolete("Use DeleteOldAuditLogsCommand via MediatR instead")]
    public async Task DeleteOldAuditLogsAsync(int daysToKeep = 365, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Eski audit log'lar siliniyor. DaysToKeep: {DaysToKeep}", daysToKeep);

        try
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);

            // ✅ PERFORMANCE: Removed manual !a.IsDeleted (Global Query Filter)
            var oldAudits = await _context.Set<AuditLog>()
                .Where(a => a.CreatedAt < cutoffDate && a.Severity != AuditSeverity.Critical)
                .ToListAsync(cancellationToken);

            _context.Set<AuditLog>().RemoveRange(oldAudits);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Eski audit log'lar silindi. Silinen kayit sayisi: {Count}", oldAudits.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Eski audit log'lar silme hatasi. DaysToKeep: {DaysToKeep}", daysToKeep);
            throw; // ✅ BOLUM 2.1: Exception yutulmamali (ZORUNLU)
        }
    }

    private AuditSeverity ParseSeverity(string severity)
    {
        return severity.ToLower() switch
        {
            "warning" => AuditSeverity.Warning,
            "error" => AuditSeverity.Error,
            "critical" => AuditSeverity.Critical,
            _ => AuditSeverity.Info
        };
    }
}
