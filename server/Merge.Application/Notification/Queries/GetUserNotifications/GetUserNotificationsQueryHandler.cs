using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using AutoMapper;
using Merge.Application.Common;
using Merge.Application.Configuration;
using Merge.Application.DTOs.Notification;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using NotificationEntity = Merge.Domain.Modules.Notifications.Notification;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Notifications;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Notification.Queries.GetUserNotifications;


public class GetUserNotificationsQueryHandler(IDbContext context, IMapper mapper, IOptions<PaginationSettings> paginationSettings) : IRequestHandler<GetUserNotificationsQuery, PagedResult<NotificationDto>>
{
    private readonly PaginationSettings paginationConfig = paginationSettings.Value;


    public async Task<PagedResult<NotificationDto>> Handle(GetUserNotificationsQuery request, CancellationToken cancellationToken)
    {
        var pageSize = request.PageSize > paginationConfig.MaxPageSize 
            ? paginationConfig.MaxPageSize 
            : request.PageSize;
        var page = request.Page < 1 ? 1 : request.Page;

        IQueryable<NotificationEntity> query = context.Set<NotificationEntity>()
            .AsNoTracking()
            .Where(n => n.UserId == request.UserId);

        if (request.UnreadOnly)
        {
            query = query.Where(n => !n.IsRead);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var notifications = await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var notificationDtos = mapper.Map<List<NotificationDto>>(notifications);

        return new PagedResult<NotificationDto>
        {
            Items = notificationDtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}
