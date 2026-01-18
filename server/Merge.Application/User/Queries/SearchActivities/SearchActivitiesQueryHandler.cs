using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AutoMapper;
using Merge.Application.Configuration;
using Merge.Application.DTOs.User;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using IDbContext = Merge.Application.Interfaces.IDbContext;

namespace Merge.Application.User.Queries.SearchActivities;

public class SearchActivitiesQueryHandler(IDbContext context, IMapper mapper, ILogger<SearchActivitiesQueryHandler> logger, IOptions<PaginationSettings> paginationSettings) : IRequestHandler<SearchActivitiesQuery, IEnumerable<UserActivityLogDto>>
{
    public async Task<IEnumerable<UserActivityLogDto>> Handle(SearchActivitiesQuery request, CancellationToken cancellationToken)
    {

        logger.LogInformation("Retrieving filtered activities - Page: {PageNumber}, Size: {PageSize}", 
            request.Filter.PageNumber, request.Filter.PageSize);
        var pageSize = request.Filter.PageSize;
        if (pageSize > paginationSettings.Value.MaxPageSize) pageSize = paginationSettings.Value.MaxPageSize;
        if (pageSize < 1) pageSize = paginationSettings.Value.DefaultPageSize;

        var pageNumber = request.Filter.PageNumber;
        if (pageNumber < 1) pageNumber = 1;

        IQueryable<UserActivityLog> query = context.Set<UserActivityLog>()
            .AsNoTracking()
            .Include(a => a.User)
            .AsQueryable();

        if (request.Filter.UserId.HasValue)
            query = query.Where(a => a.UserId == request.Filter.UserId.Value);

        if (!string.IsNullOrEmpty(request.Filter.ActivityType) && 
            Enum.TryParse<ActivityType>(request.Filter.ActivityType, true, out var activityType))
            query = query.Where(a => a.ActivityType == activityType);

        if (!string.IsNullOrEmpty(request.Filter.EntityType) && 
            Enum.TryParse<EntityType>(request.Filter.EntityType, true, out var entityType))
            query = query.Where(a => a.EntityType == entityType);

        if (request.Filter.EntityId.HasValue)
            query = query.Where(a => a.EntityId == request.Filter.EntityId.Value);

        if (request.Filter.StartDate.HasValue)
            query = query.Where(a => a.CreatedAt >= request.Filter.StartDate.Value);

        if (request.Filter.EndDate.HasValue)
            query = query.Where(a => a.CreatedAt <= request.Filter.EndDate.Value);

        if (!string.IsNullOrEmpty(request.Filter.IpAddress))
            query = query.Where(a => a.IpAddress == request.Filter.IpAddress);

        if (!string.IsNullOrEmpty(request.Filter.DeviceType) && 
            Enum.TryParse<DeviceType>(request.Filter.DeviceType, true, out var deviceType))
            query = query.Where(a => a.DeviceType == deviceType);

        if (request.Filter.WasSuccessful.HasValue)
            query = query.Where(a => a.WasSuccessful == request.Filter.WasSuccessful.Value);

        var activities = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        logger.LogInformation("Retrieved {Count} filtered activities", activities.Count);

        return mapper.Map<IEnumerable<UserActivityLogDto>>(activities);
    }
}
