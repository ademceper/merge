using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AutoMapper;
using Merge.Application.Common;
using Merge.Application.DTOs.Security;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using Merge.Domain.Enums;

namespace Merge.Application.Governance.Queries.SearchAuditLogs;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class SearchAuditLogsQueryHandler : IRequestHandler<SearchAuditLogsQuery, PagedResult<AuditLogDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<SearchAuditLogsQueryHandler> _logger;
    private readonly PaginationSettings _paginationSettings;

    public SearchAuditLogsQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<SearchAuditLogsQueryHandler> logger,
        IOptions<PaginationSettings> paginationSettings)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _paginationSettings = paginationSettings.Value;
    }

    public async Task<PagedResult<AuditLogDto>> Handle(SearchAuditLogsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Searching audit logs. PageNumber: {PageNumber}, PageSize: {PageSize}",
            request.PageNumber, request.PageSize);

        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        var pageSize = request.PageSize > _paginationSettings.MaxPageSize ? _paginationSettings.MaxPageSize : request.PageSize;
        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;

        // ✅ PERFORMANCE: Global Query Filter automatically filters !a.IsDeleted
        // ✅ FIX: Explicitly type as IQueryable to avoid IIncludableQueryable type mismatch
        IQueryable<AuditLog> query = _context.Set<AuditLog>()
            .AsNoTracking()
            .Include(a => a.User);

        if (request.UserId.HasValue)
            query = query.Where(a => a.UserId == request.UserId.Value);

        if (!string.IsNullOrEmpty(request.UserEmail))
            query = query.Where(a => a.UserEmail.Contains(request.UserEmail));

        if (!string.IsNullOrEmpty(request.Action))
            query = query.Where(a => a.Action == request.Action);

        if (!string.IsNullOrEmpty(request.EntityType))
            query = query.Where(a => a.EntityType == request.EntityType);

        if (request.EntityId.HasValue)
            query = query.Where(a => a.EntityId == request.EntityId.Value);

        if (!string.IsNullOrEmpty(request.TableName))
            query = query.Where(a => a.TableName == request.TableName);

        if (!string.IsNullOrEmpty(request.Severity))
        {
            var severity = ParseSeverity(request.Severity);
            query = query.Where(a => a.Severity == severity);
        }

        if (!string.IsNullOrEmpty(request.Module))
            query = query.Where(a => a.Module == request.Module);

        if (request.IsSuccessful.HasValue)
            query = query.Where(a => a.IsSuccessful == request.IsSuccessful.Value);

        if (request.StartDate.HasValue)
            query = query.Where(a => a.CreatedAt >= request.StartDate.Value);

        if (request.EndDate.HasValue)
            query = query.Where(a => a.CreatedAt <= request.EndDate.Value);

        if (!string.IsNullOrEmpty(request.IpAddress))
            query = query.Where(a => a.IpAddress == request.IpAddress);

        var totalCount = await query.CountAsync(cancellationToken);

        var audits = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
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
            Page = pageNumber,
            PageSize = pageSize
        };
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

