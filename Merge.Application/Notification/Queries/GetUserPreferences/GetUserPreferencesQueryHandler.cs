using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using AutoMapper;
using Merge.Application.Common;
using Merge.Application.Configuration;
using Merge.Application.DTOs.Notification;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;

namespace Merge.Application.Notification.Queries.GetUserPreferences;

/// <summary>
/// Get User Preferences Query Handler - BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// BOLUM 3.4: Pagination (ZORUNLU)
/// </summary>
public class GetUserPreferencesQueryHandler : IRequestHandler<GetUserPreferencesQuery, PagedResult<NotificationPreferenceDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly PaginationSettings _paginationSettings;

    public GetUserPreferencesQueryHandler(
        IDbContext context,
        IMapper mapper,
        IOptions<PaginationSettings> paginationSettings)
    {
        _context = context;
        _mapper = mapper;
        _paginationSettings = paginationSettings.Value;
    }

    public async Task<PagedResult<NotificationPreferenceDto>> Handle(GetUserPreferencesQuery request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 12.0: Magic Numbers YASAK - Configuration kullan
        var pageSize = request.PageSize > _paginationSettings.MaxPageSize 
            ? _paginationSettings.MaxPageSize 
            : request.PageSize;
        var page = request.Page < 1 ? 1 : request.Page;

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !np.IsDeleted (Global Query Filter)
        IQueryable<NotificationPreference> query = _context.Set<NotificationPreference>()
            .AsNoTracking()
            .Where(np => np.UserId == request.UserId)
            .OrderBy(np => np.NotificationType)
            .ThenBy(np => np.Channel);

        var totalCount = await query.CountAsync(cancellationToken);

        var preferences = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var preferenceDtos = _mapper.Map<List<NotificationPreferenceDto>>(preferences);

        return new PagedResult<NotificationPreferenceDto>
        {
            Items = preferenceDtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}
