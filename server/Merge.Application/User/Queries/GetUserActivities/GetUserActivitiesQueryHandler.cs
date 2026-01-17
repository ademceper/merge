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

namespace Merge.Application.User.Queries.GetUserActivities;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetUserActivitiesQueryHandler(IDbContext context, IMapper mapper, ILogger<GetUserActivitiesQueryHandler> logger, IOptions<UserSettings> userSettings) : IRequestHandler<GetUserActivitiesQuery, IEnumerable<UserActivityLogDto>>
{
    private readonly UserSettings config = userSettings.Value;

    public async Task<IEnumerable<UserActivityLogDto>> Handle(GetUserActivitiesQuery request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)

        logger.LogInformation("Retrieving activities for user: {UserId} for last {Days} days", request.UserId, request.Days);
        var days = request.Days;
        if (days > config.Activity.MaxDays) days = config.Activity.MaxDays;
        if (days < 1) days = config.Activity.DefaultDays;

        var startDate = DateTime.UtcNow.AddDays(-days);

        var activities =         // ✅ PERFORMANCE: AsNoTracking
        await context.Set<UserActivityLog>()
            .AsNoTracking()
            .Include(a => a.User)
            .Where(a => a.UserId == request.UserId && a.CreatedAt >= startDate)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);

        logger.LogInformation("Found {Count} activities for user: {UserId}", activities.Count, request.UserId);

        return mapper.Map<IEnumerable<UserActivityLogDto>>(activities);
    }
}
