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

namespace Merge.Application.Security.Queries.GetUserSecurityEvents;

public class GetUserSecurityEventsQueryHandler(IDbContext context, IMapper mapper, ILogger<GetUserSecurityEventsQueryHandler> logger, IOptions<PaginationSettings> paginationSettings) : IRequestHandler<GetUserSecurityEventsQuery, PagedResult<AccountSecurityEventDto>>
{
    private readonly PaginationSettings paginationConfig = paginationSettings.Value;


    public async Task<PagedResult<AccountSecurityEventDto>> Handle(GetUserSecurityEventsQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("User security events sorgulanıyor. UserId: {UserId}, EventType: {EventType}, Page: {Page}, PageSize: {PageSize}",
            request.UserId, request.EventType ?? "All", request.Page, request.PageSize);

        var pageSize = request.PageSize > paginationConfig.MaxPageSize ? paginationConfig.MaxPageSize : request.PageSize;
        var page = request.Page < 1 ? 1 : request.Page;

        var query = context.Set<AccountSecurityEvent>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(e => e.User)
            .Include(e => e.ActionTakenBy)
            .Where(e => e.UserId == request.UserId);

        if (!string.IsNullOrEmpty(request.EventType))
        {
            if (Enum.TryParse<Merge.Domain.Enums.SecurityEventType>(request.EventType, true, out var eventTypeEnum))
            {
                query = query.Where(e => e.EventType == eventTypeEnum);
            }
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var events = await query
            .OrderByDescending(e => e.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var eventDtos = mapper.Map<IEnumerable<AccountSecurityEventDto>>(events).ToList();

        logger.LogInformation("User security events bulundu. UserId: {UserId}, TotalCount: {TotalCount}, Page: {Page}, PageSize: {PageSize}, ReturnedCount: {ReturnedCount}",
            request.UserId, totalCount, page, pageSize, eventDtos.Count);

        return new PagedResult<AccountSecurityEventDto>
        {
            Items = eventDtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}
