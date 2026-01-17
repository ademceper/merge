using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AutoMapper;
using Merge.Application.DTOs.Security;
using Merge.Application.Interfaces;
using Merge.Application.Common;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Security.Queries.GetSecurityAlerts;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetSecurityAlertsQueryHandler(IDbContext context, IMapper mapper, ILogger<GetSecurityAlertsQueryHandler> logger, IOptions<PaginationSettings> paginationSettings) : IRequestHandler<GetSecurityAlertsQuery, PagedResult<SecurityAlertDto>>
{
    private readonly PaginationSettings paginationConfig = paginationSettings.Value;


    public async Task<PagedResult<SecurityAlertDto>> Handle(GetSecurityAlertsQuery request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation("Security alerts sorgulanıyor. UserId: {UserId}, Severity: {Severity}, Status: {Status}, Page: {Page}, PageSize: {PageSize}",
            request.UserId?.ToString() ?? "All", request.Severity ?? "All", request.Status ?? "All", request.Page, request.PageSize);

        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU) - ✅ BOLUM 12.0: Magic number config'den
        var pageSize = request.PageSize > paginationConfig.MaxPageSize ? paginationConfig.MaxPageSize : request.PageSize;
        var page = request.Page < 1 ? 1 : request.Page;

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !a.IsDeleted (Global Query Filter)
        // ✅ PERFORMANCE: AsSplitQuery - Multiple Include'lar için Cartesian Explosion önleme
        IQueryable<SecurityAlert> query = context.Set<SecurityAlert>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(a => a.User)
            .Include(a => a.AcknowledgedBy)
            .Include(a => a.ResolvedBy);

        if (request.UserId.HasValue)
        {
            query = query.Where(a => a.UserId == request.UserId.Value);
        }

        if (!string.IsNullOrEmpty(request.Severity))
        {
            if (Enum.TryParse<AlertSeverity>(request.Severity, true, out var severityEnum))
            {
                query = query.Where(a => a.Severity == severityEnum);
            }
        }

        if (!string.IsNullOrEmpty(request.Status))
        {
            if (Enum.TryParse<AlertStatus>(request.Status, true, out var statusEnum))
            {
                query = query.Where(a => a.Status == statusEnum);
            }
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var alerts = await query
            .OrderByDescending(a => a.Severity == AlertSeverity.Critical)
            .ThenByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var alertDtos = mapper.Map<IEnumerable<SecurityAlertDto>>(alerts).ToList();

        logger.LogInformation("Security alerts bulundu. TotalCount: {TotalCount}, Page: {Page}, PageSize: {PageSize}, ReturnedCount: {ReturnedCount}",
            totalCount, page, pageSize, alertDtos.Count);

        return new PagedResult<SecurityAlertDto>
        {
            Items = alertDtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}
