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
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.User.Queries.SearchActivities;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class SearchActivitiesQueryHandler : IRequestHandler<SearchActivitiesQuery, IEnumerable<UserActivityLogDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<SearchActivitiesQueryHandler> _logger;
    private readonly PaginationSettings _paginationSettings;

    public SearchActivitiesQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<SearchActivitiesQueryHandler> logger,
        IOptions<PaginationSettings> paginationSettings)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _paginationSettings = paginationSettings.Value;
    }

    public async Task<IEnumerable<UserActivityLogDto>> Handle(SearchActivitiesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving filtered activities - Page: {PageNumber}, Size: {PageSize}", 
            request.Filter.PageNumber, request.Filter.PageSize);

        // ✅ BOLUM 12.0: Magic numbers configuration'dan alınıyor
        var pageSize = request.Filter.PageSize;
        if (pageSize > _paginationSettings.MaxPageSize) pageSize = _paginationSettings.MaxPageSize;
        if (pageSize < 1) pageSize = _paginationSettings.DefaultPageSize;

        var pageNumber = request.Filter.PageNumber;
        if (pageNumber < 1) pageNumber = 1;

        IQueryable<UserActivityLog> query = _context.Set<UserActivityLog>()
            .AsNoTracking()
            .Include(a => a.User)
            .AsQueryable();

        if (request.Filter.UserId.HasValue)
            query = query.Where(a => a.UserId == request.Filter.UserId.Value);

        if (!string.IsNullOrEmpty(request.Filter.ActivityType))
            query = query.Where(a => a.ActivityType == request.Filter.ActivityType);

        if (!string.IsNullOrEmpty(request.Filter.EntityType))
            query = query.Where(a => a.EntityType == request.Filter.EntityType);

        if (request.Filter.EntityId.HasValue)
            query = query.Where(a => a.EntityId == request.Filter.EntityId.Value);

        if (request.Filter.StartDate.HasValue)
            query = query.Where(a => a.CreatedAt >= request.Filter.StartDate.Value);

        if (request.Filter.EndDate.HasValue)
            query = query.Where(a => a.CreatedAt <= request.Filter.EndDate.Value);

        if (!string.IsNullOrEmpty(request.Filter.IpAddress))
            query = query.Where(a => a.IpAddress == request.Filter.IpAddress);

        if (!string.IsNullOrEmpty(request.Filter.DeviceType))
            query = query.Where(a => a.DeviceType == request.Filter.DeviceType);

        if (request.Filter.WasSuccessful.HasValue)
            query = query.Where(a => a.WasSuccessful == request.Filter.WasSuccessful.Value);

        var activities = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Retrieved {Count} filtered activities", activities.Count);

        return _mapper.Map<IEnumerable<UserActivityLogDto>>(activities);
    }
}
