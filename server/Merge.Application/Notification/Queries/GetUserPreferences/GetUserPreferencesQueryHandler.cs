using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using AutoMapper;
using Merge.Application.Common;
using Merge.Application.Configuration;
using Merge.Application.DTOs.Notification;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Notifications;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Notification.Queries.GetUserPreferences;


public class GetUserPreferencesQueryHandler(IDbContext context, IMapper mapper, IOptions<PaginationSettings> paginationSettings) : IRequestHandler<GetUserPreferencesQuery, PagedResult<NotificationPreferenceDto>>
{
    private readonly PaginationSettings paginationConfig = paginationSettings.Value;


    public async Task<PagedResult<NotificationPreferenceDto>> Handle(GetUserPreferencesQuery request, CancellationToken cancellationToken)
    {
        var pageSize = request.PageSize > paginationConfig.MaxPageSize 
            ? paginationConfig.MaxPageSize 
            : request.PageSize;
        var page = request.Page < 1 ? 1 : request.Page;

        IQueryable<NotificationPreference> query = context.Set<NotificationPreference>()
            .AsNoTracking()
            .Where(np => np.UserId == request.UserId)
            .OrderBy(np => np.NotificationType)
            .ThenBy(np => np.Channel);

        var totalCount = await query.CountAsync(cancellationToken);

        var preferences = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var preferenceDtos = mapper.Map<List<NotificationPreferenceDto>>(preferences);

        return new PagedResult<NotificationPreferenceDto>
        {
            Items = preferenceDtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}
