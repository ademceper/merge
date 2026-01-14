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

/// <summary>
/// Get User Notifications Query Handler - BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// BOLUM 3.4: Pagination (ZORUNLU)
/// </summary>
public class GetUserNotificationsQueryHandler : IRequestHandler<GetUserNotificationsQuery, PagedResult<NotificationDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly PaginationSettings _paginationSettings;

    public GetUserNotificationsQueryHandler(
        IDbContext context,
        IMapper mapper,
        IOptions<PaginationSettings> paginationSettings)
    {
        _context = context;
        _mapper = mapper;
        _paginationSettings = paginationSettings.Value;
    }

    public async Task<PagedResult<NotificationDto>> Handle(GetUserNotificationsQuery request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 12.0: Magic Numbers YASAK - Configuration kullan
        var pageSize = request.PageSize > _paginationSettings.MaxPageSize 
            ? _paginationSettings.MaxPageSize 
            : request.PageSize;
        var page = request.Page < 1 ? 1 : request.Page;

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !n.IsDeleted (Global Query Filter)
        IQueryable<NotificationEntity> query = _context.Set<NotificationEntity>()
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

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var notificationDtos = _mapper.Map<List<NotificationDto>>(notifications);

        return new PagedResult<NotificationDto>
        {
            Items = notificationDtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}
