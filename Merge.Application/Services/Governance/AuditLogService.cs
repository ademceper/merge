using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Governance;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Infrastructure.Data;
using Merge.Infrastructure.Repositories;
using System.Text.Json;
using Merge.Application.DTOs.Security;


namespace Merge.Application.Services.Governance;

public class AuditLogService : IAuditLogService
{
    private readonly ApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<AuditLogService> _logger;

    public AuditLogService(ApplicationDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<AuditLogService> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task LogAsync(CreateAuditLogDto auditDto, string ipAddress, string userAgent)
    {
        var audit = new AuditLog
        {
            UserId = auditDto.UserId,
            UserEmail = auditDto.UserEmail,
            Action = auditDto.Action,
            EntityType = auditDto.EntityType,
            EntityId = auditDto.EntityId,
            TableName = auditDto.TableName,
            PrimaryKey = auditDto.PrimaryKey,
            OldValues = auditDto.OldValues,
            NewValues = auditDto.NewValues,
            Changes = auditDto.Changes,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            Severity = ParseSeverity(auditDto.Severity),
            Module = auditDto.Module,
            IsSuccessful = auditDto.IsSuccessful,
            ErrorMessage = auditDto.ErrorMessage,
            AdditionalData = auditDto.AdditionalData ?? string.Empty
        };

        await _context.Set<AuditLog>().AddAsync(audit);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<AuditLogDto?> GetAuditByIdAsync(Guid id)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !a.IsDeleted (Global Query Filter)
        var audit = await _context.Set<AuditLog>()
            .AsNoTracking()
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (audit == null)
        {
            return null;
        }

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<AuditLogDto>(audit);
    }

    public async Task<IEnumerable<AuditLogDto>> GetAuditLogsAsync(AuditLogFilterDto filter)
    {
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

        var audits = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        // ✅ PERFORMANCE: ToListAsync() sonrası Select() YASAK - AutoMapper'ın Map<IEnumerable<T>> metodunu kullan
        return _mapper.Map<IEnumerable<AuditLogDto>>(audits);
    }

    public async Task<EntityAuditHistoryDto?> GetEntityHistoryAsync(string entityType, Guid entityId)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !a.IsDeleted (Global Query Filter)
        // ✅ PERFORMANCE: Database'de aggregation yap (memory'de işlem YASAK)
        // ✅ PERFORMANCE: Database'de Count yap (memory'de işlem YASAK)
        var auditCount = await _context.Set<AuditLog>()
            .AsNoTracking()
            .CountAsync(a => a.EntityType == entityType && a.EntityId == entityId);

        if (auditCount == 0)
        {
            return null;
        }

        var audits = await _context.Set<AuditLog>()
            .AsNoTracking()
            .Include(a => a.User)
            .Where(a => a.EntityType == entityType &&
                       a.EntityId == entityId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        // ✅ PERFORMANCE: Database'de aggregation yap
        var firstChange = await _context.Set<AuditLog>()
            .AsNoTracking()
            .Where(a => a.EntityType == entityType && a.EntityId == entityId)
            .MinAsync(a => (DateTime?)a.CreatedAt);

        var lastChange = await _context.Set<AuditLog>()
            .AsNoTracking()
            .Where(a => a.EntityType == entityType && a.EntityId == entityId)
            .MaxAsync(a => (DateTime?)a.CreatedAt);

        var totalChanges = await _context.Set<AuditLog>()
            .AsNoTracking()
            .CountAsync(a => a.EntityType == entityType && a.EntityId == entityId);

        var modifiedBy = await _context.Set<AuditLog>()
            .AsNoTracking()
            .Where(a => a.EntityType == entityType && 
                   a.EntityId == entityId && 
                   !string.IsNullOrEmpty(a.UserEmail))
            .Select(a => a.UserEmail)
            .Distinct()
            .ToListAsync();

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return new EntityAuditHistoryDto
        {
            EntityId = entityId,
            EntityType = entityType,
            AuditLogs = _mapper.Map<IEnumerable<AuditLogDto>>(audits).ToList(),
            FirstChange = firstChange ?? DateTime.UtcNow,
            LastChange = lastChange ?? DateTime.UtcNow,
            TotalChanges = totalChanges,
            ModifiedBy = modifiedBy
        };
    }

    public async Task<AuditStatsDto> GetAuditStatsAsync(int days = 30)
    {
        var startDate = DateTime.UtcNow.AddDays(-days);
        var today = DateTime.UtcNow.Date;

        // ✅ PERFORMANCE: Removed manual !a.IsDeleted (Global Query Filter)
        // ✅ PERFORMANCE: Database'de aggregation yap (memory'de işlem YASAK)
        var query = _context.Set<AuditLog>()
            .AsNoTracking()
            .Where(a => a.CreatedAt >= startDate);

        var totalAudits = await query.CountAsync();
        var todayAudits = await query.CountAsync(a => a.CreatedAt.Date == today);
        var failedActions = await query.CountAsync(a => !a.IsSuccessful);
        var criticalEvents = await query.CountAsync(a => a.Severity == AuditSeverity.Critical);

        // ✅ PERFORMANCE: Database'de grouping yap
        var actionsByType = await query
            .GroupBy(a => a.Action)
            .Select(g => new { Action = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.Action, g => g.Count);

        var actionsByModule = await query
            .Where(a => !string.IsNullOrEmpty(a.Module))
            .GroupBy(a => a.Module)
            .Select(g => new { Module = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.Module!, g => g.Count);

        var actionsBySeverity = await query
            .GroupBy(a => a.Severity.ToString())
            .Select(g => new { Severity = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.Severity, g => g.Count);

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
            .ToListAsync();

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
            .ToListAsync();

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

    public async Task<IEnumerable<AuditLogDto>> GetUserAuditHistoryAsync(Guid userId, int days = 30)
    {
        var startDate = DateTime.UtcNow.AddDays(-days);

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !a.IsDeleted (Global Query Filter)
        var audits = await _context.Set<AuditLog>()
            .AsNoTracking()
            .Include(a => a.User)
            .Where(a => a.UserId == userId &&
                       a.CreatedAt >= startDate)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        // ✅ PERFORMANCE: ToListAsync() sonrası Select() YASAK - AutoMapper'ın Map<IEnumerable<T>> metodunu kullan
        return _mapper.Map<IEnumerable<AuditLogDto>>(audits);
    }

    public async Task<IEnumerable<AuditComparisonDto>> CompareChangesAsync(Guid auditLogId)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !a.IsDeleted (Global Query Filter)
        var audit = await _context.Set<AuditLog>()
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == auditLogId);

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

            var comparisons = new List<AuditComparisonDto>();

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
        catch
        {
            return new List<AuditComparisonDto>();
        }
    }

    public async Task DeleteOldAuditLogsAsync(int daysToKeep = 365)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);

        // ✅ PERFORMANCE: Removed manual !a.IsDeleted (Global Query Filter)
        var oldAudits = await _context.Set<AuditLog>()
            .Where(a => a.CreatedAt < cutoffDate && a.Severity != AuditSeverity.Critical)
            .ToListAsync();

        _context.Set<AuditLog>().RemoveRange(oldAudits);
        await _unitOfWork.SaveChangesAsync();
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
