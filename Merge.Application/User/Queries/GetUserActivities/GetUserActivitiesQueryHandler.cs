using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AutoMapper;
using Merge.Application.Configuration;
using Merge.Application.DTOs.User;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.User.Queries.GetUserActivities;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetUserActivitiesQueryHandler : IRequestHandler<GetUserActivitiesQuery, IEnumerable<UserActivityLogDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetUserActivitiesQueryHandler> _logger;
    private readonly UserSettings _userSettings;

    public GetUserActivitiesQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetUserActivitiesQueryHandler> logger,
        IOptions<UserSettings> userSettings)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _userSettings = userSettings.Value;
    }

    public async Task<IEnumerable<UserActivityLogDto>> Handle(GetUserActivitiesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving activities for user: {UserId} for last {Days} days", request.UserId, request.Days);

        // ✅ BOLUM 12.0: Magic numbers configuration'dan alınıyor
        var days = request.Days;
        if (days > _userSettings.Activity.MaxDays) days = _userSettings.Activity.MaxDays;
        if (days < 1) days = _userSettings.Activity.DefaultDays;

        var startDate = DateTime.UtcNow.AddDays(-days);

        var activities = await _context.Set<UserActivityLog>()
            .AsNoTracking()
            .Include(a => a.User)
            .Where(a => a.UserId == request.UserId && a.CreatedAt >= startDate)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Found {Count} activities for user: {UserId}", activities.Count, request.UserId);

        return _mapper.Map<IEnumerable<UserActivityLogDto>>(activities);
    }
}
