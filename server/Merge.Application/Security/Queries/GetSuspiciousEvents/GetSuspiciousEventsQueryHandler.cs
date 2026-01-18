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

namespace Merge.Application.Security.Queries.GetSuspiciousEvents;

public class GetSuspiciousEventsQueryHandler(IDbContext context, IMapper mapper, ILogger<GetSuspiciousEventsQueryHandler> logger, IOptions<PaginationSettings> paginationSettings) : IRequestHandler<GetSuspiciousEventsQuery, PagedResult<AccountSecurityEventDto>>
{
    private readonly PaginationSettings paginationConfig = paginationSettings.Value;


    public async Task<PagedResult<AccountSecurityEventDto>> Handle(GetSuspiciousEventsQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Suspicious security events sorgulanıyor. Page: {Page}, PageSize: {PageSize}",
            request.Page, request.PageSize);

        var pageSize = request.PageSize > paginationConfig.MaxPageSize ? paginationConfig.MaxPageSize : request.PageSize;
        var page = request.Page < 1 ? 1 : request.Page;

        var query = context.Set<AccountSecurityEvent>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(e => e.User)
            .Include(e => e.ActionTakenBy)
            .Where(e => e.IsSuspicious);

        var totalCount = await query.CountAsync(cancellationToken);

        var events = await query
            .OrderByDescending(e => e.Severity == Merge.Domain.Enums.SecurityEventSeverity.Critical)
            .ThenByDescending(e => e.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var eventDtos = mapper.Map<IEnumerable<AccountSecurityEventDto>>(events).ToList();

        logger.LogInformation("Suspicious security events bulundu. TotalCount: {TotalCount}, Page: {Page}, PageSize: {PageSize}, ReturnedCount: {ReturnedCount}",
            totalCount, page, pageSize, eventDtos.Count);

        return new PagedResult<AccountSecurityEventDto>
        {
            Items = eventDtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}
